#include "Module.hpp"
#include "ModuleReader.hpp"

namespace ChelaVm
{
    class ArgumentData
    {
    public:
        ArgumentData(Module *module);
        ArgumentData(const ArgumentData &other);
        ~ArgumentData();

        Module *GetModule() const;

        const std::string &GetName() const;

        void Read(ModuleReader &reader);

    private:
        Module *module;
        std::string name;
    };
}
