using System.Collections.Generic;

namespace TestGenerator
{
    public class TestClass
    {
        public TestClass(string Filename, string Namespace, bool realizaLogin = false)
        {
            this.Filename = Filename;
            this.Definition = $"public class {Filename}";
            this.Namespace = Namespace;

            this.realizaLogin = realizaLogin;

            Constructor =
                $"\tpublic {Filename}(IntegrationTestFixture fixture)\n" +
                "\t{\n" +
                "\t\t_fixture = fixture;\n" +
                "\t\tclient = fixture.app;\n" +
                $"\t\t{(realizaLogin ? "_fixture.RealizarLogin();" : "")}\n" +
                "\t}\n";
        }
        public bool realizaLogin;
        public string Filename { get; }
        public string Namespace { get; }
        public string Definition { get; }
        public List<TestMethod> TestMethods { get; } = new List<TestMethod>();
        public Dictionary<string, string> Fields { get; } =
        new Dictionary<string, string>
        {
            { "IntegrationTestFixture","_fixture" },
            { "HttpClient","client" }
        };
        public List<string> UsingDirectives { get; } =
        new List<string>
        {
            "System.Net.Http",
            "System.Threading.Tasks",
            "Xunit",
            "Xunit.Priority",
            "Xunit.Categories"
        };

        public List<string> Atributes { get; } =
        new List<string>
        {
            "Collection(nameof(IntegrationApiTestFixtureCollection))",
            "TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)"
        };

        public string Constructor { get; }

        public void AddTestMethods(IEnumerable<TestMethod> testMethod) => this.TestMethods.AddRange(testMethod);
        public void AddUsingDirective(string usingDirective) => this.UsingDirectives.Add(usingDirective);

        public override string ToString()
        {
            string testClass = "";

            // Adds any using directives at the top of the file
            string usigns = "";
            foreach (var usingDirective in this.UsingDirectives)
                usigns += ("using" + usingDirective + ";\n");
            testClass += usigns;

            testClass += "namespace " + this.Namespace + "\n";
            testClass += "{\n";

            // Adds the class definition
            testClass += "\t" + this.Definition+"\n";
            testClass += "\t{\n";

            // Adds any fields the test class may need
            string fields = "";
            foreach (var field in this.Fields)
                fields += "\tprivate " + field.Key + " " + field.Value + ";\n";

            // Adds the constructor to the test class
            testClass += this.Constructor;

            // Adds the test methods to the test class
            string testMethods = "";
            foreach (var testMethod in this.TestMethods)
            {
                testMethods += "\t[Fact]\n";
                testMethods += testMethod.ToString();
            }
            testClass += testMethods;

            testClass += "\t}\n";
            testClass += "}\n";


            return testClass;
        }
    }
}
