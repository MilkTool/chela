#ifndef _CHELAVM_CLOSURE_HPP
#define _CHELAVM_CLOSURE_HPP

#include <vector>
#include "Function.hpp"
#include "Class.hpp"

namespace ChelaVm
{
    class Closure: public Class
    {
    public:
        Closure(Module *module);
        ~Closure();

        virtual bool IsClosure() const;

        Field *GetParentClosure();

        size_t GetLocalCount();
        Field *GetLocal(size_t id);

        void Read(ModuleReader &reader);
        static Closure *Create(Module *module, Closure *parent, std::vector<const ChelaType*> &locals);

    private:
        Field *parentClosure;
        std::vector<Field*> locals;
    };
}

#endif //_CHELAVM_CLOSURE_HPP
