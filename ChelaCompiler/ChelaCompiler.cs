using System.IO;
using Chela.Compiler.Ast;
using Chela.Compiler.Module;
using Chela.Compiler.Semantic;

namespace Chela.Compiler
{
	public class ChelaCompiler
	{
		private ModuleNode moduleNode;

		public ChelaCompiler (ModuleType moduleType)
		{
			// Create the module node.
			moduleNode = new ModuleNode(null, null);
			moduleNode.SetName("global");
			
			// Create the module.
			ChelaModule module = new ChelaModule();
			moduleNode.SetModule(module);
			module.SetName("unnamed");
            module.SetModuleType(moduleType);
		}

        public void LoadReference(string filename)
        {
            // Load the referenced module.
            ChelaModule module = ChelaModule.LoadNamedModule(filename);

            // Add the reference.
            moduleNode.GetModule().AddReference(module);
        }

        public void AddNativeLibrary(string name)
        {
            // Add the reference.
            moduleNode.GetModule().AddNativeLibrary(name);
        }

        public void AddLibraryPath(string path)
        {
            // Add the library path.
            ChelaModule.AddLibraryPath(path);
        }

        public void AddResource(ResourceData data)
        {
            // Add the resource.
            moduleNode.GetModule().AddResource(data);
        }
        
		public void CompileFile(string fileName)
		{
			// Open the input file.
			FileStream file = new FileStream(fileName, FileMode.Open);
			
			// Compile it.
			CompileFile(file, fileName);
		}
		
		public void CompileFile(Stream file, string fileName)
		{
			// Create the lexer.
			Lexer lexer = new Lexer(fileName, file);
			
			// Create the parser.
			Parser parser = new Parser();
			parser.Lexer = lexer;
			
			// Parse the file.
            Benchmark.Begin();
			AstNode node = parser.Parse();
            Benchmark.End("Parse file " + fileName);

            // Prepare the file.
            PrepareFile(node, fileName);			
		}

        private void PrepareFile(AstNode content, string fileName)
        {
            // Use an special position.
            TokenPosition pos = new TokenPosition(fileName, -1, -1);

            // Create the file node.
            FileNode file = new FileNode(content, pos);

            // Add the file into the module node.
            moduleNode.AddFirst(file);
        }

		public void LinkProgram()
		{
			// Read the result.
			AstNode content = moduleNode;

			// Perform semantic analysis passes.
            AstVisitor[] semanticPasses = new AstVisitor[] {
                // Declare module types.
                new ModuleTypeDeclaration(),

                // Expand using <namespaces>.
                new PseudoScopeExpansion(),

                // Expand type defs.
                new TypedefExpansion(),

                // Read the object bases.
                new ModuleReadBases(),

                // Declare module object(type members)
                new ModuleObjectDeclarator(),

                // Explicit interface member binding.
                new ExplicitInterfaceBinding(),

                // Build type inheritance tables.
                new ModuleInheritance(),

                // Perform function semantic analysis.
                new FunctionSemantic(),

                // Expand constants.
                new ConstantExpansion(),

                // Generate function byte code.
                new FunctionGenerator(),
            };

            // Execute passes.
            for(int i = 0; i < semanticPasses.Length; ++i)
            {
                AstVisitor pass = semanticPasses[i];
                pass.BeginPass();
                Benchmark.Begin();
                content.Accept(pass);
                Benchmark.End(pass.GetType().ToString());
                pass.EndPass();
            }
		}

		public void Dump()
		{
			// Dump the module.
			moduleNode.GetModule().Dump();
		}

        public void SetModuleName(string moduleName)
        {
            // Set the module name.
            ChelaModule module = moduleNode.GetModule();
            module.SetName(moduleName);
        }

        public void SetDebugBuild(bool debug)
        {
            ChelaModule module = moduleNode.GetModule();
            module.SetDebugBuild(debug);
        }

		public void WriteOutput(string outputFile)
		{
            // Set the module filename data used by debugging.
            ChelaModule module = moduleNode.GetModule();
            module.SetFileName(outputFile);
            module.SetWorkDirectory(System.Environment.CurrentDirectory);

            // Create the output file.
			FileStream output = new FileStream(outputFile, FileMode.Create);
			
			// Write the module.
			ModuleWriter writer = new ModuleWriter(output);
            Benchmark.Begin();
			module.Write(writer);

            // Close the output file.
            output.Flush();
            output.Close();
            Benchmark.End("Write module");

		}
	}
}

