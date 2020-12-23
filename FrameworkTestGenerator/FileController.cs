using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TestGenerator
{
    public class FileController
    {
        public string projectAssemblyPath { get; }
        public string controllerTestFolderPath { get; }

        public FileController(string projectAssemblyPath, string controllerTestFolderPath)
        {
            this.projectAssemblyPath = projectAssemblyPath;
            this.controllerTestFolderPath = controllerTestFolderPath;
        }

        public List<FileInfo> LoadProjectAssemblyFileInfo()
            => new List<FileInfo> { new FileInfo(projectAssemblyPath) };

        public async Task WriteTests(List<TestClass> testClasses)
        {
            foreach (var testClass in testClasses)
            {
                var stream = File.CreateText(this.controllerTestFolderPath + testClass.Filename + ".cs");
                await stream.WriteAsync(testClass.ToString());
                stream.Flush();
                stream.Close();
                stream.Dispose();
            }
        }
    }
}
