using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace TestGenerator
{
    public class TestController
    {
        public List<Type> Types { get; set; }

        public void CreateTypes(List<FileInfo> files)
        {
            foreach (var sourceFile in files)
            {
                Console.WriteLine("Loading file: " + sourceFile.Exists);
                // Prepary a file path for the compiled library.
                string outputName = string.Format(@"{0}\{1}.dll",
                    Environment.CurrentDirectory,
                    Path.GetFileNameWithoutExtension(sourceFile.Name));

                // Compile the code as a dynamic-link library.
                bool success = Compile(sourceFile, new CompilerParameters()
                {
                    GenerateExecutable = false, // wiil compile as library (dll)
                    OutputAssembly = outputName,
                    GenerateInMemory = false, // will generate as a physical file
                });
                if (success)
                {
                    // Load the compiled library.
                    Assembly assembly = Assembly.LoadFrom(outputName);

                    Types.Add(assembly.GetType("Command"));
                }
            }
        }

        public List<TestClass> CreateTests(List<FileInfo> controllers)
        {
            CreateTypes(controllers);
            throw new NotImplementedException();
        }

        private static bool Compile(FileInfo sourceFile, CompilerParameters options)
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

            CompilerResults results = provider.CompileAssemblyFromFile(options, sourceFile.FullName);

            if (results.Errors.Count > 0)
            {
                Console.WriteLine("Errors building {0} into {1}", sourceFile.Name, results.PathToAssembly);
                foreach (CompilerError error in results.Errors)
                {
                    Console.WriteLine("  {0}", error.ToString());
                    Console.WriteLine();
                }
                return false;
            }
            else
            {
                Console.WriteLine("Source {0} built into {1} successfully.", sourceFile.Name, results.PathToAssembly);
                return true;
            }
        }
    }
}
