#include "ChelaVm/Class.hpp"
#include "ChelaVm/Interface.hpp"
#include "ChelaVm/Structure.hpp"
#include "ChelaVm/Field.hpp"
#include "ChelaVm/Function.hpp"
#include "ChelaVm/FunctionGroup.hpp"
#include "ChelaVm/Property.hpp"
#include "ChelaVm/GenericInstance.hpp"
#include "ChelaVm/MemberInstance.hpp"
#include "ChelaVm/FunctionInstance.hpp"
#include "ChelaVm/TypeInstance.hpp"
#include "ChelaVm/VirtualMachine.hpp"
#include "ChelaVm/DebugInformation.hpp"
#include "llvm/Analysis/Verifier.h"

namespace ChelaVm
{
	Structure::Structure(Module *module)
		: ChelaType(module), declaringModule(module), genericPrototype(module)
	{
		baseStructure = NULL;
        boxIndex = 0;
		flags = MFL_Default;
		vtableVariable = NULL;
		isOpaque = false;
        declaredTables = false;
        setupVTables = NULL;
        vtableOffsetType = NULL;
        vtableType = NULL;
        structType = NULL;
        boxedStructType = NULL;
        typeInfo = NULL;
        isGenericPreload = false;
        genericInstance = NULL;
        genericTemplate = NULL;
        predeclared = false;
        declared = false;
        defined = false;
	}
	
	Structure::~Structure()
	{
        delete genericInstance;
	}
		
    Module *Structure::GetDeclaringModule() const
    {
        return declaringModule;
    }

	std::string Structure::GetName() const
	{
		return name;
	}
	
	MemberFlags Structure::GetFlags() const
	{
		return flags;
	}

	bool Structure::IsStructure() const
	{
		return true;
	}
	
	bool Structure::IsClass() const
	{
		return false;
	}

    bool Structure::IsInterface() const
    {
        return false;
    }

    bool Structure::IsAbstract() const
    {
        return ChelaType::IsAbstract() || IsInterface();
    }

    bool Structure::IsComplexStructure() const
    {
        if(!IsStructure())
            return false;

        // Predeclare myself.
        Structure *self = const_cast<Structure*> (this);
        self->PredeclarePass();

        // Is this structure complex?
        size_t startCount = 0;
        Structure *base =GetBaseStructure();
        if(base != NULL)
            startCount = base->fields.size();

        return (fields.size() - startCount) > 2;
    }
	
	size_t Structure::GetFieldCount() const
	{
		return fields.size();
	}
	
	Field *Structure::GetField(uint32_t id) const
	{
		if(id >= fields.size())
			Error("Invalid field id.");
		return fields[id];
	}

    Field *Structure::GetField(const std::string &name) const
    {
        for(size_t i = 0; i < fields.size(); i++)
        {
            Field *field = GetField(i);
            if(field->GetName() == name)
                return field;
        }
        
        return NULL;
    }

    bool Structure::HasSubReferences() const
    {
        return !valueRefSlots.empty();
    }

    size_t Structure::GetValueRefCount() const
    {
        return valueRefSlots.size();
    }

    Field *Structure::GetValueRefField(size_t index)
    {
        return valueRefSlots[index];
    }

    const Field *Structure::GetValueRefField(size_t index) const
    {
        return valueRefSlots[index];
    }

    FunctionGroup *Structure::GetFunctionGroup(const std::string &name) const
    {
        for(size_t i = 0; i < members.size(); ++i)
        {
            Member *member = members[i];
            if(member && member->IsFunctionGroup() && member->GetName() == name)
                return static_cast<FunctionGroup*> (member);
        }

        return NULL;
    }

    Property *Structure::GetProperty(const std::string &name) const
    {
        for(size_t i = 0; i < members.size(); ++i)
        {
            Member *member = members[i];
            if(member && member->IsProperty() && member->GetName() == name)
                return static_cast<Property*> (member);
        }

        return NULL;
    }

    size_t Structure::GetFieldStructureIndex(const std::string &name) const
    {
        for(size_t i = 0; i < fields.size(); i++)
        {
            Field *field = GetField(i);
            if(field->GetName() == name)
                return field->GetStructureIndex();
        }
        Error("Failed to get field: " + name);
        return 0;
    }
		
	size_t Structure::GetVMethodCount() const
	{
		return vmethods.size();
	}
	
	Function *Structure::GetVMethod(uint32_t id) const
	{
		if(id >= vmethods.size())
			Error("Invalid vmethod id.");
		return vmethods[id];
	}

    Structure *Structure::GetBaseStructure() const
    {
        if(baseStructure && baseStructure->IsTypeInstance())
        {
            TypeInstance *instance = static_cast<TypeInstance*> (baseStructure);
            return static_cast<Structure*> (instance->GetImplementation());
        }

        return static_cast<Structure*> (baseStructure);
    }

    Structure *Structure::GetInterface(size_t id) const
    {
        if(id >= interfaces.size())
            Error("Invalid base interface index.");

        // Use the type instance.
        Member *iface = interfaces[id];
        if(iface->IsTypeInstance())
        {
            TypeInstance *instance = static_cast<TypeInstance*> (iface);
            return static_cast<Structure*> (instance->GetImplementation());
        }

        return static_cast<Structure*> (iface);
    }

	bool Structure::IsDerivedFrom(const Structure *base) const
	{
		// Check all of the bases until hiting the tested one.
        Structure *currentBase = GetBaseStructure();
        while(currentBase != NULL)
        {
            // Compare the base.
            if(currentBase == base)
                return true;

            // Check the next base.
            currentBase = currentBase->GetBaseStructure();
        }

        return false;
	}

    bool Structure::Implements(const Structure *check) const
    {
        // Compare with all of the interfaces.
        for(size_t i = 0; i < interfaces.size(); i++)
        {
            // Read the interface.
            Structure *iface = GetInterface(i);
            if(iface)
            {
                if(iface == check || iface->Implements(check))
                   return true;
            }
        }

        // Check if the base implements the interface.
        Structure *currentBase = GetBaseStructure();
        if(currentBase != NULL)
            return currentBase->Implements(check);

        return false;
    }

    int Structure::GetITableIndex(const Structure *iface) const
    {
        // Only a linear search is needed.
        for(size_t i = 0; i < implementations.size(); i++)
        {
            const InterfaceImplementation &impl = implementations[i];
            if(impl.interface == iface)
                return impl.vtableField;
        }

        // Not found.
        return -1;
    }

	llvm::Type *Structure::GetTargetType() const
	{
        return GetStructType();
	}

    size_t Structure::GetSize() const
    {
        return GetModule()->GetTargetData()->getTypeAllocSize(GetTargetType());
    }

    size_t Structure::GetAlign() const
    {
        return GetModule()->GetTargetData()->getABITypeAlignment(GetTargetType());
    }

    void Structure::PredeclarePass()
    {
        // Only predeclare once.
        if(predeclared)
            return;
        predeclared = true;

        // Make sure the generic data has been loaded.
        FinishLoad();

        // Predeclare the base structure.
        Structure *base = GetBaseStructure();
        if(base)
            base->PredeclarePass();

        // This pass identifies the fields, required by IsComplexStructure()
        // Prepare the slots.
        PrepareSlots(this);
    }

	void Structure::DeclarePass()
	{
        // Only declare once.
        if(declared)
            return;
        declared = true;

        // Finish preload.
        if(isGenericPreload)
            FinishLoad();

        // Don't declare generic structures.
        if(IsGeneric())
            return;

		// Only declare if needed.
		if(structType != NULL && !isOpaque)
			return;

        // Declare the base instance.
        if(baseStructure)
            baseStructure->DeclarePass();

        // Declare the base structure.
        Structure *base = GetBaseStructure();
        if(base)
        {
            base->DeclarePass();

            // The base cannot be sealed.
            if(base->IsSealed())
                Error("cannot inherit from a sealed class.");

            // If the base is unsafe, I must be unsafe.
            if(!IsUnsafe() && base->IsUnsafe())
                Error("safe structure/class/interface with unsafe base.");
        }

        // Declare the interfaces.
        for(size_t i = 0; i < interfaces.size(); i++)
        {
            // Declare iface instance.
            Member *iface = interfaces[i];
            if(iface)
                iface->DeclarePass();

            // Declare the interface itself.
            iface = GetInterface(i);
            iface->DeclarePass();

            // If the base is unsafe, I must be unsafe.
            if(!IsUnsafe() && iface->IsUnsafe())
                Error("safe structure/class/interface with unsafe interface.");
        }

        // Perform predeclare pass.
        PredeclarePass();

        // Declare the type info.
        DeclareTypeInfo();

        // Reference and value types layouts are different.
        if(IsStructure())
            ValueDeclare();
        else
            ReferenceDeclare();

        // Declare the children.
        if(genericInstance || IsClosure())
        {
            for(size_t i = 0; i < members.size(); ++i)
            {
                Member *member = members[i];
                if(member)
                    member->DeclarePass();
            }
        }
	}

    void Structure::ValueDeclare()
    {
        // Check the base structure.
        Structure *base = GetBaseStructure();
        Structure *valueTypeClass = GetModule()->GetVirtualMachine()->GetValueTypeClass();
        if(!base || (base != valueTypeClass && !base->IsDerivedFrom(valueTypeClass)))
            Error("all of the structures must be derived from ValueType.");

        // Prepare the implementations.
        PrepareImplementations();

        // Check for complete interfaces.
        CheckCompleteness();

        // Create the opaque type for recursivity.
        GetStructType();
        GetBoxedType();

        // Prepare the slots.
        std::vector<llvm::Type*> layout;

        // Append structural layout.
        AppendLayout(layout, this, true);

        // Complete the structure type.
        structType->setBody(layout, false);

        // Create the boxed struct layout.
        std::vector<llvm::Type*> boxedLayout;

        // Add a pointer to the vtable into the structure.
        if(!vmethods.empty())
        {
            // Create the main vtable layout.
            CreateVTableLayout();
            boxedLayout.push_back(llvm::PointerType::getUnqual(GetVTableType()));
        }

        AppendLayout(boxedLayout, this, false);

        // Append the struct to the box.
        boxIndex = boxedLayout.size();
        boxedLayout.push_back(GetStructType());

        // Complete the boxed structure.
        boxedStructType->setBody(boxedLayout, false);

        // Unset the opaque flag.
        isOpaque = false;
    }

    void Structure::ReferenceDeclare()
    {
        // Prepare the implementations.
        PrepareImplementations();

        // Check for complete interfaces.
        CheckCompleteness();

        // Create the opaque type for recursivity.
        GetStructType();

        // Prepare the slots.
        std::vector<llvm::Type*> layout;

        // Add a pointer to the vtable into the structure.
        if(IsInterface() || !vmethods.empty())
        {
            // Create the main vtable layout.
            CreateVTableLayout();
            layout.push_back(llvm::PointerType::getUnqual(GetVTableType()));
        }

        // Append structural layout.
        AppendLayout(layout, this, false);

        // Complete the structure type.
        structType->setBody(layout, false);

        // Unset the opaque flag.
        isOpaque = false;
    }

    void Structure::AppendLayout(std::vector<llvm::Type*> &layout, Structure *building, bool valueLayout)
    {
        // Append first the parent.
        size_t start = 0;
        Structure *parent = building->GetBaseStructure();
        if(parent)
        {
            if(parent->IsStructure() == IsStructure() || !valueLayout)
                AppendLayout(layout, parent, valueLayout);
            start = parent->fields.size();
        }

        // Store the interface vtables.
        if(!valueLayout)
        {
            for(size_t i = 0; i < building->implementations.size(); i++)
            {
                InterfaceImplementation &org = building->implementations[i];
                if(!org.own)
                    continue;

                // Store the interface vtable offset and field index.
                org.vtableField = layout.size();

                // Append the interface vtable pointer.
                Structure *iface = org.interface;
                layout.push_back(llvm::PointerType::getUnqual(iface->GetVTableType()));
            }
        }

        // Don't add the struct fields when building the box.
        if(!valueLayout && building->IsStructure())
            return;

        // Store the fields.
        for(size_t i = start; i < building->fields.size(); i++)
        {
            // Get the slot field.
            Field *field = building->fields[i];

            // Make sure its a field.
            if(!field)
                throw ModuleException("Expected a field in " + GetFullName());

            // Set the field index.
            field->SetStructureIndex(layout.size());

            // Check the slot type.
            const ChelaType *fieldType = field->GetType();
            if(fieldType->IsReference() && valueLayout)
                valueRefSlots.push_back(field);

            // Store the slot type.
            layout.push_back(fieldType->GetTargetType());
        }
    }

	void Structure::PrepareSlots(Structure *building)
	{
		// First use the base structures, to allow overriding.
        Structure *buildingBase = building->GetBaseStructure();
    	if(buildingBase != NULL)
            PrepareSlots(buildingBase);

        // Also use the interfaces, only when I'm also an interface.
        if(IsInterface())
        {
            for(size_t i = 0; i < building->interfaces.size(); i++)
                PrepareSlots(building->GetInterface(i));
        }
		
		// Assign the members with their slots.
		for(size_t i = 0; i < building->members.size(); i++)
		{
			Member *member = building->members[i];;
			if(!member || member->IsStatic())
				continue;
				
			if(member->IsField())
			{
				Field *field = static_cast<Field*> (member);

				// Check if the field was already counted.
                bool counted = false;
                for(size_t i = 0; i < fields.size(); ++i)
                {
                    if(fields[i] == field)
                    {
                        counted = true;
                        break;
                    }
                }

                // Store the field.
                if(!counted)
                    fields.push_back(field);
			}
			else if(member->IsFunction())
			{
				Function *method = static_cast<Function*> (member);
                PrepareMethodSlot(method, method->GetMemberId());
			}
			else if(member->IsFunctionGroup())
			{
				// Cast the group.
				FunctionGroup *group = static_cast<FunctionGroup*> (member);
				
				// Iterate each one of the functions of the group.
				size_t numfunctions = group->GetFunctionCount();
				for(size_t i = 0; i < numfunctions; i++)
				{
					Function *method = group->GetFunction(i);
                    PrepareMethodSlot(method, group->GetFunctionId(i));
				}
			}
		}
	}

    void Structure::PrepareMethodSlot(Function *method, uint32_t methodId)
    {
        // Ignore static methods.
        if(method->IsStatic())
            return;

        // Ignore non virtual/override.
        if(!method->IsContract() && !method->IsAbstract() &&
           !method->IsVirtual() && !method->IsOverride())
            return;

        // Check the vtable slot.
        int vslot = method->GetVSlot();
        if(vslot < 0 || vslot >= (int)vmethods.size())
            Error("Invalid virtual/override method slot in " + GetFullName());

        // Check if actually overriding.
        if(vmethods[vslot] != NULL)
        {
            Function *oldMethod = vmethods[vslot];
            const FunctionType *oldMethodType = static_cast<const FunctionType*> (oldMethod->GetType());
            const FunctionType *methodType = static_cast<const FunctionType*> (method->GetType());
            if(!MatchFunctionType(oldMethodType, methodType, 1))
            {
                std::string error = "Incompatible virtual slot override ";
                error += method->GetFullName();
                error += " -> ";
                error += oldMethod->GetFullName();
                Error(error);
            }
            if(!method->IsOverride())
                throw ModuleException("Explicit virtual slot override required.");
        }

        vmethods[vslot] = method;
    }

    Member *Structure::GetActualMember(Member *member)
    {
        if(member->IsMemberInstance())
        {
            MemberInstance *instance = static_cast<MemberInstance*> (member);
            member = instance->GetActualMember();
            if(!member && genericInstance)
                member = instance->InstanceMember(NULL, GetModule(), GetCompleteGenericInstance());
            if(!member)
                Error("Invalid member");
        }

        return member;
    }

    inline Structure *Structure::GetActualInterface(Member *candidate)
    {
        candidate = GetActualMember(candidate);

        if(candidate->IsTypeInstance())
        {
            TypeInstance *instance = static_cast<TypeInstance*> (candidate);
            candidate = instance->GetImplementation();
        }

        if(!candidate->IsInterface())
            Error("Expected interface.");
        return static_cast<Structure*> (candidate);
    }

    Function *Structure::GetActualFunction(Member *candidate)
    {
        candidate = GetActualMember(candidate);

        if(candidate->IsFunctionInstance())
        {
            FunctionInstance *instance = static_cast<FunctionInstance*> (candidate);
            candidate = instance->GetImplementation();
        }

        if(!candidate->IsFunction())
            Error("Expected function.");
        return static_cast<Function*> (candidate);
    }

    void Structure::PrepareImplementations()
    {
        // Prepare first the parent implementations.
        Structure *base = GetBaseStructure();
        if(base)
        {
            // Copy the base implementations.
            for(size_t i = 0; i < base->implementations.size(); i++)
            {
                InterfaceImplementation impl = base->implementations[i];
                impl.own = false;
                implementations.push_back(impl);
            }

            // Add parent contracts.
            for(size_t i = 0; i < base->contracts.size(); ++i)
            {
                // Get the contract.
                ContractData &parentContract = base->contracts[i];

                // Get the actual declaration.
                Function *parentDecl = GetActualFunction(parentContract.declaration);

                // Find for better implementation.
                bool hasBetter = false;
                for(size_t j = 0; j < contracts.size(); ++j)
                {
                    // Get the contract.
                    ContractData &contract = contracts[i];

                    // Get the actual declaration.
                    Function *decl = GetActualFunction(contract.declaration);
                    if(parentDecl == decl)
                    {
                        // Found a better implementation.
                        hasBetter = true;
                        break;
                    }
                 }

                 // Use the parent contract only if I don't have a
                 // better implementation.
                 if(!hasBetter)
                    contracts.push_back(parentContract);
            }
        }

        // Create the new implementations.
        for(size_t i = 0; i < interfaces.size(); i++)
        {
            // Check all of the interfaces.
            Structure *interface = GetInterface(i);

             // Find an existing implementation.
            bool create = true;
            for(size_t i = 0; i < implementations.size(); i++)
            {
                InterfaceImplementation &impl = implementations[i];
                Structure *implemented = impl.interface;
                if(implemented == interface)
                {
                    // Only ignore complete coincidences.
                    create = false;
                }
            }

            // Create the implementation.
            if(create)
                ImplementInterface(interface, false);
        }

        // Connect the contract with the implementations.
        for(size_t i = 0; i < contracts.size(); i++)
        {
            // Get the contract.
            ContractData &contract = contracts[i];

            // Get the actual declaration and implementation.
            Function *decl = GetActualFunction(contract.declaration);
            Function *impl = GetActualFunction(contract.implementation);

            // Check compatibility.
            const FunctionType *declType = decl->GetFunctionType();
            const FunctionType *implType = impl->GetFunctionType();
            if(!MatchFunctionType(declType, implType, 1))
                Error("Incompatible contract implementation override.");

            // Update the interface implementations.
            Structure *interface = GetActualInterface(contract.interface);
            for(size_t i = 0; i < implementations.size(); i++)
            {
                // Get the implementation.
                InterfaceImplementation &ifaceImpl = implementations[i];

                // Ignore aliases.
                if(ifaceImpl.aliasIndex >= 0)
                    continue;

                // Check the interface for compatibility.
                Structure *derivedInterface = ifaceImpl.interface;
                if(derivedInterface != interface &&
                   !derivedInterface->Implements(interface))
                   continue;

                // Store the contract.
                ifaceImpl.implementations.push_back(contract);
            }
        }
    }

    void Structure::ImplementInterface(Structure *interface, bool checkAlias)
    {
        // Find an alias.
        int alias = -1;
        if(checkAlias)
        {
            // Find an existing implementation of the interface.
            for(size_t i = 0; i < implementations.size(); i++)
            {
                const InterfaceImplementation &old = implementations[i];
                if(old.interface == interface)
                {
                    alias = i;
                    break;
                }
            }
        }

        // Implement the interface itself.
        InterfaceImplementation impl;
        impl.interface = interface;
        impl.own = true;
        impl.aliasIndex = alias;
        implementations.push_back(impl);

        // Implement the parent of the interface.
        for(size_t i = 0; i < interface->interfaces.size(); i++)
            ImplementInterface(interface->GetInterface(i), true);
    }

    void Structure::CheckCompleteness()
    {
        // Find any abstract method.
        for(size_t i = 0; i < vmethods.size(); i++)
        {
            if(vmethods[i] == 0)
            {
                if(!IsAbstract())
                    Error("found pure abstract method.");
                return;
            }
        }

        // Check in the interface implementations.
        for(size_t i = 0; i < implementations.size(); i++)
        {
            InterfaceImplementation &impl = implementations[i];
            if(impl.aliasIndex >= 0)
                continue;

            // Allocate space for the slots.
            Structure *interface = impl.interface;
            impl.slots.resize(interface->vmethods.size());

            // Check all of the contract impementations.
            for(size_t j = 0; j < impl.implementations.size(); j++)
            {
                // Read the contract.
                ContractData &contract = impl.implementations[j];
                Function *decl = GetActualFunction(contract.declaration);

                // Find the matching declarations.
                for(size_t k = 0; k < interface->vmethods.size(); k++)
                {
                    if(interface->vmethods[k] == decl)
                    {
                        // Store the implementation.
                        impl.slots[k] = GetActualFunction(contract.implementation);
                        // There can't be duplicates.
                        // break;
                    }
                }
            }

            // Make sure that all of the slots have values.
            for(size_t j = 0; j < impl.slots.size(); j++)
            {
                if(impl.slots[j] == NULL)
                {
                    // Abstract class.
                    if(!IsAbstract())
                    {
                        if(interface->vmethods[j] != NULL)
                            Error("found abstract method " + interface->vmethods[j]->GetFullName()
                                + " in implementation of interface "
                                + interface->GetFullName() );
                        else
                            Error("found abstract method in implementation of interface "
                                + interface->GetFullName() );
                    }
                    return;
                }
            }
        }
    }

    void Structure::CreateVTableLayout()
    {
        if(!IsInterface() && vmethods.empty())
            return;

        // Create the vtable layout.
        std::vector<llvm::Type*> vtableLayout;

        // Add the class member.
        Module *module = GetModule();
        VirtualMachine *vm = module->GetVirtualMachine();
        llvm::Type *classType = llvm::PointerType::getUnqual(vm->GetTypeClass()->GetTargetType());
        vtableOffsetType = llvm::Type::getInt32Ty(GetLlvmContext());
        vtableLayout.push_back(classType);
        vtableLayout.push_back(vtableOffsetType);

        for(size_t i = 0; i < vmethods.size(); i++)
        {
            // Get the slot method.
            Function *method = vmethods[i];
            if(method == NULL)
                Error("Structure virtual method is unknown.");

            // Store the slot type.
            const ChelaType *functionType = method->GetType();
            llvm::Type *functionPointer = llvm::PointerType::getUnqual(functionType->GetTargetType());
            vtableLayout.push_back(functionPointer);
        }

        // Complete the vtable type.
        GetVTableType()->setBody(vtableLayout, false);
    }

    void Structure::DeclareVTable() const
    {
        if(vtableVariable)
            return;

        // Create the vtable variable.
        Module *module = GetModule();
        llvm::Module *targetModule = module->GetTargetModule();
        vtableVariable = new llvm::GlobalVariable(*targetModule, GetVTableType(), true, ComputeLinkage(true),
                                                    0, GetMangledName() + "_vt");
    }

    void Structure::DeclareTables() const
    {
        // Only declare once the tables.
        if(declaredTables)
            return;
        declaredTables = true;

        // Declare the vtable.
        DeclareVTable();

        // Declare the itables.
        llvm::Module *targetModule = GetModule()->GetTargetModule();
        for(size_t i = 0; i < implementations.size(); i++)
        {
            // Ignore implementations already declared.
            InterfaceImplementation &impl = implementations[i];
            if(impl.vtable)
                continue;

            // Create the name suffix.
            char suffix[32];
            sprintf(suffix, "_it%d", (int)i);

            // Declare the itable.
            Structure *interface = impl.interface;
            impl.vtable = new llvm::GlobalVariable(*targetModule,
                               interface->GetVTableType(), true,
                                ComputeLinkage(), 0, GetMangledName() + suffix);
        }
    }

	llvm::Constant *Structure::CreateVTable(Module *implModule, llvm::Constant *typeInfoVar, const std::string &prefix)
	{
        llvm::Module *targetModule = implModule->GetTargetModule();
        bool local = typeInfoVar == typeInfo;
		if(vmethods.empty())
        {
            if(local)
            {
			    return vtableVariable;
            }
            else
            {
                llvm::PointerType *int8PtrTy = llvm::Type::getInt8PtrTy(targetModule->getContext());
                return llvm::ConstantPointerNull::get(int8PtrTy);
            }
        }

        DeclareVTable();

		// Initialize the vtable variable.
		std::vector<llvm::Constant*> vtableValues;
		vtableValues.push_back(typeInfoVar);
        vtableValues.push_back(llvm::ConstantInt::get(vtableOffsetType, 0));
		
		// Create his value.
		for(size_t i = 0; i < vmethods.size(); i++)
		{
			// Load the method.
			Function *method = vmethods[i];

            // Treat different abstract/contract methods.
            if(method->IsAbstract() || method->IsContract())
            {
                // Get the function pointer type.
                const ChelaType *functionType = method->GetType();
                llvm::PointerType *functionPointer = llvm::PointerType::getUnqual(functionType->GetTargetType());

                // Set the abstract flag.
                if(!IsAbstract())
                    Error("found abstract method " + method->GetFullName() +
                        " in non abstract class");

                // Add a null pointer.
                vtableValues.push_back(llvm::ConstantPointerNull::get(functionPointer));
            }
            else
            {
    			// Declare it.
    			method->DeclarePass();

                // Choose the function.
                llvm::Constant *function;
                if(method->GetParent() == this && IsStructure())
                    function = CreateBoxTrampoline(method);
                else
                    function = method->ImportFunction(implModule);

    			// Store it in the vtable.
    			vtableValues.push_back(function);
            }
		}
		
		// Create the vtable value.
        llvm::Constant *vtableConstant = llvm::ConstantStruct::get(GetVTableType(), vtableValues);
        if(local)
        {
            vtableVariable->setInitializer(vtableConstant);
            return vtableVariable;
        }
        else
        {
            return new llvm::GlobalVariable(*targetModule, GetVTableType(), true, llvm::GlobalValue::LinkOnceODRLinkage,
                                                    vtableConstant, prefix + "_vt");
        }
	}

    llvm::Constant *Structure::CreateITable(InterfaceImplementation &impl, int index,
                                Module *implModule, llvm::Constant *typeInfoVar, const std::string &prefix)
    {
        // Get the interface type.
        Structure *interface = impl.interface;
        bool local = typeInfoVar == typeInfo;

        // Create the vtable variable.
        char suffix[32];
        sprintf(suffix, "_it%d", index);
        llvm::Module *targetModule = implModule->GetTargetModule();
        llvm::GlobalVariable *global = NULL;
        if(local)
        {
            if(!impl.vtable)
                impl.vtable = new llvm::GlobalVariable(*targetModule,
                               interface->GetVTableType(), true,
                               ComputeLinkage(true), 0, GetMangledName() + suffix);

            global = impl.vtable;
        }
        else
        {
            global = new llvm::GlobalVariable(*targetModule,
                               interface->GetVTableType(), true,
                               llvm::GlobalValue::LinkOnceODRLinkage, 0, prefix + suffix);
        }

        // Compute the vtable offset.
        llvm::Constant *vtableOffset = llvm::ConstantExpr::getOffsetOf(GetBoxedType(), impl.vtableField);
        vtableOffset = llvm::ConstantExpr::getIntegerCast(vtableOffset, vtableOffsetType, false);

        // Initialize it.
        std::vector<llvm::Constant*> vtableValues;
        vtableValues.push_back(typeInfoVar);
        vtableValues.push_back(vtableOffset);

        // Use the slots of the aliased implementation.
        InterfaceImplementation *usedImpl = &impl;
        if(impl.aliasIndex >= 0 && local)
        {
            // The alias index must be always less than my index.
            assert(impl.aliasIndex < index);

            // Copy the values from the aliased implementation.
            InterfaceImplementation &aliased = implementations[impl.aliasIndex];
            llvm::ConstantStruct *oldStruct = llvm::cast<llvm::ConstantStruct> (aliased.vtable->getInitializer());
            for(size_t i = 0; i < aliased.slots.size(); ++i)
                vtableValues.push_back(oldStruct->getOperand(i+2));
        }
        else if(impl.aliasIndex >= 0)
        {
            usedImpl = &implementations[impl.aliasIndex];
            assert(usedImpl->aliasIndex < 0);
        }

        // Append the vmethods.
        for(size_t i = 0; i < usedImpl->slots.size(); i++)
        {
            // Load the method.
            Function *method = usedImpl->slots[i];

            // Get the declaration type.
            Function *decl = interface->vmethods[i];
            llvm::PointerType *declPointerTy = llvm::PointerType::getUnqual(decl->GetFunctionType()->GetTargetType());

            // Make sure theres a method.
            if(!method)
            {
                // Use the declaration.
                method = interface->vmethods[i];
                if(!method)
                    Error("Expected method slot.");
            }

            // Use the latest override for abstract, virtual or override method.
            if(method->IsAbstract() || method->IsVirtual() || method->IsOverride())
                method = vmethods[method->GetVSlot()];

            // Treat different abstract/contract methods.
            if(method->IsAbstract() || method->IsContract())
            {
                // Set the abstract flag.
                if(!IsAbstract())
                    Error("found abstract method " + method->GetFullName() +
                        " in non abstract class");

                // Add a null pointer.
                vtableValues.push_back(llvm::ConstantPointerNull::get(declPointerTy));
            }
            else
            {
                // Declare it.
                method->DeclarePass();

                // Choose the function.
                llvm::Constant *function;
                if(method->GetParent() == this && IsStructure())
                    function = CreateBoxTrampoline(method);
                else
                    function = method->ImportFunction(implModule);

                // Cast the function type.
                function = llvm::ConstantExpr::getPointerCast(function, declPointerTy);

                // Store it in the vtable.
                vtableValues.push_back(function);
            }
        }

        // Create the vtable value.
        global->setInitializer(llvm::ConstantStruct::get(interface->GetVTableType(), vtableValues));
        return global;
    }

    void Structure::CreateTables()
    {
        // Don't create tables for interfaces.
        if(IsGeneric() || IsInterface())
            return;

        // Create the main vtable.
        Module *module = GetModule();
        CreateVTable(module, typeInfo);

        // Create the interface implementation vtables.
        for(size_t i = 0; i < implementations.size(); i++)
        {
            InterfaceImplementation &impl = implementations[i];

            // Create the implementation vtable.
            CreateITable(impl, i, module, typeInfo);
        }

        // Don't create the setup vtables for abstract objects.
        if(IsAbstract())
            return;

        // Create the setup vtables function type.
        std::vector<llvm::Type*> args;
        args.push_back(llvm::PointerType::getUnqual(GetBoxedType()));

        llvm::FunctionType *setupType = llvm::FunctionType::get(llvm::Type::getVoidTy(GetLlvmContext()), args, false);

        // Create the setup vtable function.
        llvm::Module *targetModule = module->GetTargetModule();
        setupVTables = llvm::Function::Create(setupType, ComputeLinkage(), GetMangledName() + "_vc", targetModule);

        // Get the first argument.
        llvm::Value *objectPointer = setupVTables->arg_begin();

        // Create the IR builder.
        llvm::IRBuilder<> builder(GetLlvmContext());

        // Create the only basic block.
        llvm::BasicBlock *bb = llvm::BasicBlock::Create(GetLlvmContext(), "vc", setupVTables);
        builder.SetInsertPoint(bb);

        // Set the main vtable.
        if(vtableVariable)
        {
            llvm::Value *vtableField = builder.CreateConstGEP2_32(objectPointer, 0, 0);
            builder.CreateStore(vtableVariable, vtableField);
        }

        // Set the interfaces vtable.
        for(size_t i = 0; i < implementations.size(); i++)
        {
            InterfaceImplementation *impl = &implementations[i];

            // Used the aliased implementation.
            if(!impl->vtable)
                continue;

            llvm::Value *vtableField = builder.CreateConstGEP2_32(objectPointer, 0, impl->vtableField);
            builder.CreateStore(impl->vtable, vtableField);
        }

        // Return void.
        builder.CreateRetVoid();

        // Verify the function.
        llvm::verifyFunction(*setupVTables);
    }

    llvm::Constant* Structure::CreateCustomVCtor(Module *implModule, llvm::Constant *vtable,
                                std::vector<llvm::Constant*> &itables, const std::string &prefix)
    {
        // Create the setup vtables function type.
        std::vector<llvm::Type*> args;
        args.push_back(llvm::PointerType::getUnqual(GetBoxedType()));

        llvm::FunctionType *setupType = llvm::FunctionType::get(llvm::Type::getVoidTy(GetLlvmContext()), args, false);

        // Create the setup vtable function.
        llvm::Module *targetModule = implModule->GetTargetModule();
        llvm::Function *customSetup = llvm::Function::Create(setupType, llvm::Function::LinkOnceODRLinkage, prefix + "_vc", targetModule);

        // Get the first argument.
        llvm::Value *objectPointer = customSetup->arg_begin();

        // Create the IR builder.
        llvm::IRBuilder<> builder(GetLlvmContext());

        // Create the only basic block.
        llvm::BasicBlock *bb = llvm::BasicBlock::Create(GetLlvmContext(), "vctor", customSetup);
        builder.SetInsertPoint(bb);

        // Set the main vtable.
        if(vtable)
        {
            llvm::Value *vtableField = builder.CreateConstGEP2_32(objectPointer, 0, 0);
            builder.CreateStore(vtable, vtableField);
        }

        // Set the interfaces vtable.
        for(size_t i = 0; i < implementations.size(); i++)
        {
            InterfaceImplementation *impl = &implementations[i];
            llvm::Value *vtableField = builder.CreateStructGEP(objectPointer, impl->vtableField);
            builder.CreateStore(itables[i], vtableField);
        }

        // Return void.
        builder.CreateRetVoid();

        // Verify the function.
        llvm::verifyFunction(*customSetup);
        return customSetup;
    }

    void Structure::DeclareTypeInfo() const
    {
        // Don't declare more than once.
        if(typeInfo)
            return;

        // Get the type class.
        Module *module = GetModule();
        Class *typeClass = module->GetVirtualMachine()->GetTypeClass();

        // Create the typeinfo variable.
        llvm::Module *targetModule = module->GetTargetModule();
        typeInfo = new llvm::GlobalVariable(*targetModule, typeClass->GetTargetType(),
                    false, ComputeLinkage(true), 0, GetMangledName() + "_typeinfo");
    }

    llvm::Function *Structure::CreateBoxTrampoline(Function *vfunction)
    {
        // Get the target function and his type.
        const FunctionType *functionType = static_cast<const FunctionType*> (vfunction->GetType());
        llvm::Function *targetFunction = vfunction->GetTarget();

        // Create the function.
        Module *module = GetModule();
        VirtualMachine *vm = GetVM();
        llvm::Module *targetModule = module->GetTargetModule();
        llvm::Function *trampoline =
            llvm::Function::Create(static_cast<llvm::FunctionType*> (functionType->GetTargetType()),
                llvm::Function::PrivateLinkage, vfunction->GetMangledName() + "_btramp",
                    targetModule);

        // Get the first argument.
        llvm::Value *objectPointer = trampoline->arg_begin();

        // Create the IR builder.
        llvm::IRBuilder<> builder(GetLlvmContext());

        // Create the only basic block.
        llvm::BasicBlock *bb = llvm::BasicBlock::Create(GetLlvmContext(), "tramp", trampoline);
        builder.SetInsertPoint(bb);

        // Cast the object pointer.
        objectPointer = builder.CreatePointerCast(objectPointer, llvm::PointerType::getUnqual(GetBoxedType()));

        // Adjust the function pointer.
        llvm::Value *structPointer = builder.CreateStructGEP(objectPointer, boxIndex);

        // Build the argument list.
        std::vector<llvm::Value*> args;
        args.push_back(structPointer);
        llvm::Function::arg_iterator it = ++trampoline->arg_begin();
        for(; it != trampoline->arg_end(); ++it)
            args.push_back(it);

        // Perform the tail call.
        llvm::CallInst *call = builder.CreateCall(targetFunction, args);
        call->setTailCall();

        // Return the value.
        if(functionType->GetReturnType() == ChelaType::GetVoidType(vm))
            builder.CreateRetVoid();
        else
            builder.CreateRet(call);

        // Return the trampoline.
        return trampoline;
    }

    void Structure::AddSubReferencesOffsets(llvm::Constant *baseOffset, const Structure *building, std::vector<llvm::Constant*> &dest)
    {
        // Iterate through all of the sub references.
        llvm::Type *sizeType = ChelaType::GetSizeType(GetVM())->GetTargetType();
        for(size_t i = 0; i < building->GetValueRefCount(); ++i)
        {
            // Get the field.
            const Field *subRefField = building->GetValueRefField(i);

            // Get the field offset.
            llvm::Constant *subOffset = llvm::ConstantExpr::getOffsetOf(building->GetStructType(), subRefField->GetStructureIndex());
            subOffset = llvm::ConstantExpr::getIntegerCast(subOffset, sizeType, false);
            llvm::Constant *fieldOffset = llvm::ConstantExpr::getAdd(baseOffset, subOffset);

            // Store the field reference.
            const ChelaType *fieldType = subRefField->GetType();
            if(fieldType->IsReference())
            {
                //This is a simple reference, store it verbatim.
                dest.push_back(baseOffset);
            }
            else if(fieldType->IsStructure())
            {
                // This is a structure with sub references.
                const Structure *subBuilding = static_cast<const Structure*> (fieldType);
                AddSubReferencesOffsets(fieldOffset, subBuilding, dest);
            }
            else
                Error("Unsupported ref counted field of type " + fieldType->GetFullName());
        }
    }

    void Structure::CreateTypeInfo()
    {
        // Get the type info data.
        Module *module = GetModule();
        ConstantStructurePtr typeInfoValue = GetTypeInfoData(module, typeInfo);

        // Initialize the type info.
        GetTypeInfo(module)->setInitializer(typeInfoValue->Finish());

        // Dump the type info.
        //typeInfo->dump();

        // Register the type info with the module.
        module->RegisterTypeInfo(GetFullName(), typeInfo);
    }

    struct CompareMemberInfo
    {
        typedef std::pair<std::string, llvm::Constant*> pair;
        bool operator()(const pair &a, const pair &b)
        {
            return a.first < b.first;
        }
    };

    ConstantStructurePtr Structure::GetTypeInfoData(Module *implModule, llvm::Constant *typeInfoVar, const std::string &prefix)
    {
        // Get the local module.
        Module *module = GetModule();
        bool hasReflection = module->HasReflection();

        // Is this a local type info?
        bool localTypeInfo = typeInfoVar == typeInfo;
        std::string dataPrefix = localTypeInfo ? GetMangledName() : prefix;

        // Important objects.
        VirtualMachine *vm = implModule->GetVirtualMachine();
        Class *typeClass = vm->GetTypeClass();
        llvm::Module *targetModule = implModule->GetTargetModule();

        // Some types used.
        llvm::PointerType *vtablePtrType = llvm::Type::getInt8PtrTy(targetModule->getContext());
        llvm::PointerType *int8PtrTy = llvm::Type::getInt8PtrTy(targetModule->getContext());
        llvm::Type *sizeType = ChelaType::GetSizeType(vm)->GetTargetType();
        llvm::Type *intType = ChelaType::GetIntType(vm)->GetTargetType();
        llvm::Type *int64Ty = llvm::Type::getInt64Ty(vm->getContext());
        llvm::Type *boxedType = GetBoxedType();
        llvm::Constant *null8 = llvm::ConstantPointerNull::get(int8PtrTy);

        // Get the vtable type.
        llvm::Type *vtableType = GetVTableType();
        if(IsGeneric())
            vtableType = boxedType = llvm::Type::getInt8Ty(targetModule->getContext());

        // Select the vtable.
        llvm::Constant *selectedVTable;
        if(IsInterface() || IsGeneric())
            selectedVTable = llvm::Constant::getNullValue(llvm::PointerType::getUnqual(vtableType));
        else if(localTypeInfo)
            selectedVTable = vtableVariable;
        else
        {
            // Make sure the original is defined.
            DefinitionPass();
            selectedVTable = CreateVTable(implModule, typeInfoVar, prefix);
        }

        // Initialize the implementations array.
        std::vector<llvm::Constant*> implArrayValues;
        std::vector<llvm::Constant*> implStructValues;
        std::vector<llvm::Constant*> selectedITables;
        for(size_t i = 0; i < implementations.size(); i++)
        {
            // Create a structure for each implementation.
            InterfaceImplementation *impl = &implementations[i];
            implStructValues.clear();

            // Store the interface type info.
            implStructValues.push_back(impl->interface->typeInfo);

            // Select the itable.
            llvm::Constant *itable = NULL;
            if(IsInterface() || IsGeneric())
                itable = llvm::Constant::getNullValue(llvm::PointerType::getUnqual(impl->interface->GetVTableType()));
            else if(localTypeInfo)
                itable = impl->vtable;
            else
                itable = CreateITable(*impl, i, implModule, typeInfoVar, prefix);

            // Store a pointer to the vtable.
            implStructValues.push_back(
                llvm::ConstantExpr::getPointerCast(itable, vtablePtrType));

            // Store the vtable offset.
            llvm::Constant *vtableOffset =
                llvm::ConstantExpr::getIntegerCast(
                    llvm::ConstantExpr::getOffsetOf(GetBoxedType(), impl->vtableField),
                    vtableOffsetType, false);
            implStructValues.push_back(vtableOffset);

            // Create the implementation structure.
            implArrayValues.push_back(llvm::ConstantStruct::get(implModule->GetInterfaceImplStruct(), implStructValues));
        }

        // Create the implementations array variable.
        llvm::ArrayType *implArrayType = llvm::ArrayType::get(module->GetInterfaceImplStruct(), implementations.size());
        llvm::Constant *implArrayConstant = llvm::ConstantArray::get(implArrayType, implArrayValues);
        llvm::Constant *implementationsGlobal = NULL;
        implementationsGlobal = new llvm::GlobalVariable(*targetModule, implArrayType,
                     true, llvm::GlobalValue::PrivateLinkage, implArrayConstant,
                     dataPrefix + "_implementations");

        // Create the references array.
        std::vector<llvm::Constant*> refsArrayValues;
        for(size_t i = 0; i < fields.size(); ++i)
        {
            // Ignore not ref counted fields.
            Field *field = GetField(i);
            field->DeclarePass();
            if(!field->IsRefCounted())
                continue;

            // Compute the field base offset.
            llvm::Constant *baseOffset = NULL;
            if(field->GetParent()->IsStructure())
            {
                baseOffset = llvm::ConstantExpr::getAdd(
                    llvm::ConstantExpr::getOffsetOf(GetBoxedType(), boxIndex),
                    llvm::ConstantExpr::getOffsetOf(GetStructType(), field->GetStructureIndex()));
            }
            else
            {
                baseOffset = llvm::ConstantExpr::getOffsetOf(GetBoxedType(), field->GetStructureIndex());
            }
            
            // Cast the offset.
            baseOffset = llvm::ConstantExpr::getIntegerCast(baseOffset, sizeType, false);

            // Get the field type.
            const ChelaType *fieldType = field->GetType();
            if(fieldType->IsReference())
            {
                //This is a simple reference, store it verbatim.
                refsArrayValues.push_back(baseOffset);
            }
            else if(fieldType->IsStructure())
            {
                // This is a structure with sub references.
                const Structure *subBuilding = static_cast<const Structure*> (fieldType);
                AddSubReferencesOffsets(baseOffset, subBuilding, refsArrayValues);
            }
            else
                throw ModuleException("Unsupported ref counted field of type " + fieldType->GetFullName());
        }

        // Create the references array.
        size_t totalSubReferences = refsArrayValues.size();
        llvm::ArrayType *refsArrayType = llvm::ArrayType::get(sizeType, refsArrayValues.size());
        llvm::Constant *refsArrayConstant = llvm::ConstantArray::get(refsArrayType, refsArrayValues);
        llvm::Constant *refsGlobal = new llvm::GlobalVariable(*targetModule, refsArrayType,
                        true, llvm::GlobalValue::PrivateLinkage, refsArrayConstant,
                        dataPrefix + "_refs");

        // Initialize the type info.
        ConstantStructurePtr typeInfoValue(typeClass->CreateConstant(implModule));

        // Setup MemberInfo
        SetMemberInfoData(typeInfoValue, implModule);

        // Set the type kind.
        TypeInfoKind kind;
        if(IsClass())
            kind = TIK_Class;
        else if(IsInterface())
            kind = TIK_Interface;
        else if(IsStructure())
            kind = TIK_Structure;
        else
            throw ModuleException("unknown structure type.");
        typeInfoValue->SetField("kind", llvm::ConstantInt::get(intType, (int)kind));

        // Compute the structure align and size.
        llvm::Constant *boxedAlign = llvm::ConstantExpr::getAlignOf(boxedType);
        llvm::Constant *boxedSize = llvm::ConstantExpr::getSizeOf(boxedType);

        // Make sure the size is a multiple of the alignment.
        boxedSize =
            llvm::ConstantExpr::getMul(
                llvm::ConstantExpr::getUDiv(
                    llvm::ConstantExpr::getAdd(boxedSize,
                        llvm::ConstantExpr::getSub(boxedAlign, llvm::ConstantInt::get(int64Ty, 1))),
                    boxedAlign),
                boxedAlign);
        
        // Store the structure size, align and data offset.
        typeInfoValue->SetField("size", boxedSize);
        typeInfoValue->SetField("align", boxedAlign);

        // Compute the data offset.
        llvm::Constant *dataOffset = NULL;
        if(IsGeneric())
            dataOffset = llvm::ConstantInt::get(intType, 0);
        else
            dataOffset = llvm::ConstantExpr::getOffsetOf(GetBoxedType(), GetBoxIndex());
        typeInfoValue->SetField("dataOffset", dataOffset);

        // When creating a non local type info, I'm the base.
        Structure *base = localTypeInfo ? GetBaseStructure() : this;

        // Store the base type info.
        llvm::Constant *baseTypeInfo = base != NULL ? base->typeInfo : null8;
        typeInfoValue->SetField("baseType", baseTypeInfo);

        // Store the main vtable.
        if(vtableVariable) // FIXME: interfaces bases.
            typeInfoValue->SetField("vtable", selectedVTable);

        // Store the implementations.
        typeInfoValue->SetField("numimplementations", llvm::ConstantInt::get(sizeType, implementations.size()));
        typeInfoValue->SetField("implementations", implementationsGlobal);

        // Store the setup vtables.
        if(!IsInterface())
        {
            if(localTypeInfo)
            {
                typeInfoValue->SetField("setupVTables", GetSetupVTables());
            }
            else
            {
                llvm::Constant *customSetup = CreateCustomVCtor(implModule, selectedVTable, selectedITables, prefix);
                typeInfoValue->SetField("setupVTables", customSetup);
            }
        }

        // Store the references.
        typeInfoValue->SetField("numreferences", llvm::ConstantInt::get(sizeType, totalSubReferences));
        typeInfoValue->SetField("references", refsGlobal);

        // Store the assembly.
        typeInfoValue->SetField("assembly", implModule->GetAssemblyVariable());

        // Store the fullname.
        typeInfoValue->SetField("fullName", implModule->CompileString(GetFullName()));

        // Store the namespace.
        Member *parentMember = GetParent();
        if(parentMember != NULL)
            typeInfoValue->SetField("namespaceName", implModule->CompileString(parentMember->GetFullName()));

        // Create the member information.
        if(localTypeInfo && hasReflection)
        {
            // Extract the member informations and sort them by name.
            std::vector<std::pair<std::string, llvm::Constant*> > memberInfos;
            for(size_t i = 0; i < this->members.size(); ++i)
            {
                // Get the child member.
                Member *member = this->members[i];
                if(!member)
                    continue;
    
                // Get the child member info.
                const std::string &memberName = member->GetName();
                llvm::GlobalVariable *memberInfoGlobal = member->GetMemberInfo();
                if(memberInfoGlobal)
                {
                    // Store themember info pointer.
                    memberInfos.push_back(std::make_pair(memberName, llvm::ConstantExpr::getPointerCast(memberInfoGlobal, int8PtrTy)));
                }
                else if(member->IsFunctionGroup())
                {
                    // Iterate through the group member.
                    FunctionGroup *group = static_cast<FunctionGroup*> (member);
                    for(size_t i = 0; i < group->GetFunctionCount(); ++i)
                    {
                        // Get the function member info.
                        Function *function = group->GetFunction(i);
                        if(function->IsGeneric())
                            continue;
                        memberInfoGlobal = function->GetMemberInfo();
    
                        // Store the member info.
                        if(memberInfoGlobal)
                            memberInfos.push_back(std::make_pair(memberName, llvm::ConstantExpr::getPointerCast(memberInfoGlobal, int8PtrTy)));
                    }
                }
            }

            if(!memberInfos.empty())
            {
                // Sort the member infos.
                std::sort(memberInfos.begin(), memberInfos.end(), CompareMemberInfo());
    
                // Copy the the sorted infos.
                size_t nummembers = memberInfos.size();
                std::vector<llvm::Constant*> sortedInfo;
                sortedInfo.reserve(nummembers);
                for(size_t i = 0; i < nummembers; ++i)
                    sortedInfo.push_back(memberInfos[i].second);
    
                // Store the type members.
                llvm::ArrayType *infoArrayType = llvm::ArrayType::get(int8PtrTy, nummembers);
                llvm::Constant *infoArray = llvm::ConstantArray::get(infoArrayType, sortedInfo);
                llvm::Constant *infoMembers = new llvm::GlobalVariable(*targetModule, infoArrayType, true,
                                llvm::GlobalValue::PrivateLinkage, infoArray, "_members_");

                // Store the children member pointers.
                typeInfoValue->SetField("nummembers", llvm::ConstantInt::get(sizeType, memberInfos.size()));
                typeInfoValue->SetField("members", infoMembers);
            }
        }

        // Get the associated primitive.
        const ChelaType *assocPrim = vm->GetAssociatedPrimitive(this);
        if(assocPrim != NULL)
        {
            // Encode the primitive, vector or matrix in the dimensions field.
            unsigned short dimensions = 0;
            llvm::Constant *lowerPrimitive = null8;
            if(assocPrim->IsVector())
            {
                // Cast into a vector type.
                const VectorType *vectorType = static_cast<const VectorType*> (assocPrim);

                // Encode the dimensions and store the element type.
                dimensions = vectorType->GetNumComponents();
                lowerPrimitive = GetReflectedType(vectorType->GetPrimitiveType());
            }
            else if(assocPrim->IsMatrix())
            {
                // Cast into a matrix type.
                const MatrixType *matrixType = static_cast<const MatrixType*> (assocPrim);

                // Encode the dimensions and store the element type.
                dimensions = (matrixType->GetNumRows() << 8) | matrixType->GetNumColumns();
                lowerPrimitive = GetReflectedType(matrixType->GetPrimitiveType());
            }
            else
            {
                lowerPrimitive = GetTypeInfo(implModule);
            }

            // Store the primitive pointer in a variable.
            lowerPrimitive = llvm::ConstantExpr::getPointerCast(lowerPrimitive, int8PtrTy);
            llvm::Constant *primitivePointer = new llvm::GlobalVariable(*targetModule, int8PtrTy, true,
                                llvm::GlobalValue::PrivateLinkage, lowerPrimitive, "_primitive_");

            // Store the subtypes and the dimensions.
            typeInfoValue->SetField("dimensions", llvm::ConstantInt::get(sizeType, dimensions));
            typeInfoValue->SetField("numsubtypes", llvm::ConstantInt::get(sizeType, 1));
            typeInfoValue->SetField("subtypes", primitivePointer);
        }

        // Store structure slots in subtypes, required by reflection invoke.
        if(IsStructure() && !fields.empty() && assocPrim == NULL)
        {
            size_t startField = 0;
            if(base != NULL)
                startField = base->fields.size();

            // Store the type of each one of the fields.
            std::vector<llvm::Constant*> subtypes;
            subtypes.reserve(fields.size() - startField);
            for(size_t i = startField; i <  fields.size(); ++i)
            {
                // Ignore static fields.
                Field *field = fields[i];
                if(field->IsStatic())
                    continue;

                // Store the subtype.
                llvm::Constant *subtype = llvm::ConstantExpr::getPointerCast(GetReflectedType(field->GetType()), int8PtrTy);
                subtypes.push_back(subtype);
            }

            // Get the number of fields.
            size_t numfields = subtypes.size();
            
            // Store the subtypes.
            llvm::ArrayType *subtypesArrayTy = llvm::ArrayType::get(int8PtrTy, numfields);
            llvm::Constant *subtypesConstant = llvm::ConstantArray::get(subtypesArrayTy, subtypes);
            llvm::Constant *subtypesArray = new llvm::GlobalVariable(*targetModule, subtypesArrayTy, true,
                                llvm::GlobalValue::PrivateLinkage, subtypesConstant, "_subtypes_");

            // Store the children member pointers.
            typeInfoValue->SetField("numsubtypes", llvm::ConstantInt::get(sizeType, numfields));
            typeInfoValue->SetField("subtypes", subtypesArray);
        }

        return typeInfoValue;
    }

    void Structure::DefinitionPass()
    {
        // Just in case declare.
        DeclarePass();

        // Don't define generic types.
        if(defined)
            return;

        // Set the defined flag.
        defined = true;

        // Get the module.
        Module *module = GetModule();

        // Define the base.
        Structure *base = GetBaseStructure();
        if(base && base->GetModule() == module)
            base->DefinitionPass();

        // Define the interfaces.
        for(size_t i = 0; i < interfaces.size(); ++i)
        {
            Structure *iface = GetInterface(i);
            if(iface && iface->GetModule() == module)
                iface->DefinitionPass();
        }

        // Optimize the pre-constructor.
        if(setupVTables)
            module->GetFunctionPassManager()->run(*setupVTables);

        // Create the dispatch tables.
        CreateTables();

        // Create the type info.
        CreateTypeInfo();

        // Required by custom attributes.
        DefineAttributes();

        // Declare the children.
        if(genericInstance || IsGeneric() || IsClosure())
        {
            for(size_t i = 0; i < members.size(); ++i)
            {
                Member *member = members[i];
                if(member)
                    member->DefinitionPass();
            }
        }
    }

    ConstantStructure *Structure::CreateConstant(Module *targetModule) const
    {
        // Declare the dispatch tables.
        DeclareTables();

        return new ConstantStructure(targetModule, this);
    }

	llvm::StructType *Structure::GetStructType() const
	{
        // Cannot get target type of generic.
        if(IsGeneric())
            Error("cannot get target type of generic.");

        // Target type is only null during the declare pass.
        if(structType == NULL)
        {
            structType = llvm::StructType::create(GetLlvmContext(), GetMangledName());
            isOpaque = true;
        }

        return structType;
	}

    size_t Structure::GetBoxIndex() const
    {
        return boxIndex;
    }

    llvm::StructType *Structure::GetBoxedType() const
    {
        if(IsGeneric())
            return NULL;

        if(!IsStructure())
            return GetStructType();

        if(!boxedStructType)
            boxedStructType = llvm::StructType::create(GetLlvmContext(), GetMangledName() + ".__box");
        return boxedStructType;
    }
	
	llvm::StructType *Structure::GetVTableType() const
	{
        if(IsGeneric())
            return NULL;

        if(!vtableType)
            vtableType = llvm::StructType::create(GetLlvmContext(), GetMangledName() + ".__vt");
		return vtableType;
	}
	
	llvm::GlobalVariable *Structure::GetVTable(Module *targetModule) const
	{
        if(IsGeneric())
            return NULL;

        if(!vtableVariable)
            DeclareVTable();
		return llvm::cast<llvm::GlobalVariable> (targetModule->ImportGlobal(vtableVariable));
	}

    llvm::GlobalVariable *Structure::GetTypeInfo(Module *targetModule) const
    {
        if(!typeInfo)
            DeclareTypeInfo();
        return llvm::cast<llvm::GlobalVariable> (targetModule->ImportGlobal(typeInfo));
    }

    llvm::Function *Structure::GetSetupVTables() const
    {
        return setupVTables;
    }

    Structure *Structure::PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header)
    {
        // Create the result.
        Structure *res = new Structure(module);

        // Store the name and the member flags.
        res->name = module->GetString(header.memberName);
        res->flags = (MemberFlags)header.memberFlags;

        // Skip the member data.
        reader.Skip(header.memberSize);
        return res;
    }

    void Structure::ReadStructure(ModuleReader &reader, const MemberHeader &header)
    {
        // Read the member attributes.
        SkipAttributes(reader, header.memberAttributes);

        // Get the module.
        Module *module = GetModule();

        // Skip the generic prototype.
        GenericPrototype::Skip(reader);

        // Read the base, number of interfaces, slots and virtual methods.
        uint32_t baseStructureId;
        uint16_t ifaceCount, slotCount, vslotCount;
        uint16_t contractCount, memberCount;
        reader >> baseStructureId >> ifaceCount >> slotCount >> vslotCount;
        reader >> contractCount >> memberCount;

        // Get the base structure.
        if(baseStructureId != 0)
        {
            baseStructure = module->GetMember(baseStructureId);
            if(!baseStructure->IsStructure() && !baseStructure->IsClass() &&
                !baseStructure->IsInterface() && !baseStructure->IsTypeInstance())
                Error("expected base structure/class/interface.");
        }

        // Make space for the fields and virtual methods.
        vmethods.resize(vslotCount, 0);

        // Read the base interfaces.
        for(int i = 0; i < ifaceCount; i++)
        {
            // Read the interface data and store it.
            uint32_t id;
            reader >> id;
            Member *iface = module->GetMember(id);
            if(!iface->IsInterface() && !iface->IsTypeInstance())
                Error("expected interface or type instance.");
            interfaces.push_back(iface);
        }

        // Read the structure contracts.
        for(int i = 0; i < contractCount; i++)
        {
            // Read the contract implementation and store it.
            uint32_t iface, decl, impl;
            reader >> iface >> decl >> impl;

            // Get the interface and implementation.
            ContractData contract;
            contract.interface = module->GetMember(iface);
            contract.declaration = module->GetMember(decl);
            contract.implementation = module->GetMember(impl);
            if(!contract.declaration)
                Error("expected contract declaration.");
            if(!contract.implementation)
                Error("expected contract implementation.");

            // Store the contract.
            contracts.push_back(contract);
        }

        // Read the structure members.
        for(int i = 0; i < memberCount; i++)
        {
            uint32_t id;
            reader >> id;
            Member *member = module->GetMember(id);
            members.push_back(member);

            // Update the member parent.
            if(member)
                member->UpdateParent(this);
        }
    }

    void Structure::Read(ModuleReader &reader, const MemberHeader &header)
	{
        // Read the member attributes.
        ReadAttributes(reader, header.memberAttributes);

        // Read the generic prototype.
        size_t start = reader.GetPosition();
        genericPrototype.Read(reader);

        // Skip the content.
        reader.Skip(start + header.memberSize - reader.GetPosition());
	}

    // Generic support
    const GenericPrototype *Structure::GetGenericPrototype() const
    {
        return &genericPrototype;
    }

    const GenericInstance *Structure::GetGenericInstanceData() const
    {
        return genericInstance;
    }

    bool Structure::IsGeneric() const
    {
        if(genericPrototype.GetPlaceHolderCount() != 0)
        {
            if(genericInstance == NULL || genericInstance->IsGeneric())
                return true;
        }

        return IsParentGeneric();
    }

    bool Structure::IsGenericType() const
    {
        return IsGeneric();
    }

    Structure *Structure::PreloadGeneric(Member *factory, Module *implementingModule, const GenericInstance *instance)
    {
        // Return this if I'm not generic
        if(!IsGeneric())
            return this;

        // Normalized the instance instance.
        instance = instance->Normalize();

        // Find an existing implementation in the using module.
        Structure *implTmpl = genericTemplate ? genericTemplate : this;
        Structure *impl = implementingModule->FindGenericImplementation(implTmpl, instance);
        if(impl)
        {
            delete instance;
            return impl;
        }

        // Create the new structure.
        impl = CreateBuilding(implementingModule);
        impl->declaringModule = GetDeclaringModule();
        impl->name = name;
        impl->flags = flags;
        impl->UpdateParent(factory);

        // Store the generic instance data.
        impl->genericInstance = instance->Normalize(GetGenericPrototype());
        impl->genericPrototype = implTmpl->genericPrototype;
        impl->genericTemplate = implTmpl;
        impl->isGenericPreload = true;

        // Register the implemenation
        implementingModule->RegisterGenericImplementation(implTmpl, impl, instance);

        // Preload subtypes.
        for(size_t i = 0; i < members.size(); ++i)
        {
            // Ignore null members.
            Member *member = members[i];
            if(!member)
                continue;

            // Ignore no-subtype member.
            if(!member->IsStructure() && !member->IsClass() && !member->IsInterface())
                continue;

            // Instance the member.
            Member *instanced = member->InstanceMember(impl, implementingModule, impl->GetCompleteGenericInstance());
            impl->members.push_back(instanced);
        }


        // Return the implementation.
        return impl;
    }

    void Structure::FinishLoad()
    {
        if(!isGenericPreload)
            return;

        // Unset the preload flag.
        isGenericPreload = false;

        // Get the module.
        Module *module = GetModule();

        // Use the complete generic instance.
        const GenericInstance *completeInstance = GetCompleteGenericInstance();

        // Instance the base.
        Structure *tmpl = genericTemplate;
        if(tmpl->baseStructure != NULL)
            baseStructure = tmpl->baseStructure->InstanceMember(module, completeInstance);

        // Instance the interfaces.
        for(size_t i = 0; i < tmpl->interfaces.size(); ++i)
        {
            Member *iface = tmpl->interfaces[i];
            iface = iface->InstanceMember(module, completeInstance);
            interfaces.push_back(iface);
        }

        // Make space for the fields and virtual methods.
        fields.resize(tmpl->fields.size(), 0);
        vmethods.resize(tmpl->vmethods.size(), 0);

        // Instance the members.
        for(size_t i = 0; i < tmpl->members.size(); ++i)
        {
            // Ignore null members.
            Member *member = tmpl->members[i];
            if(!member)
                continue;

            // Ignore subtype member
            if(member->IsStructure() || member->IsClass() || member->IsInterface())
                continue;

            // Instance the member.
            Member *instanced = member->InstanceMember(this, module, completeInstance);
            members.push_back(instanced);
        }

        // Instance the contracts.
        for(size_t i = 0; i < tmpl->contracts.size(); ++i)
        {
            const ContractData &old = tmpl->contracts[i];

            // Instance the contract.
            ContractData newContract;
            newContract.interface = old.interface->InstanceMember(module, completeInstance);
            newContract.declaration = old.declaration->InstanceMember(module, completeInstance);
            newContract.implementation = old.implementation->InstanceMember(module, completeInstance);

            // Store the instanced contract.
            contracts.push_back(newContract);
        }
    }

    Structure *Structure::InstanceGeneric(Module *implementingModule, const GenericInstance *instance)
    {
        // Avoid work.
        if(!IsGeneric())
            return this;

        Structure *tmpl = this;

        // Instance first the parent.
        if(IsParentGeneric())
        {
            // Make sure the parent its a class or a structure.
            Member *parentMember = GetParent();
            if(!parentMember->IsStructure() && !parentMember->IsClass())
                Error("Expected instantiable parent member.");

            // Instance the parent.
            Structure *parent = static_cast<Structure*> (parentMember);
            parent = parent->InstanceGeneric(implementingModule, instance);

            // Find my template member.
            Member *tmplMember = parent->GetActualMember(this);
            if(!tmplMember)
                Error("Couldn't find my own instance.");
            tmpl = static_cast<Structure*> (tmplMember);
        }

        // Preload the implementation
        return tmpl->PreloadGeneric(GetParent(), implementingModule, instance);
    }

    const ChelaType *Structure::InstanceGeneric(const GenericInstance *instance) const
    {
        // FIXME
        Structure *self = const_cast<Structure*> (this);
        return self->InstanceGeneric(instance->GetModule(), instance);
    }

    Member *Structure::InstanceMember(Member *factory, Module *implementingModule, const GenericInstance *instance)
    {
        return PreloadGeneric(factory, implementingModule, instance);
    }

    Member *Structure::GetTemplateMember() const
    {
        if(!genericTemplate)
            return (Member*)this;
        return genericTemplate;
    }

    Structure *Structure::CreateBuilding(Module *module) const
    {
        return new Structure(module);
    }

    Member *Structure::GetInstancedMember(Member *templateMember) const
    {
        // Finish load.
        //if(isGenericPreload)
        //    const_cast<Structure*> (this)->FinishLoad();
            
        // Find the instanced version of the member.
        for(size_t i = 0; i < members.size(); ++i)
        {
            Member *child = members[i];
            if(child)
            {
                if(child->GetTemplateMember() == templateMember)
                    return child;
                else if(child->IsFunctionGroup())
                {
                    // Check in the function groups.
                    FunctionGroup *fgroup = static_cast<FunctionGroup*> (child);
                    for(size_t j = 0; j < fgroup->GetFunctionCount(); ++j)
                    {
                        Function *function = fgroup->GetFunction(j);
                        if(function->GetTemplateMember() == templateMember)
                            return function;
                    }
                }
            }
        }

        return NULL;
    }

    llvm::DIType Structure::CreateDebugType(DebugInformation *context) const
    {
        // Use my own context for source line reading.
        DebugInformation *debugInfo = GetDeclaringModule()->GetDebugInformation();

        // Find an existing opaque type.
        llvm::DIType opaque = context->GetCachedDebugType(this);

        // Get the debug builder.
        llvm::DIBuilder &diBuilder = context->GetDebugBuilder();

        // If the target type is opaque, give back an opaque type.
        if(isOpaque)
        {
            if(opaque)
                return opaque;
            else if(IsClass() || IsInterface())
                return diBuilder.createClassType(llvm::DIScope(), GetName(), llvm::DIFile(), 0,
                                0, 0, 0, 0, llvm::DIType(), llvm::DIArray());
            else// if(IsStructure())
                return diBuilder.createStructType(llvm::DIScope(), GetName(), llvm::DIFile(), 0,
                                0, 0, 0, llvm::DIArray());
        }

        // Always try to produce the info.
        llvm::DIFile file = context->GetModuleFileDescriptor();
        int line = 0;

        // Get the structure info, for the position.
        StructureDebugInfo *buildingInfo = debugInfo ? debugInfo->GetStructureDebug(GetMemberId()) : NULL;
        if(buildingInfo)
        {
            const SourcePosition &pos = buildingInfo->GetPosition();
            file = context->GetFileDescriptor(pos.GetFileName());
            line = pos.GetLine();
        }

        // Get the target data.
        const llvm::TargetData *targetData = GetModule()->GetTargetData();

        // Get the scope descriptor.
        llvm::DIDescriptor scope = GetParent()->GetDebugNode(context);

        // Get the structure layout.
        const llvm::StructLayout *layout = targetData->getStructLayout(GetStructType());

        // First create an opaque type.
        size_t sizeInBits = layout->getSizeInBits();
        size_t alignInBits = layout->getAlignment()*8;
        size_t offsetInBits = 0;
        llvm::DIType oldOpaque = opaque;
        if(IsClass() || IsInterface())
            opaque = diBuilder.createClassType(scope, GetName(), file, line,
                                sizeInBits, alignInBits, offsetInBits, 0, llvm::DIType(), llvm::DIArray());
        else// if(IsStructure())
            opaque = diBuilder.createStructType(scope, GetName(), file, line,
                                sizeInBits, alignInBits, 0, llvm::DIArray());

        // Replace the old opaque, this is to prevent an stack overflow.
        if(oldOpaque)
        {
            context->ReplaceDebugType(this, opaque);
        }
        else
        {
            context->RegisterDebugType(this, opaque);
        }

        // Get the base type.
        llvm::DIType baseType;
        Structure *baseBuilding = NULL;
        if(!IsStructure())
        {
            baseBuilding = GetBaseStructure();
            if(baseBuilding != NULL)
                baseType = baseBuilding->GetDebugType(context);
        }

        // Build the members list.
        std::vector<llvm::Value*> elements;

        // Add the members.
        for(size_t i = 0; i < members.size(); ++i)
        {
            // Ignore null members.
            Member *member = members[i];
            if(!member)
                continue;

            // Ignore inherited members.
            if(member->GetParent() != this)
                continue;

            // Use the member debug node.
            if(member->IsStatic())
            {
                llvm::DIDescriptor desc = member->GetDebugNode(context);
                if(desc)
                    elements.push_back(desc);
                continue;
            }

            // The member file and line.
            llvm::DIFile memberFile = file;
            int memberLine = line;

            // Add fields.
            if(member->IsField())
            {
                // Cast the fields.
                Field *field = static_cast<Field*> (member);

                // Get the field debug info.
                FieldDebugInfo *fieldInfo = debugInfo ? debugInfo->GetFieldDebug(field->GetMemberId()) : NULL;
                if(fieldInfo)
                {
                    const SourcePosition &pos = fieldInfo->GetPosition();
                    file = context->GetFileDescriptor(pos.GetFileName());
                    line = pos.GetLine();
                }

                // Create the member type.
                llvm::Type *fieldType = field->GetType()->GetTargetType();
                size_t fieldSizeInBits = targetData->getTypeSizeInBits(fieldType);
                size_t fieldAlignInBits = targetData->getABITypeAlignment(fieldType)*8;
                size_t fieldOffsetInBits = layout->getElementOffsetInBits(field->GetStructureIndex());
                llvm::DIType memberType = diBuilder.createMemberType(scope, field->GetName(), memberFile, memberLine,
                                                fieldSizeInBits, fieldAlignInBits, fieldOffsetInBits,
                                                0, field->GetType()->GetDebugType(context));
                elements.push_back(memberType);
            }
            else if(member->IsFunctionGroup())
            {
            }
            else if(member->IsFunction())
            {
            }
        }

        // Create the members array.
        llvm::DIArray membersArray = diBuilder.getOrCreateArray(elements);

        // Now create the actual type.
        llvm::DIType type;
        if(IsClass() || IsInterface())
            type = diBuilder.createClassType(scope, GetName(), file, line,
                            sizeInBits, alignInBits, offsetInBits, 0, llvm::DIType(), membersArray);
        else
            type = diBuilder.createStructType(scope, GetName(), file, line,
                            sizeInBits, alignInBits, 0, membersArray);

        // Replace the opaque type.
        context->ReplaceDebugType(this, type);
        return type;
    }

    ConstantStructure::ConstantStructure(Module *targetModule, const Structure *building)
        : targetModule(targetModule), building(building)
    {
        // Get the struct type.
        llvm::StructType *structType = building->GetStructType();

        // Allocate space for the elements.
        elements.resize(structType->getNumElements());

        // Structures don't have vtables by itself.
        if(building->IsStructure())
            return;

        // Set the initial reference count.
        VirtualMachine *vm = targetModule->GetVirtualMachine();
        elements[1] = llvm::ConstantInt::get(ChelaType::GetUIntType(vm)->GetTargetType(), ConstantRefData);

        // Set the main vtable.
        if(building->vtableVariable)
            elements[0] = targetModule->ImportGlobal(building->vtableVariable);

        // Set the interfaces vtable.
        for(size_t i = 0; i < building->implementations.size(); i++)
        {
            const Structure::InterfaceImplementation *impl = &building->implementations[i];
            if(!impl->vtable)
                continue;

            elements[impl->vtableField] = targetModule->ImportGlobal(impl->vtable);
        }
    }

    ConstantStructure::~ConstantStructure()
    {
    }

    void ConstantStructure::SetField(const std::string &name, llvm::Constant *value)
    {
        assert((!building->IsStructure() || building->GetField(name)->GetParent() == building)
         && "Don't set inherited members in structures.");

        // Ignore null values.
        if(!value)
            return;

        // Find the element index.
        size_t elementIndex = building->GetFieldStructureIndex(name);

        // Get the field type.
        llvm::StructType *structType = building->GetStructType();
        llvm::Type *fieldType = structType->getElementType(elementIndex);
        llvm::Type *valueType = value->getType();

        // Convert the value.
        if(valueType != fieldType)
        {
            if(fieldType->isPointerTy() && valueType->isPointerTy())
                value = llvm::ConstantExpr::getPointerCast(value, fieldType);
            else if(fieldType->isIntegerTy() && valueType->isIntegerTy())
                value = llvm::ConstantExpr::getIntegerCast(value, fieldType, false);
            else if(fieldType->isFloatingPointTy() && valueType->isFloatingPointTy())
                value = llvm::ConstantExpr::getFPCast(value, fieldType);
            else
            {
                building->Error("Unsupported constant cast for compile time field " + name);
                return;
            }
        }

        // Store the field value.
        elements[elementIndex] = value;
    }

    llvm::Constant *ConstantStructure::Finish()
    {
        // Get the struct type.
        llvm::StructType *structType = building->GetStructType();

        // Fill undef values with null constants.
        for(size_t i = 0; i < elements.size(); ++i)
        {
            if(elements[i])
                continue;
            elements[i] = llvm::Constant::getNullValue(structType->getElementType(i));
        }

        // Create the constant structure.
        return llvm::ConstantStruct::get(structType, elements);
    }
}
