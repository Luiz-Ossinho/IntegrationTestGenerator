using System.Collections.Generic;

namespace TestGenerator
{
    public class TestClass
    {
        public TestClass(string Filename, string Namespace, string setupNamespace, bool realizaLogin = false)
        {
            this.Filename = Filename;
            this.Definition = $"public class {Filename}";
            this.Namespace = Namespace;
            AddUsingDirective(setupNamespace);

            Constructor =
                $"\tpublic {Filename}(IntegrationTestFixture fixture)\n" +
                "\t{\n" +
                "\t\t_fixture = fixture;\n" +
                "\t\tclient = fixture.app;\n" +
                $"\t\t{(realizaLogin ? "_fixture.RealizarLogin();" : "")}\n" +
                "\t}\n";
        }
        public string Filename { get; }
        public string Namespace { get; }
        public string Definition { get; }
        public string Constructor { get; }
        public List<TestMethod> TestMethods { get; } = new List<TestMethod>();
        public Dictionary<string, string> Fields { get; } = new Dictionary<string, string>
        {
            { "IntegrationTestFixture","_fixture" },
            { "HttpClient","client" }
        };
        public List<string> UsingDirectives { get; } = new List<string>
        {
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "System.Net.Http",
            "System.Threading.Tasks",
            "Xunit",
            "Xunit.Priority",
            "Xunit.Categories"
        };
        public List<string> Atributes { get; } = new List<string>
        {
            "Collection(nameof(IntegrationApiTestFixtureCollection))",
            "TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)"
        };
        public void AddTestMethods(IEnumerable<TestMethod> testMethod) => this.TestMethods.AddRange(testMethod);
        public void AddUsingDirective(string usingDirective)
        {
            if (!this.UsingDirectives.Contains(usingDirective))
                this.UsingDirectives.Add(usingDirective);
        }

        public override string ToString()
        {
            string testClass = "";

            // Adds any using directives at the top of the file
            string usigns = "";
            foreach (var usingDirective in this.UsingDirectives)
                usigns += ("using " + usingDirective + ";\n");
            testClass += usigns;

            testClass += "namespace " + this.Namespace + "\n";
            testClass += "{\n";

            // Adds the class definition
            testClass += "\t" + this.Definition + "\n";
            testClass += "\t{\n";

            // Adds any fields the test class may need
            string fields = "";
            foreach (var field in this.Fields)
                fields += "\tprivate " + field.Key + " " + field.Value + ";\n";
            testClass += fields;

            // Adds the constructor to the test class
            testClass += this.Constructor;

            // Adds the test methods to the test class
            string testMethods = "";
            var aux = new List<string>();
            for (int i = 0; i < this.TestMethods.Count; i++)
            {
                // If the same endpoint has two implementations,
                // This assures there wont be test methods with the same name
                if (aux.Contains(this.TestMethods[i].Contract))
                    this.TestMethods[i].Contract += $"{i + 1}";

                aux.Add(this.TestMethods[i].Contract);

                testMethods += "\t[Fact]\n";
                testMethods += $"\t{this.TestMethods[i].Contract}" + "_Success()" + "\n";
                testMethods += this.TestMethods[i].ToString();
            }
            testClass += testMethods;

            testClass += "\t}\n";
            testClass += "}\n";

            return testClass;
        }
    }
}
