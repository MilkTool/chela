#include "ChelaVm/AttributeConstant.hpp"
#include "ChelaVm/DebugInformation.hpp"
#include "ChelaVm/Member.hpp"
#include "ChelaVm/Class.hpp"
#include "ChelaVm/VirtualMachine.hpp"
#include "ChelaVm/GenericInstance.hpp"
#include "llvm/Constants.h"
#include "llvm/ADT/SmallString.h"
#include "llvm/Support/raw_ostream.h"

namespace ChelaVm
{
    Member::Member(Module *module)
        : virtualMachine(module->GetVirtualMachine()), module(module)
    {
        parent = NULL;
        attributeList = NULL;
        memberId = 0;
        completeGenericPrototype = NULL;
        completeGenericInstance = NULL;
    }

    Member::Member(VirtualMachine *virtualMachine)
        : virtualMachine(virtualMachine), module(NULL)
    {
        parent = NULL;
        attributeList = NULL;
        memberId = 0;
        completeGenericPrototype = NULL;
        completeGenericInstance = NULL;
    }

    Member::~Member()
    {
        // Delete the complete generic prototype.
        delete completeGenericPrototype;

        // Delete the attributes.
        for(size_t i = 0; i < attributes.size(); ++i)
            delete attributes[i];
    }

    Module *Member::GetModule() const
    {
        return module;
    }

    Module *Member::GetDeclaringModule() const
    {
        return GetModule();
    }

    MemberFlags Member::GetFlags() const
    {
        return MFL_Default;
    }
    
    std::string Member::GetName() const
    {
        return "";
    }
    
    std::string Member::GetFullName() const
    {
        // Use the raw name as full name for cdecl.
        if(IsCdecl())
            return GetName();

        // The buffer to construct the full name.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Add the parent name.
        if(parent)
        {
            const std::string &pname = parent->GetFullName();
            if(!pname.empty())
                out << pname << '.';
        }

        // Add my name.
        out << GetName();

        // Add the generic data.
        const GenericInstance *instance = GetGenericInstanceData();
        if(instance && instance->GetArgumentCount() != 0)
        {
            out << instance->GetFullName();
        }
        else
        {
            // Add the generic prototype data.
            const GenericPrototype *prototype = GetGenericPrototype();
            if(prototype)
                out << prototype->GetFullName();
        }

        return out.str();
    }

    std::string Member::GetMangledName() const
    {
        // Use the raw name as mangled name for cdecl.
        if(IsCdecl())
            return GetName();

        // The buffer to construct the mangled name.
        llvm::SmallString<64> buffer;
        llvm::raw_svector_ostream out(buffer);

        // Compute the member prefix.
        const char *typePrefix = "U";
        if(IsField())
        {
            if(IsStatic())
                typePrefix = "g";
            else
                typePrefix = "f";
        }
        else if(IsFunction())
        {
            if(IsStatic())
                typePrefix = "F";
            else if(IsStaticConstructor())
                typePrefix = "C";
            else
                typePrefix = "M";
        }
        else if(IsProperty())
            typePrefix = "P";
        else if(IsStructure())
            typePrefix = "S";
        else if(IsInterface())
            typePrefix = "I";
        else if(IsClass())
            typePrefix = "C";
        else if(IsNamespace())
            typePrefix = "N";

        // Add the parent and module name.
        Module *declModule = GetDeclaringModule();
        if(parent)
        {
            const std::string &pname = parent->GetFullName();
            if(!pname.empty())
                out << parent->GetMangledName();
            else if(declModule)
                    out << declModule->GetMangledName();
        }
        else if(declModule)
            out << declModule->GetMangledName();

        // Sanitize the name.
        llvm::SmallString<64> nameBuffer;
        const std::string &name = GetName();
        for(size_t i = 0; i < name.size(); ++i)
        {
            char c = name[i];
            if(c == '.')
                nameBuffer += "_d_";
            else if(c == '<')
                nameBuffer += "_l_";
            else if(c == '>')
                nameBuffer += "_g_";
            else
                nameBuffer.push_back(c);
        }
        
        // Add the name prefixed by the member type and name size.
        out << typePrefix << (int)nameBuffer.size() << nameBuffer;

        const GenericInstance *instance = GetGenericInstanceData();
        if(instance && instance->GetArgumentCount() != 0)
        {
            out << instance->GetMangledName();
        }
        else
        {
            const GenericPrototype *prototype = GetGenericPrototype();
            if(prototype)
                out << prototype->GetMangledName();
        }

        return out.str();
    }

    void Member::SetMemberId(uint32_t id)
    {
        memberId = id;
    }

    uint32_t Member::GetMemberId() const
    {
        return memberId;
    }

    void Member::UpdateParent(Member *parent)
    {
        this->parent = parent;
    }
    
    Member *Member::GetParent()
    {
        return parent;
    }

    const Member *Member::GetParent() const
    {
        return parent;
    }

    bool Member::IsAnonymous() const
    {
        return false;
    }

    bool Member::IsType() const
    {
        return false;
    }

    bool Member::IsTypeGroup() const
    {
        return false;
    }
    
    bool Member::IsNamespace() const
    {
        return false;
    }
    
    bool Member::IsStructure() const
    {
        return false;
    }
    
    bool Member::IsClass() const
    {
        return false;
    }

    bool Member::IsClosure() const
    {
        return false;
    }

    bool Member::IsInterface() const
    {
        return false;
    }
    
    bool Member::IsFunction() const
    {
        return false;
    }

    bool Member::IsFunctionInstance() const
    {
        return false;
    }
    
    bool Member::IsFunctionGroup() const
    {
        return false;
    }

    bool Member::IsGeneric() const
    {
        return false;
    }

    bool Member::IsGenericBased() const
    {
        // Check for generic.
        bool res = IsGeneric();
        if(res)
            return true;

        // Check the generic instance arguments.
        const GenericInstance *instance = GetGenericInstanceData();
        if(instance && instance->GetArgumentCount() != 0)
            return true;

        return false;
    }

    bool Member::IsField() const
    {
        return false;
    }

    bool Member::IsMemberInstance() const
    {
        return false;
    }

    bool Member::IsProperty() const
    {
        return false;
    }

    bool Member::IsTypeInstance() const
    {
        return false;
    }
    
    bool Member::IsPublic() const
    {
        return (GetFlags() & MFL_VisibilityMask) == MFL_Public;
    }
    
    bool Member::IsInternal() const
    {
        return (GetFlags() & MFL_VisibilityMask) == MFL_Internal;
    }
    
    bool Member::IsProtected() const
    {
        return (GetFlags() & MFL_VisibilityMask) == MFL_Protected;
    }

    bool Member::IsProtectedInternal() const
    {
        return (GetFlags() & MFL_VisibilityMask) == MFL_ProtectedInternal;
    }

    bool Member::IsPrivate() const
    {
        return (GetFlags() & MFL_VisibilityMask) == MFL_Private;
    }
    
    bool Member::IsExtern() const
    {
        return (GetFlags() & MFL_LinkageMask) == MFL_External;
    }
    
    bool Member::IsCdecl() const
    {
        return (GetFlags() & MFL_LanguageMask) == MFL_Cdecl;
    }

    bool Member::IsRuntime() const
    {
        return (GetFlags() & MFL_LanguageMask) == MFL_Runtime;
    }

    bool Member::IsKernel() const
    {
        return (GetFlags() & MFL_LanguageMask) == MFL_Kernel;
    }

    bool Member::IsStatic() const
    {
        return (GetFlags() & MFL_InstanceMask) == MFL_Static;
    }

    bool Member::IsAbstract() const
    {
        return (GetFlags() & MFL_InstanceMask) == MFL_Abstract;
    }

    bool Member::IsContract() const
    {
        return (GetFlags() & MFL_InstanceMask) == MFL_Contract;
    }

    bool Member::IsVirtual() const
    {
        return (GetFlags() & MFL_InstanceMask) == MFL_Virtual;
    }
    
    bool Member::IsOverride() const
    {
        return (GetFlags() & MFL_InstanceMask) == MFL_Override;
    }

    bool Member::IsConstructor() const
    {
        return (GetFlags() & MFL_InstanceMask) == MFL_Constructor;
    }

    bool Member::IsStaticConstructor() const
    {
        return (GetFlags() & MFL_InstanceMask) == MFL_StaticConstructor;
    }

    bool Member::IsSealed() const
    {
        return (GetFlags() & MFL_InheritanceMask) == MFL_Sealed;
    }

    bool Member::IsUnsafe() const
    {
        return (GetFlags() & MFL_SecurityMask) == MFL_Unsafe;
    }

    bool Member::IsReadOnly() const
    {
        return (GetFlags() & MFL_AccessMask) == MFL_ReadOnly;
    }

    bool Member::IsParentGeneric() const
    {
        if(parent)
        {
            if(parent->IsGeneric())
                return true;

            // TODO: Check if I can cache the result.
            return parent->IsParentGeneric(); // Delayed check.
        }

        return false;
    }

    void Member::ReadStructure(ModuleReader &reader, const MemberHeader &header)
    {
        // Skip the attributes.
        SkipAttributes(reader, header.memberAttributes);

        // Skip the member data.
        reader.Skip(header.memberSize);
    }

    void Member::Read(ModuleReader &reader, const MemberHeader &header)
    {
        // Skip the attributes.
        SkipAttributes(reader, header.memberAttributes);

        // Skip the member data.
        reader.Skip(header.memberSize);
    }

    void Member::DeclarePass()
    {
    }
    
    void Member::DefinitionPass()
    {
    }

    const GenericInstance *Member::GetGenericInstanceData() const
    {
        return NULL;
    }

    const GenericPrototype *Member::GetGenericPrototype() const
    {
        return NULL;
    }

    const GenericPrototype *Member::GetCompleteGenericPrototype() const
    {
        // Return the cached prototype.
        if(completeGenericPrototype != NULL)
            return completeGenericPrototype;

        // Get my prototype.
        const GenericPrototype *myPrototype = GetGenericPrototype();

        // Use or append the parent complete generic prototype.
        const Member *parent = GetParent();
        if(parent)
        {
            const GenericPrototype *parentProto = parent->GetCompleteGenericPrototype();
            if(parentProto != NULL && parentProto->GetPlaceHolderCount() != 0)
            {
                // Append the parent prototype.
                completeGenericPrototype = new GenericPrototype(GetModule());
                completeGenericPrototype->AppendPrototype(parentProto);
            }
        }

        // Append my prototype.
        if(myPrototype != NULL && myPrototype->GetPlaceHolderCount() != 0)
        {
            if(completeGenericPrototype == NULL)
                completeGenericPrototype = new GenericPrototype(GetModule());
            completeGenericPrototype->AppendPrototype(myPrototype);
        }

        // Return the complete prototype.
        return completeGenericPrototype;
    }

    const GenericInstance *Member::GetCompleteGenericInstance() const
    {
        // Return the cached instance.
        if(completeGenericInstance != NULL)
            return completeGenericInstance;

        // Get the complete generic prototype.
        const GenericPrototype *completeProto = GetCompleteGenericPrototype();
        if(!completeProto || completeProto->GetPlaceHolderCount() == 0)
            return NULL;

        // Get my generic instance.
        const GenericInstance *myInstance = GetGenericInstanceData();

        // Use or append the parent complete generic instance.
        const Member *parent = GetParent();
        if(parent)
        {
            const GenericInstance *parentInstance = parent->GetCompleteGenericInstance();
            if(parentInstance != NULL && parentInstance->GetArgumentCount() != 0)
            {
                // Append the parent instance.
                completeGenericInstance = new GenericInstance(GetModule());
                completeGenericInstance->SetPrototype(completeProto);
                completeGenericInstance->AppendInstance(parentInstance);
            }
        }

        // Append my instance.
        if(myInstance != NULL && myInstance->GetArgumentCount() != 0)
        {
            if(completeGenericInstance == NULL)
            {
                completeGenericInstance = new GenericInstance(GetModule());
                completeGenericInstance->SetPrototype(completeProto);
            }
            completeGenericInstance->AppendInstance(myInstance);
        }

        // Return the instance
        return completeGenericInstance;
    }

    Member *Member::InstanceMember(Module *implementingModule,
                                   const GenericInstance *instance)
    {
        return InstanceMember(GetParent(), implementingModule, instance);
    }

    Member *Member::InstanceMember(Member *factory, Module *implementingModule,
                           const GenericInstance *instance)
    {
        Error("unimplemented member instancing.");
        return NULL;
    }

    Member *Member::GetTemplateMember() const
    {
        return NULL;
    }

    llvm::DIDescriptor Member::GetDebugNode(DebugInformation *context) const
    {
        if(parent)
            return parent->GetDebugNode(context);
        return context->GetModuleFileDescriptor();
    }

    llvm::GlobalVariable *Member::GetMemberInfo()
    {
        return NULL;
    }

    llvm::Constant *Member::GetMemberInfo(Module *import)
    {
        llvm::GlobalVariable *info = GetMemberInfo();
        if(info == NULL || import == GetModule())
            return info;
        return import->ImportGlobal(info);
    }

    void Member::SetMemberInfoData(ConstantStructurePtr &info, Module *implModule)
    {
        // Use my module as the default implementation module.
        Module *module = GetModule();
        if(implModule == NULL)
            implModule = module;

        // Get the member info class.
        VirtualMachine *vm = implModule->GetVirtualMachine();
        llvm::Module *targetModule = implModule->GetTargetModule();

        // Get some types.
        llvm::Type *intTy = llvm::Type::getInt32Ty(targetModule->getContext());

        // Set the parent member.
        llvm::Constant *parentMember = parent ? parent->GetMemberInfo() : NULL;
        info->SetField("parent", parentMember);

        // Find the declaring type.
        Member *currentParent = parent;
        Structure *declaringType = NULL;
        while(currentParent != NULL)
        {
            // Is the parent a type?
            if(currentParent->IsStructure() || currentParent->IsInterface() || currentParent->IsClass())
            {
                declaringType = static_cast<Structure*> (currentParent);
                break;
            }

            // Check the next parent
            currentParent = currentParent->parent;
        }

        // Set the declaring type.
        if(declaringType != NULL)
            info->SetField("declaringType", declaringType->GetTypeInfo(module));

        // Set the member name.
        info->SetField("name", implModule->CompileCString(GetName()));

        // Set the number of attributes.
        info->SetField("numCustomAttributes", llvm::ConstantInt::get(intTy, attributes.size()));

        // Set the custom attributes
        if(implModule == module)
        {
            info->SetField("customAttributes", DeclareAttributes());
        }
        else if(!attributes.empty())
        {
            llvm::Constant *customAttributes = targetModule->getOrInsertGlobal(GetMangledName() + "_attrlist_", llvm::Type::getInt8PtrTy(vm->getContext()));
            info->SetField("customAttributes", customAttributes);
        }
    }

    llvm::Constant *Member::GetReflectedType(const ChelaType *type) const
    {
        return GetModule()->GetTypeInfo(type);
    }

    void Member::Error(const std::string &message) const
    {
        throw ModuleException("Error in '" + GetFullName() + "': " + message);
    }

    void Member::Warning(const std::string &message) const
    {
        fprintf(stderr, "Warning in '%s': %s", GetFullName().c_str(), message.c_str());
    }

    void Member::ReadAttributes(ModuleReader &reader, size_t numattributes)
    {
        // Read each one of the attributes.
        attributes.reserve(numattributes);
        for(size_t i = 0; i < numattributes; ++i)
        {
            // Create the attribute
            AttributeConstant *attr = new AttributeConstant(GetModule());
            attributes.push_back(attr);

            // Read it.
            attr->Read(reader);
        }
    }

    void Member::SkipAttributes(ModuleReader &reader, size_t numattributes)
    {
        for(size_t i = 0; i < numattributes; ++i)
        {
            AttributeConstant attr(GetModule());
            attr.Read(reader);
        }
    }

    AttributeConstant *Member::GetCustomAttribute(Structure *attribute) const
    {
        for(size_t i = 0; i < attributes.size(); ++i)
        {
            AttributeConstant *attr = attributes[i];
            if(attr->GetClass() == attribute)
                return attr;
        }
        
        return NULL;
    }

    llvm::Constant *Member::DeclareAttributes()
    {
        Module *module = GetModule();
        VirtualMachine *vm = module->GetVirtualMachine();
        llvm::Module *targetModule = module->GetTargetModule();

        // Use i8* pointers.
        llvm::LLVMContext &context = targetModule->getContext();
        //llvm::Type *int8Ty = llvm::Type::getInt8Ty(context);
        llvm::PointerType *int8PtrTy = llvm::Type::getInt8PtrTy(context);

        // Create the attribute list.
        if(attributeList == NULL && !attributes.empty())
        {
            std::vector<llvm::Constant*> attributePointers;

            // Declare and store the attributes.
            for(size_t i = 0; i < attributes.size(); ++i)
            {
                AttributeConstant *attrConstant = attributes[i];
                llvm::Constant *constant = attrConstant->DeclareVariable();
                attributePointers.push_back(llvm::ConstantExpr::getPointerCast(constant, int8PtrTy));
            }

            // Create the array value.
            llvm::ArrayType *arrayType = llvm::ArrayType::get(int8PtrTy, attributes.size());
            llvm::Constant *attributeArray = llvm::ConstantArray::get(arrayType, attributePointers);

            // Array class requires external linkage
            llvm::GlobalValue::LinkageTypes attrLinkage = llvm::GlobalVariable::PrivateLinkage;
            if(this == vm->GetArrayClass())
                attrLinkage = llvm::GlobalValue::ExternalLinkage;
            else if(IsGenericBased())
                attrLinkage = llvm::GlobalValue::LinkOnceODRLinkage;

            // Create the attribute list variable.
            std::string listName = GetMangledName() + "_attrlist_";
            attributeList = new llvm::GlobalVariable(*targetModule, arrayType, true,
                attrLinkage, attributeArray, listName);
        }

        // Return the attribute list or null.
        if(attributeList != NULL)
            return attributeList;
        return llvm::ConstantPointerNull::get(int8PtrTy);
    }

    void Member::DeclareAttributes(std::vector<llvm::Constant*> &dest)
    {
        // Get the llvm context.
        llvm::LLVMContext &context = GetLlvmContext();
        llvm::PointerType *int8PtrTy = llvm::Type::getInt8PtrTy(context);

        // Append to the layout the list size.
        dest.push_back(llvm::ConstantInt::get(ChelaType::GetIntType(GetVM())->GetTargetType(), attributes.size()));

        // Append a pointer to the first element.
        dest.push_back(llvm::ConstantExpr::getPointerCast(DeclareAttributes(), int8PtrTy));
    }

    void Member::DefineAttributes()
    {
        for(size_t i = 0; i < attributes.size(); ++i)
            attributes[i]->DefineAttribute();
    }

    bool Member::CheckExternalVisibility() const
    {
        // The parent has the priority.
        const Member *parent = GetParent();
        if(parent && !parent->CheckExternalVisibility() && !IsCdecl())
            return false;

        // Use my visibility.
        return IsPublic() || IsProtected() || IsExtern(); // Protected is also exported.
    }

    llvm::GlobalValue::LinkageTypes Member::ComputeLinkage(bool needsPointer) const
    {
        // Closures are private, generic uses LinknceODR
        if(IsClosure())
        {
            // Closures are always private.
            return llvm::GlobalValue::PrivateLinkage;
        }
        else if(CheckExternalVisibility())
        {
            if(IsGenericBased())
            {
                // Use LinkOnceODR for functions.
                if(IsFunction())
                    return llvm::GlobalValue::LinkOnceODRLinkage;
                else
                    return llvm::GlobalValue::LinkOnceAnyLinkage;
            }
            else
            {
                // Try to use dllexport in windows.
                //Module *module = GetModule();
                //if(!needsPointer && (GetVM()->IsWindows() && module != NULL && module->IsSharedLibrary()))
                //    return llvm::GlobalValue::DLLExportLinkage;
                return llvm::GlobalValue::ExternalLinkage;
            }
        }
        else
        {
            // Private linkage;
            return llvm::GlobalValue::PrivateLinkage;
        }
    }

    llvm::GlobalValue::LinkageTypes Member::ComputeMetadataLinkage() const
    {
        if(IsKernel())
            return llvm::GlobalValue::ExternalLinkage;
        else
            return llvm::GlobalValue::PrivateLinkage;
    }
}
