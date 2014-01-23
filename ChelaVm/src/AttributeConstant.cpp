#include "ChelaVm/AttributeConstant.hpp"
#include "ChelaVm/Class.hpp"
#include "ChelaVm/Function.hpp"
#include "ChelaVm/Field.hpp"
#include "ChelaVm/Property.hpp"

namespace ChelaVm
{
    // AttributePropertyValue implementation.
    AttributePropertyValue::AttributePropertyValue(Module *module)
        : module(module), value(module)
    {
    }

    AttributePropertyValue::~AttributePropertyValue()
    {
    }

    Member *AttributePropertyValue::GetVariable()
    {
        Member *member = module->GetMember(variableId);
        if(!member || !(member->IsField() || member->IsProperty()))
            throw ModuleException("invalid attribute property id.");
        return member;
    }

    ConstantValue &AttributePropertyValue::GetValue()
    {
        return value;
    }

    void AttributePropertyValue::Read(ModuleReader &reader)
    {
        // Read the variable id.
        reader >> variableId;
    
        // Read the variable value.
        value.Read(reader);
    }
    
    // AttributeConstant implementation.
    AttributeConstant::AttributeConstant(Module *module)
        : module(module)
    {
        attributeVariable = NULL;
        defined = false;
    }
    
    AttributeConstant::~AttributeConstant()
    {
    }

    Class *AttributeConstant::GetClass()
    {
        Member *classMember = module->GetMember(attributeClassId);
        if(!classMember || !classMember->IsClass())
            throw ModuleException("invalid attribute class id");
        return static_cast<Class*> (classMember);
    }

    Function *AttributeConstant::GetConstructor()
    {
        Member *ctorMember = module->GetMember(attributeCtorId);
        if(!ctorMember || !ctorMember->IsConstructor())
            throw ModuleException("invalid attribute constructor id");
        return static_cast<Function*> (ctorMember);
    }
    
    size_t AttributeConstant::GetArgumentCount()
    {
        return arguments.size();
    }

    ConstantValue *AttributeConstant::GetArgument(size_t id)
    {
        if(id < arguments.size())
            return &arguments[id];
        return NULL;
    }

    void AttributeConstant::Read(ModuleReader &reader)
    {
        // Read the attribute class, the constructor and the number of arguments.
        uint8_t numargs;
        reader >> attributeClassId >> attributeCtorId >> numargs;
    
        // Read each one of the arguments.
        arguments.resize(numargs, ConstantValue(module));
        for(size_t i = 0; i < numargs; ++i)
            arguments[i].Read(reader);
    
        // Read the property values.
        uint8_t numprops;
        reader >> numprops;
        propertyValues.resize(numprops, AttributePropertyValue(module));
        for(size_t i = 0; i < numprops; ++i)
            propertyValues[i].Read(reader);
    }

    llvm::GlobalVariable *AttributeConstant::DeclareVariable()
    {
        if(!attributeVariable)
        {
            // Get the target module and the attribute class.
            llvm::Module *targetModule = module->GetTargetModule();
            Class *attributeClass = GetClass();

            // Create the attribute variable.
            attributeVariable =
                new llvm::GlobalVariable(*targetModule, attributeClass->GetTargetType(),
                    false, llvm::GlobalVariable::PrivateLinkage, NULL, "_C_mattr");
        }

        return attributeVariable;
    }

    void AttributeConstant::DefineAttribute()
    {
        // Define the attribute value only once.
        if(defined || !attributeVariable)
            return;
        defined = true;

        // Create the initial value.
        Class *attributeClass = GetClass();
        ConstantStructurePtr attributeValue(attributeClass->CreateConstant(module));
        attributeVariable->setInitializer(attributeValue->Finish());

        // Register the attribute with the module.
        module->RegisterAttributeConstant(this);
    }

    llvm::Constant *AttributeConstant::CheckArgument(const ChelaType *argType, const ChelaType *valueType, llvm::Constant *value)
    {
        if(argType != valueType)
        {
            // Handle boxes and enumerations.
            if(argType->IsStructure() && valueType->IsFirstClass())
            {
                // Cast the structure.
                const Structure *building = static_cast<const Structure*> (argType);

                // Wrap transparently structures with one element.
                llvm::StructType *structTy = building->GetStructType();
                if(structTy->getNumElements() != 1)
                    throw ModuleException("invalid attribute argument type.");

                // Check if wrapping is possible.
                llvm::Type *boxType = *structTy->element_begin();
                if(boxType != valueType->GetTargetType())
                    throw ModuleException("cannot wrap attribute constant.");

                // Wrap the constant.
                std::vector<llvm::Constant*> elements;
                elements.push_back(value);
                value = llvm::ConstantStruct::get(structTy, elements);
            }
            else
                throw ModuleException("incompatible attribute argument type.");
        }

        // Return the constant.
        return value;
    }

    void AttributeConstant::Construct(llvm::IRBuilder<> &builder)
    {
        // Get the attribute class and the constructor.
        Class *attributeClass = GetClass();
        Function *attributeCtor = GetConstructor();

        // Make sure the attribute constructoris the one of the class.
        if(attributeCtor->GetParent() != attributeClass)
            throw ModuleException("incorrect attribute constructor.");

        // TODO: Check the constructor visibility.

        // Store the arguments.
        std::vector<llvm::Value *> args;
        args.push_back(attributeVariable);

        // Check the argument count.
        const FunctionType *functionType = attributeCtor->GetFunctionType();
        if(functionType->GetArgumentCount() != arguments.size()+1)
            throw ModuleException("incompatible argument count.");

        // Store and check the arguments.
        for(size_t i = 0; i < arguments.size(); ++i)
        {
            // Check and store the argument.
            ConstantValue &argVal = arguments[i];
            const ChelaType *argType = functionType->GetArgument(i+1);
            const ChelaType *valueType = argVal.GetChelaType();
            args.push_back(CheckArgument(argType, valueType, argVal.GetTargetConstant()));
        }

        // Construct the attribute.
        builder.CreateCall(attributeCtor->ImportFunction(module), args);

        // Now set the named properties.
        for(size_t i = 0; i < propertyValues.size(); ++i)
        {
            // Get key-value pair.
            AttributePropertyValue &propVal = propertyValues[i];

            // Get the property.
            Member *variable = propVal.GetVariable();
            ConstantValue &value = propVal.GetValue();
            const ChelaType *valueType = value.GetChelaType();

            if(variable->IsProperty())
            {
                // Get the property.
                Property *property = static_cast<Property*> (variable);
                if(property->IsStatic())
                    throw ModuleException("cannot set static attribute property.");

                // Get the setter.
                Function *setter = property->GetSetAccessor();
                if(!setter)
                    throw ModuleException("cannot set readonly attribute property.");

                // Prepare the value.
                llvm::Value *newValue = CheckArgument(property->GetType(), valueType, value.GetTargetConstant());

                // Store the property arguments.
                args.clear();
                args.push_back(attributeVariable);
                args.push_back(newValue);

                // Set the property.
                builder.CreateCall(setter->ImportFunction(module), args);
            }
            else if(variable->IsField())
            {
                // Get the field pointer.
                Field *field = static_cast<Field*> (variable);
                llvm::Value *fieldPtr = builder.CreateStructGEP(attributeVariable, field->GetStructureIndex());

                // Prepare the value.
                llvm::Value *newValue = CheckArgument(field->GetType(), valueType, value.GetTargetConstant());

                // Store the value.
                builder.CreateStore(newValue, fieldPtr);
            }
            else
                throw ModuleException("unexpected property type.");
        }
    }
}

