#include "ChelaVm/ArgumentData.hpp"

namespace ChelaVm
{
    ArgumentData::ArgumentData(Module *module)
        : module(module)
    {
    }

    ArgumentData::ArgumentData(const ArgumentData &other)
        : module(other.module), name(other.name)
    {
    }

    ArgumentData::~ArgumentData()
    {
    }

    Module *ArgumentData::GetModule() const
    {
        return module;
    }

    const std::string &ArgumentData::GetName() const
    {
        return name;
    }

    void ArgumentData::Read(ModuleReader &reader)
    {
        // Read the name id.
        uint32_t nameId;
        reader >> nameId;
        name = module->GetString(nameId);
    }
}
