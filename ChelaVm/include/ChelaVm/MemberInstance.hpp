#ifndef _CHELAVM_MEMBER_INSTANCE_HPP
#define _CHELAVM_MEMBER_INSTANCE_HPP

#include "Member.hpp"
#include "ModuleReader.hpp"

namespace ChelaVm
{
    class MemberInstance: public Member
    {
    public:
        MemberInstance(Module *module);
        ~MemberInstance();

        virtual bool IsAnonymous() const;
        virtual bool IsGeneric() const;
        virtual bool IsMemberInstance() const;

        Member *GetTemplateMember() const;
        Member *GetActualMember() const;

        static Member *PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header);
        void Read(ModuleReader &reader, const MemberHeader &header);

        Member *InstanceMember(Member */*factory*/, Module *implementingModule, const GenericInstance *instance);
        
    private:
        Member *factoryMember;
        Member *templateMember;
        mutable Member *actualMember;
    };
}

#endif //_CHELAVM_MEMBER_INSTANCE_HPP

