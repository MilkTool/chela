#include <cmath>
#include <cstdio>
#include <cstring>
#include <ctime>
#include "ChelaVm/VirtualMachine.hpp"
#include "ChelaVm/Module.hpp"

using namespace ChelaVm;

static void PrintHelp()
{
}

static void PrintVersion()
{
	printf("ChelaVm version 0.1a");
}

static void AddRuntimePaths(int argc, const char *argv[], std::vector<std::string> &paths)
{
    // argv[0] must contain the application path.
    std::string appPath = argv[0];

    // Extract the folder.
    size_t pos1 = appPath.rfind('/');
    size_t pos2 = appPath.rfind('\\');
    size_t pos;
    if(pos1 == std::string::npos)
        pos = pos2;
    else if(pos2 == std::string::npos)
        pos = pos1;
    else
        pos = std::max(pos1, pos2);
    appPath = appPath.substr(0, pos);

    // Add the virtual machine path.
    paths.push_back(appPath);

    // Platform specifics.
#if _WIN32
    // FIXME
    paths.push_back("c:\\windows\\system32");
#else
    paths.push_back(appPath + "/../lib");
#endif
}

int main (int argc, const char *argv[])
{
	// Create the virtual machine.
	std::string outputFile;
    std::string programName;
    std::vector<std::string> libraries;
    std::vector<std::string> libPaths;
    bool outputNativeAssembler = false;
	bool outputAssembler = false;
	bool outputBitcode = false;
    bool outputObject = false;
    bool debugging = false;
    bool noruntime = false;
    bool noreflection = false;
    bool jitting = false;
    int optLevel = 0;
	Module *program = NULL;
    VirtualMachineOptions options;
	VirtualMachine vm(llvm::getGlobalContext(), options);
    
    // Initialize the random seed, required for temporals in some platforms.
    srand(time(NULL));
    	
	// Parse the command line.
	for(int i = 1; i < argc; i++)
	{
		if(!strcmp(argv[i], "-help") ||
		   !strcmp(argv[i], "-h"))
		{
			PrintHelp();
			return 0;
		}
		else if(!strcmp(argv[i], "-version"))
		{
			PrintVersion();
			return 0;
		}
		else if(!strcmp(argv[i], "-o") ||
				!strcmp(argv[i], "--output"))
		{
			outputFile = argv[++i];
		}
		else if(!strcmp(argv[i], "-S"))
		{
			outputAssembler = true;
		}
        else if(!strcmp(argv[i], "-s"))
        {
            outputNativeAssembler = true;
        }
		else if(!strcmp(argv[i], "-B"))
		{
			outputBitcode = true;
		}
        else if(!strcmp(argv[i], "-c"))
        {
            outputObject = true;
        }
        else if(!strcmp(argv[i], "-g"))
        {
            debugging = true;
        }
        else if(!strcmp(argv[i], "-noruntime"))
        {
            noruntime = true;
        }
        else if(!strcmp(argv[i], "-noreflection"))
        {
            noreflection = true;
        }
        else if(!strcmp(argv[i], "-O0"))
        {
            optLevel = 0;
        }
        else if(!strcmp(argv[i], "-O1"))
        {
            optLevel = 1;
        }
        else if(!strcmp(argv[i], "-O2"))
        {
            optLevel = 2;
        }
        else if(!strcmp(argv[i], "-O3"))
        {
            optLevel = 3;
        }
        else if(!strncmp(argv[i], "-l", 2))
        {
            libraries.push_back(argv[i] + 2);
        }
        else if(!strncmp(argv[i], "-L", 2))
        {
            libPaths.push_back(argv[i] + 2);
        }
        else if(!strcmp(argv[i], "-jit"))
        {
            // Update the command line.
            argc -= i;
            argv += i;
            jitting = true;
            break;
        }
		else if(argv[i][0] == '-')
		{
			printf("Unrecognized option '%s'.\n", argv[i]);
			PrintHelp();
			return 1;
		}
		else if(programName.empty())
		{
            programName = argv[i];
		}
	}

    // Print help if there isn't input.
    if(programName.empty())
    {
        PrintHelp();
        return 0;
    }

    // Add runtime paths.
    if(!noruntime)
        AddRuntimePaths(argc, argv, libPaths);

    // Load the program.
    program = vm.LoadModule(programName, noreflection, debugging, &libPaths[0], libPaths.size());
    program->SetOptimizationLevel(optLevel);

    // Add the libraries.
    for(size_t i = 0; i < libraries.size(); ++i)
        program->AddLibrary(libraries[i]);

	// Compile the program module.
	program->Compile();

	// Perform the output.
	if(outputAssembler)
	{
		program->Dump();
		return 0;
	}
    else if(outputNativeAssembler)
    {
        // Check the output file.
        if(outputFile.empty())
        {
            fprintf(stderr, "Expected assembler output file.");
            PrintHelp();
            return 1;
        }
        
        // Write the assembler.
        return !program->WriteNativeAssembler(outputFile);
    }
	else if(outputBitcode)
	{
		// Check the output file.
		if(outputFile.empty())
		{
			fprintf(stderr, "Expected bitcode output file.");
			PrintHelp();
			return 1;
		}
		
		// Write the module.
		return !program->WriteBitcode(outputFile);
	}
    else if(outputObject)
    {
        // Check the output file.
        if(outputFile.empty())
        {
            fprintf(stderr, "Expected object output file.");
            PrintHelp();
            return 1;
        }

        // Write the module.
        return !program->WriteObjectFile(outputFile);
    }
	else if(!jitting)
	{
        if(outputFile.empty())
            outputFile = "a.out";
		return !program->WriteModule(outputFile);
	}
	else
	{
		// Run the module.
		return program->Run(argc, argv);
	}
}

