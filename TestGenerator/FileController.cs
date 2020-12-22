using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TestGenerator
{
    public class FileController
    {
        public string controllerFolderPath { get; set; }
        public string controllerTestFolderPath { get; set; }

        public FileController()
        {

        }

        public List<FileInfo> ReadControllers() {
            throw new NotImplementedException();
        }

        internal void WriteTests(List<TestClass> controllersTests)
        {
            throw new NotImplementedException();
        }
    }
}
