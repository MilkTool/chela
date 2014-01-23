#ifdef _WIN32
#include <cstdio>
#include <windows.h>
#include "ChelaVm/System.hpp"
#include "llvm/ADT/SmallString.h"
#include "llvm/Support/raw_ostream.h"

namespace ChelaVm
{
    int RunCommand(std::vector<std::string> &args)
    {
        // Build the complete command line.
        llvm::SmallString<2048> buffer;
        llvm::raw_svector_ostream out(buffer);
        
        for(size_t i = 0; i < args.size(); ++i)
        {
            std::string &arg = args[i];
            if(arg.find(' ') != std::string::npos)
                out << '"' << arg << '"';
            else
                out << arg;
                
            // Separate arguments.
            if(i+1 < args.size())
                out << ' ';
        }
        out.flush();
        buffer.push_back(0);
        
        // Create the process.
        PROCESS_INFORMATION procInfo;
        STARTUPINFO startInfo;
        memset(&startInfo, 0, sizeof(startInfo));
        startInfo.cb = sizeof(startInfo);
        bool res = CreateProcess(NULL,                  // Search in path.
                                 &buffer[0],            // Command line.
                                 NULL,                  // Process attributes.
                                 NULL,                  // Thread attributes.
                                 false,                 // Inherit handles
                                 NORMAL_PRIORITY_CLASS, // Creation flags.
                                 NULL,                  // Environment.
                                 NULL,                  // Current directory.
                                 &startInfo,            // Startup information.
                                 &procInfo);            // Process information.
        if(!res)
        {
            // Get the error message.
            char *lpMsgBuf;
            DWORD dw = GetLastError(); 
            FormatMessage(
                FORMAT_MESSAGE_ALLOCATE_BUFFER | 
                FORMAT_MESSAGE_FROM_SYSTEM |
                FORMAT_MESSAGE_IGNORE_INSERTS,
                NULL,
                dw,
                MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
                (LPTSTR) &lpMsgBuf,
                0, NULL );
                
            // Print the error.
            fprintf(stderr, "Failed to open program '%s': %s\n", args[0].c_str(), lpMsgBuf);
            printf("command line: %s\n", &buffer[0]);
            LocalFree(lpMsgBuf);
            return -1;
        }
        
        // Wait the process to finish.
        WaitForSingleObject(procInfo.hProcess, INFINITE);
        
        // Get the exit code.
        DWORD exitCode;
        GetExitCodeProcess(procInfo.hProcess, &exitCode);
        
        // Close the handles and return the exit code.
        CloseHandle(procInfo.hThread);
        CloseHandle(procInfo.hProcess);
        return exitCode;
    }
}

#endif
