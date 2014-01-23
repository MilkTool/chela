#ifndef CHELAVM_STRUCTURE_HPP
#define CHELAVM_STRUCTURE_HPP

#include <vector>
#include <memory>
#include "ChelaVm/Types.hpp"
#include "ChelaVm/Module.hpp"
#include "ChelaVm/Field.hpp"
#include "ChelaVm/GenericInstance.hpp"

namespace ChelaVm
{
    enum TypeInfoKind
    {
        TIK_Class = 0,
        TIK_Structure,
        TIK_Interface,
        TIK_Array,
        TIK_Pointer,
        TIK_Reference,
        TIK_Function,
        TIK_Placeholder,
        TIK_Instance,
        TIK_StreamValue,
    };

    class Function;
    class FunctionGroup;
    class Property;
    class ConstantStructure;
    class Structure: public ChelaType
    {
    public:
         Structure(Module *module);
        ~Structure();
        
        Module *GetDeclaringModule() const;
        
        virtual std::string GetName() const;
        virtual MemberFlags GetFlags() const;
        
        virtual bool IsStructure() const;
        virtual bool IsClass() const;
        virtual bool IsInterface() const;
        virtual bool IsAbstract() const;
        virtual bool IsComplexStructure() const;

        virtual llvm::Type *GetTargetType() const;
        virtual size_t GetSize() const;
        virtual size_t GetAlign() const;

        virtual void DeclarePass();
        virtual void DefinitionPass();

        Structure *GetBaseStructure() const;
        Structure *GetInterface(size_t id) const;

        bool IsDerivedFrom(const Structure *base) const;
        bool Implements(const Structure *iface) const;
        int GetITableIndex(const Structure *iface) const;

        static Structure *PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header);
        void ReadStructure(ModuleReader &reader, const MemberHeader &header);
        void Read(ModuleReader &reader, const MemberHeader &header);
        
        size_t GetFieldCount() const;
        Field *GetField(uint32_t id) const;
        Field *GetField(const std::string &name) const;
        size_t GetFieldStructureIndex(const std::string &name) const;

        virtual bool HasSubReferences() const;
        size_t GetValueRefCount() const;
        Field *GetValueRefField(size_t index);
        const Field *GetValueRefField(size_t index) const;

        FunctionGroup *GetFunctionGroup(const std::string &name) const;
        Property *GetProperty(const std::string &name) const;

        size_t GetVMethodCount() const;
        Function *GetVMethod(uint32_t id) const;

        size_t GetBoxIndex() const;

        llvm::StructType *GetStructType() const;
        llvm::StructType *GetBoxedType() const;
        llvm::StructType *GetVTableType() const;
        llvm::GlobalVariable *GetVTable(Module *targetModule) const;
        llvm::GlobalVariable *GetTypeInfo(Module *targetModule) const;
        ConstantStructurePtr GetTypeInfoData(Module *targetModule, llvm::Constant *typeInfoVar, const std::string &prefix="");
        llvm::Function *GetSetupVTables() const;

        // Generic support
        const GenericPrototype *GetGenericPrototype() const;
        virtual const GenericInstance *GetGenericInstanceData() const;
        virtual bool IsGeneric() const;
        virtual bool IsGenericType() const;

        virtual Member *InstanceMember(Member *factory, Module *implementingModule, const GenericInstance *instance);
        virtual Member *GetTemplateMember() const;

        Structure *PreloadGeneric(Member *factory, Module *implementingModule, const GenericInstance *instance);
        Structure *InstanceGeneric(Module *implementingModule, const GenericInstance *instance);
        const ChelaType *InstanceGeneric(const GenericInstance *instance) const;
        Member *GetInstancedMember(Member *templateMember) const;

        ConstantStructure *CreateConstant(Module *targetModule) const;

    protected:
        friend class ConstantStructure;

        // Each interface contract implementation data.
        struct ContractData
        {
            Member *interface;
            Member *declaration;
            Member *implementation;
        };

        // Each interface implementation data.
        struct InterfaceImplementation
        {
            InterfaceImplementation()
                : own(false), interface(NULL),
                vtableField(0), aliasIndex(-1), vtable(NULL)
            {
            }

            InterfaceImplementation(const InterfaceImplementation &o)
                : own(o.own), interface(o.interface),
                vtableField(o.vtableField),
                aliasIndex(o.aliasIndex), vtable(o.vtable),
                implementations(o.implementations)
            {
            }

            bool own;
            Structure *interface;
            uint32_t vtableField;
            int aliasIndex;
            llvm::GlobalVariable *vtable;
            std::vector<ContractData> implementations;
            std::vector<Function*> slots;
        };

        void AppendLayout(std::vector<llvm::Type*> &layout, Structure *building, bool value);
        void AddSubReferencesOffsets(llvm::Constant *baseOffset, const Structure *building, std::vector<llvm::Constant*> &dest);
        void CreateVTableLayout();
        llvm::Constant *CreateVTable(Module *implModule, llvm::Constant *typeInfoVar, const std::string &prefix="");
        llvm::Constant *CreateITable(InterfaceImplementation &impl, int index, Module *implModule, llvm::Constant *typeInfoVar, const std::string &prefix="");
        void CreateTables();
        void CreateTypeInfo();
        llvm::Constant* CreateCustomVCtor(Module *implModule, llvm::Constant *vtable, std::vector<llvm::Constant*> &itables, const std::string &prefix);

        void PredeclarePass();
        void CheckCompleteness();
        void DeclareTables() const;
        void DeclareVTable() const;
        void DeclareTypeInfo() const;
        void PrepareSlots(Structure *building);
        void PrepareMethodSlot(Function *method, uint32_t methodId);
        void PrepareImplementations();
        void ImplementInterface(Structure *interface, bool checkAlias);
        void FinishLoad();
        void ValueDeclare();
        void ReferenceDeclare();
        virtual Structure *CreateBuilding(Module *module) const;

        // Debug info support.
        virtual llvm::DIType CreateDebugType(DebugInformation *context) const;

        // Generic helpers.
        Member *GetActualMember(Member *member);
        Structure *GetActualInterface(Member *candidate);
        Function *GetActualFunction(Member *candidate);

        // Box trampoline.
        llvm::Function *CreateBoxTrampoline(Function *vfunction);

        // Structure identification.
        Module *declaringModule;
        std::string name;
        MemberFlags flags;
        Member *baseStructure;

        // Structure members, relationships.
        std::vector<Member*> interfaces;
        std::vector<Field*> fields;
        std::vector<Function*> vmethods;
        std::vector<Member*> members;
        std::vector<ContractData> contracts;
        mutable std::vector<InterfaceImplementation> implementations;

        // Value structures references.
        std::vector<Field*> valueRefSlots;

        // Generic structures.
        GenericPrototype genericPrototype;
        const GenericInstance *genericInstance;
        bool isGenericPreload;
        Structure *genericTemplate;
        std::vector<Structure*> genericImplementations;

        // LLVM data structures
        llvm::Type *vtableOffsetType;
        mutable llvm::StructType *vtableType;
        mutable llvm::GlobalVariable *vtableVariable;
        mutable llvm::GlobalVariable *typeInfo;
        llvm::Function *setupVTables;

        // LLVM implementation.
        mutable bool isOpaque;
        mutable llvm::StructType *structType;
        mutable llvm::StructType *boxedStructType;
        mutable bool declaredTables;
        bool defined;
        bool predeclared;
        bool declared;
        size_t boxIndex;
    };

    // Constant structure building.
    class ConstantStructure
    {
        friend class Structure;
        ConstantStructure(Module *targetModule, const Structure *building);
    public:
        ~ConstantStructure();

        void SetField(const std::string &name, llvm::Constant *value);
        llvm::Constant *Finish();

    private:
        Module *targetModule;
        const Structure *building;
        std::vector<llvm::Constant*> elements;
    };

    typedef std::auto_ptr<ConstantStructure> ConstantStructurePtr;
};

#endif //CHELAVM_STRUCTURE_HPP
