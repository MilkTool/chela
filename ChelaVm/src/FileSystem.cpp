#include <stdio.h>
#include <string>
#include <stdlib.h>
#include "llvm/ADT/SmallString.h"
#include "llvm/Support/raw_ostream.h"
#include "ChelaVm/FileSystem.hpp"

namespace ChelaVm
{
    bool ExistsFile(const std::string &filename)
    {
        // Try to open it
        FILE *test = fopen(filename.c_str(), "r");
        if(test)
        {
            fclose(test);
            return true;
        }

        return false;
    }

    void DeleteFile(const std::string &filename)
    {
        unlink(filename.c_str());
    }

    inline size_t max(size_t a, size_t b)
    {
        if(a > b)
            return a;
        else
            return b;
    }
    std::string DirName(const std::string &filename)
    {
        size_t fpos = filename.rfind('/');
        size_t bpos = filename.rfind('\\');
        size_t lpos;
        if(fpos == std::string::npos)
            lpos = bpos;
        else if(bpos == std::string::npos)
            lpos = fpos;
        else
            lpos = max(fpos, bpos);

        return filename.substr(0, lpos);
    }

    std::string JoinPath(const std::string &a, const std::string &b)
    {
        return a + "/" + b;
    }
    
}

#ifdef _WIN32
#include <windows.h>
// This must go here due to a #define DeleteFile
#endif

namespace ChelaVm
{
#if defined(__unix__)
    void CreateTemporal(const std::string &ext, llvm::raw_ostream **stream, std::string *fileName, bool text)
    {
        char tmplate[256] = "/tmp/XXXXXX";
        strcat(tmplate, ext.c_str());

        // Open the temporal file.
        int fd = mkstemps(tmplate, ext.size());

        // Create the stream.
        *stream = new llvm::raw_fd_ostream(fd, true, text ? 0 : llvm::raw_fd_ostream::F_Binary);

        // Give back the file name.
        *fileName = tmplate;
    }
#elif defined(_WIN32)
    void CreateTemporal(const std::string &ext, llvm::raw_ostream **stream, std::string *fileName, bool text)
    {
        // Create the name buffer.
        llvm::SmallString<256> buffer;
        llvm::raw_svector_ostream out(buffer);
        
        // Add the temporal path.
        char tmpPath[128];
        GetTempPath(128, tmpPath);
        out << tmpPath << '/';
        
        // Add some random numbers.
        out.write_hex(rand());
        out.write_hex(GetTickCount());
        
        // Add the extension.
        out << ext;
        
        // Create the stream.
        out.flush();
        std::string error;
        *stream = new llvm::raw_fd_ostream(buffer.c_str(), error, text ? 0 : llvm::raw_fd_ostream::F_Binary); 
        // TODO: Check error.

        // Give back the file name.
        *fileName = buffer.str();
    }

#else
#error Unsupported platform.
#endif
}
