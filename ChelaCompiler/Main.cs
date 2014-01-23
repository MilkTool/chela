using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Chela.Compiler;
using Chela.Compiler.Module;

namespace Chela
{
	class MainClass
	{
        private static string RuntimeLibrary = "ChelaCore";

		private static void PrintHelp()
		{
			Console.WriteLine("Unimplemented chela help string.");
		}
		
		private static void PrintVersion()
		{
			Console.WriteLine("Chela version 0.1a");
		}

        private static string CreateModuleName(string outputFile)
        {
            // Get the name without extensions.
            string name = System.IO.Path.GetFileNameWithoutExtension(outputFile);

            // Remove the lib prefix.
            if(name.StartsWith("lib") && outputFile.EndsWith(".so"))
                name = name.Substring(3);

            // Return the name.
            return name;
        }

        private static string CreateTempName()
        {
            return System.IO.Path.GetTempFileName();
        }

        private static void AppendArg(StringBuilder builder, string arg)
        {
            builder.Append(' ');
            arg = arg.Replace("\"", "\\\"");
            if(arg.IndexOf(' ') >= 0)
                arg = "\"" + arg + "\"";
            builder.Append(arg);
        }

        private static bool ExistsProgram(string name)
        {
            return System.IO.File.Exists(name);
        }

        private static bool IsUnix()
        {
            PlatformID platform = Environment.OSVersion.Platform;
            return platform == PlatformID.Unix;
        }

        private static bool IsWindows()
        {
            PlatformID platform = Environment.OSVersion.Platform;
            return platform == PlatformID.Win32NT ||
               platform == PlatformID.Win32S ||
               platform == PlatformID.Win32Windows ||
               platform == PlatformID.WinCE ||
               platform == PlatformID.Xbox /* ?*/;
        }

        private static string GetChelaVm()
        {
            // Build the program name.
            string progName = "chlc-vm";
            if(IsWindows())
                progName += ".exe";

            // Find the program.
            string location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            location = System.IO.Path.GetDirectoryName(location);
            string ret = System.IO.Path.Combine(location, progName);
            if(ExistsProgram(ret))
                return ret;

            throw new ApplicationException("Couldn't find ChelaVm");
        }

        private static void AddRuntimePaths(ChelaCompiler compiler)
        {
            // Add the compiler path.
            string compilerPath = Path.GetDirectoryName(typeof(MainClass).Assembly.Location);
            compiler.AddLibraryPath(compilerPath);

            // Add the lib directory.
            if(IsUnix())
                compiler.AddLibraryPath(Path.Combine(compilerPath, "../lib/"));
            else if(IsWindows())
                compiler.AddLibraryPath(Environment.GetFolderPath(Environment.SpecialFolder.System));
            else
                throw new ApplicationException("Unsupported platform.");
        }

        private static void AddRuntimeReference(ChelaCompiler compiler)
        {
            compiler.LoadReference(RuntimeLibrary);
        }

		public static void Main (string[] args)
		{
			List<string> inputFiles = new List<string>();
            List<string> references = new List<string>();
            List<string> nativeLibs = new List<string>();
            List<string> libFindPaths = new List<string>();
            List<ResourceData> resources = new List<ResourceData> ();
			string outputFile = null;
			bool verbose = false;
            bool bitcode = false;
            bool debug = false;
            bool llvmBitcode = false;
            bool assembler = false;
            bool llvmAssembler = false;
            bool objectCode = false;
            bool noruntime = false;
            bool noreflection = false;
            bool bench = false;
            int optimization = 0;
            ModuleType moduleType = ModuleType.Executable;
			
			// Parse the command line.
            CommandLineParser parser = new CommandLineParser();
            parser.Help = PrintHelp;
            parser.Miscellaneous = delegate (string input) {
                inputFiles.Add(input);
            };

            // Output file.
            parser.AddArgument(delegate(string output) {
                    outputFile = output;
                }, "-o", "--output");

            // Help.
            parser.AddArgument(PrintHelp, "-h", "--help");

            // Resource.
            parser.AddArgument(delegate(string nameId) {
                    string fileName = nameId;
                    string name;
                    if(nameId.IndexOf(',') != -1)
                    {
                        string[] pair = nameId.Split(',');
                        fileName = pair[0];
                        name = pair[1];
                    }
                    else
                    {
                        name = Path.GetFileName(fileName);
                    }
                    resources.Add(new ResourceData(fileName, name));
                }, "-resource");

            // Benchmarking.
            parser.AddArgument(delegate() {
                bench = true;
                }, "-benchmark");

            // Version.
            parser.AddArgument(PrintVersion, "--version");

            // Verbose
            parser.AddArgument(delegate() {
                    verbose = true;
                }, "-v", "--verbose");

            // Debug
            parser.AddArgument(delegate() {
                    debug = true;
                }, "-g");

            // Optimization.
            parser.AddPrefix(delegate(string level) {
                    optimization = int.Parse(level);
                }, "-O");

            // Reference.
            parser.AddArgument(delegate(string refName) {
                    references.Add(refName);
                }, "-r", "-ref");

            // Native libraries.
            parser.AddArgument(delegate(string libName) {
                    nativeLibs.Add(libName);
                }, "-l");
            parser.AddPrefix(delegate(string libName) {
                    nativeLibs.Add(libName);
                }, "-l");

            // Library find paths.
            parser.AddPrefix(delegate(string libPath) {
                    libFindPaths.Add(libPath);
                }, "-L");

            // Target.
            parser.AddArgument(delegate(string targetType) {
                    if(targetType == "lib" || targetType == "library")
                        moduleType = ModuleType.Library;
                    else if(targetType == "exe" || targetType == "executable")
                        moduleType = ModuleType.Executable;
                    else
                    {
                        Console.Error.WriteLine("Unknown module type '" + targetType + "'");
                        PrintHelp();
                        System.Environment.Exit(-1);
                    }
                }, "-t", "--target");

            // Implicit reference to the runtime.
            parser.AddArgument(delegate() {
                noruntime = true;
            }, "-noruntime");

            // No reflection info.
            parser.AddArgument(delegate() {
                noreflection = true;
            }, "-noreflection");

            // Bitcode.
            parser.AddArgument(delegate() {
                    bitcode = true;
                }, "-b");

            // Llvm Bitcode.
            parser.AddArgument(delegate() {
                    llvmBitcode = true;
                }, "-B");

            // Llvm assembler.
            parser.AddArgument(delegate() {
                    llvmAssembler = true;
                }, "-S");

            // Assembler.
            parser.AddArgument(delegate() {
                    assembler = true;
                }, "-s");

            // Object code.
            parser.AddArgument(delegate() {
                    objectCode = true;
                }, "-c");

            // Parse the command line.
            try
            {
                parser.Parse(args);
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                Environment.Exit(-1);
            }
			
			if(outputFile == null || outputFile == "")
			   outputFile = "chelamod.out";

			try
			{
				// Create the compiler.
				ChelaCompiler compiler = new ChelaCompiler(moduleType);
                compiler.SetDebugBuild(debug);

                // Set benchmarking.
                Benchmark.Enabled = bench;

                // Add runtime search paths.
                if(!noruntime)
                    AddRuntimePaths(compiler);

                // Libraries paths.
                foreach(string libPath in libFindPaths)
                    compiler.AddLibraryPath(libPath);

                // Add runtime reference.
                if(!noruntime)
                    AddRuntimeReference(compiler);

                // Load the references.
                foreach(string modRef in references)
                    compiler.LoadReference(modRef);

                // Add native libraries.
                foreach(string natLib in nativeLibs)
                    compiler.AddNativeLibrary(natLib);

                // Add each resource.
                foreach(ResourceData resource in resources)
                    compiler.AddResource(resource);

				// Compile each input file.
				foreach(string input in inputFiles)
				{
					// Compile the input file.
					compiler.CompileFile(input);
				}
				
				// Link the program.
				compiler.LinkProgram();

                // Set the module name.
                string moduleName = CreateModuleName(outputFile);
                compiler.SetModuleName(moduleName);

				// Dump the program.
				if(verbose)
					compiler.Dump();

				// Write the chela bitcode.
                string bitcodeFile = bitcode ? outputFile : CreateTempName();
				compiler.WriteOutput(bitcodeFile);

                // Write the benchmark resume.
                if(bench)
                    Benchmark.Resume();

                // Return if the bitcode was the only thing asked.
                if(bitcode)
                    return;

                // Build the ChelaVm command line.
                StringBuilder commandLine = new StringBuilder();

                // Add the debug options.
                if(debug)
                    AppendArg(commandLine, "-g");

                // Add the no runtime flag.
                if(noruntime)
                    AppendArg(commandLine, "-noruntime");

                // Add the no reflection flag if specified.
                if(noreflection)
                    AppendArg(commandLine, "-noreflection");

                // Add the optimization level.
                AppendArg(commandLine, "-O" + optimization);

                // Set the output type.
                if(llvmBitcode)
                    AppendArg(commandLine, "-B");
                else if(llvmAssembler)
                    AppendArg(commandLine, "-S");
                else if(assembler)
                    AppendArg(commandLine, "-s");
                else if(objectCode)
                    AppendArg(commandLine, "-c");

                // Add the library find paths.
                foreach(string libPath in libFindPaths)
                    AppendArg(commandLine, "-L" + libPath);

                // Add the output file.
                AppendArg(commandLine, "-o");
                AppendArg(commandLine, outputFile);

                // Add the bitcode file.
                AppendArg(commandLine, bitcodeFile);

                // Invoke the ChelaVm compiler.
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
                proc.StartInfo.FileName = GetChelaVm();
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.Arguments = commandLine.ToString();
                proc.StartInfo.RedirectStandardError = false;
                proc.StartInfo.RedirectStandardOutput = false;
                proc.StartInfo.RedirectStandardInput = false;
                proc.Start();
                proc.WaitForExit();
                //System.Console.WriteLine("{0} {1}", GetChelaVm(), commandLine.ToString());
                Environment.ExitCode = proc.ExitCode;
                proc.Close();

                // Delete the temporal file.
                System.IO.File.Delete(bitcodeFile);
			}
			catch(CompilerException e)
			{
				Console.Error.WriteLine(e.Message);
				if(verbose)
					Console.Error.WriteLine(e.StackTrace);
                Environment.ExitCode = -1;
			}
			catch(ApplicationException e)
			{
				Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
                Environment.ExitCode = -1;
			}
		}
	}
}

