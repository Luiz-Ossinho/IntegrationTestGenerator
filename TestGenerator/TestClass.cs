using System.Collections.Generic;

namespace TestGenerator
{
        public partial class TestClass
        {
            public string Filename { get; set; }
            public string UsingDirectives { get; set; }
            public string Atributes { get; set; }
            public string Definition { get; set; }
            public string Constructor { get; set; }
            public List<TestMethod> TestMethods { get; set; }
        }
}
