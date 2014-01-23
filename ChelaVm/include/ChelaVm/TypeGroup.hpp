#ifndef CHELAVM_TYPE_GROUP_HPP
#define CHELAVM_TYPE_GROUP_HPP

#include "ChelaVm/Structure.hpp"
#include "ChelaVm/ModuleReader.hpp"

namespace ChelaVm
{
    class TypeGroup: public Member
    {
    public:
        TypeGroup(Module *module);
        ~TypeGroup();

        virtual std::string GetName() const;

        virtual void UpdateParent(Member *parent);

        virtual bool IsTypeGroup() const;

        size_t GetTypeCount() const;
        Structure *GetType(size_t index);
        uint32_t GetTypeId(size_t index);

        static TypeGroup *PreloadMember(Module *module, ModuleReader &reader, const MemberHeader &header);
        void ReadStructure(ModuleReader &reader, const MemberHeader &header);

        virtual void DeclarePass();
        virtual void DefinitionPass();
        virtual Member *InstanceMember(Member *factory, Module *implementingModule, const GenericInstance *instance);

    private:
        std::string name;
        std::vector<Structure *> buildings;
        bool isInstance;
    };

}; // namespace ChelaVm

#endif //CHELAVM_TYPE_GROUP_HPP

