#ifdef __unix__

#include "ChelaVm/System.hpp"
#include <stdio.h>
#include <sys/wait.h>
#include <unistd.h>

namespace ChelaVm
{
    int RunCommand(char *program, char **args)
    {
        pid_t pid = vfork();
        if(pid == 0) // child
        {
            // Execute the program.
            if(execvp(program, args) < 0)
            {
                fprintf(stderr, "failed to exec %s\n", program);
                _exit(1);
            }
        }
        else if(pid < 0)
        {
            fprintf(stderr, "Failed to fork\n");
            return -1;
        }

        // Wait for the child to finish.
        int state;
        do
        {
            waitpid(pid, &state, 0);
        } while(!WIFEXITED(state));

        return WEXITSTATUS(state);
    }

    int RunCommand(std::vector<std::string> &args)
    {
        std::vector<char*> unsafeArgs;
        for(size_t i = 0; i < args.size(); ++i)
            unsafeArgs.push_back((char*)args[i].c_str());
        unsafeArgs.push_back(NULL);
        return RunCommand(unsafeArgs[0], &unsafeArgs[0]);
    }
}

#endif
