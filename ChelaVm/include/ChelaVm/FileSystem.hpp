#ifndef CHELAVM_FILESYSTEM_HPP
#define CHELAVM_FILESYSTEM_HPP

#include <string>
#include "llvm/Support/raw_ostream.h"

namespace ChelaVm
{
    bool ExistsFile(const std::string &filename);
    void DeleteFile(const std::string &filename);
    std::string DirName(const std::string &filename);
    std::string JoinPath(const std::string &a, const std::string &b);

    void CreateTemporal(const std::string &ext, llvm::raw_ostream **stream, std::string *fileName, bool text = false);
}

#endif //CHELAVM_FILESYSTEM_HPP
