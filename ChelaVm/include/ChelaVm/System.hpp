#ifndef CHELAVM_SYSTEM_HPP
#define CHELAVM_SYSTEM_HPP

#include <string>
#include <vector>

#ifdef _WIN32
#define PROG_PREFIX
#define PROG_SUFFIX ".exe"
#else
#define PROG_PREFIX
#define PROG_SUFFIX
#endif

namespace ChelaVm
{
    int RunCommand(std::vector<std::string> &args);
}

#endif //CHELAVM_SYSTEM_HPP
