#ifndef CHELAVM_FIELD_HPP
#define CHELAVM_FIELD_HPP

#include "Member.hpp"
#include "Module.hpp"
#include "ConstantValue.hpp"

namespace ChelaVm
{
	class Field: public Member
	{
	public:
		Field(Module *module);
		~Field();

		virtual std::string GetName() const;
		
		virtual MemberFlags GetFlags() const;

		const ChelaType *GetType() const;
        void SetType(const ChelaType *type);
		
		virtual bool IsField() const;
        virtual bool IsGeneric() const;
		
		virtual void DeclarePass();
        virtual void DefinitionPass();

        bool IsRefCounted();
        void SetRefCounted(bool refCounted);

        llvm::GlobalVariable *GetGlobalVariable() const;

        uint32_t GetStructureIndex() const;
        void SetStructureIndex(uint32_t index);

        static Field *PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header);
		void Read(ModuleReader &reader, const MemberHeader &header);

        virtual llvm::DIDescriptor GetDebugNode(DebugInformation *context) const;
        virtual llvm::GlobalVariable *GetMemberInfo();

        Member *InstanceMember(Member *factory, Module *implementingModule, const GenericInstance *instance);
        virtual Member *GetTemplateMember() const;

	private:
        llvm::Constant *GetIntrinsicConstant(const std::string &name, llvm::Type *valueType);

		std::string name;
		MemberFlags flags;
		const ChelaType *type;
        uint32_t structureIndex;
        bool isRefCounted;
        bool declared;
        llvm::GlobalVariable *globalVariable;
        ConstantValue initData;
        llvm::GlobalVariable *fieldInfo;

        // Generic support.
        mutable bool isGeneric;
        Field *fieldTemplate;
	};
};

#endif //CHELAVM_FIELD_HPP
