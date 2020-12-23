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
            var dll = new FileInfo(@"C:\Users\ejesus\workspace\ssc-servicos-cartao\Api.Cartao\bin\Api.Cartao.dll");
            return new List<FileInfo> {dll};
        }

        public void WriteTests(List<TestClass> controllersTests)
        {
            throw new NotImplementedException();
        }
    }
}
