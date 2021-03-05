using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;

namespace TestGenerator
{

    public class TestMethod
    {
        public TestMethod(string endpointName)
        {
            this.Contract = $"public async Task {endpointName}";
        }
        public string Contract { get; set; }
        public HttpMethod Method { get; set; }
        public string Route { get; set; }
        public string Body { get; private set; } = "";
        public IDictionary<string, string> Initializations { get; } = new Dictionary<string, string>();
        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public List<string> Asserts { get; } = new List<string>
        {
            "Assert.True(response.IsSuccessStatusCode, response.LerConteudo());"
        };
        public void AddHeader(string variable) => this.Headers.Add(variable, variable);
        public void AddInitialization(string key, string value) => this.Initializations.Add(key, value);
        public void AddBody(string variable) => this.Body = variable;
        public override string ToString()
        {
            string testBody =
                "\t{\n" +
                "\t\t//Arrange\n";

            string arranges = "";
            //Initializes any variables that may need initializing
            foreach (var initialization in this.Initializations)
            {
                arranges += $"\t\tvar {initialization.Key} = {initialization.Value};\n";
            }

            testBody += arranges;

            string httpMethod = "";
            if (this.Method == HttpMethod.Get)
                httpMethod = "GetAsync";
            if (this.Method == HttpMethod.Post)
                httpMethod = "PostAsJsonAsync";
            if (this.Method == HttpMethod.Delete)
                httpMethod = "DeleteAsync";
            if (this.Method == HttpMethod.Put)
                httpMethod = "PutAsJsonAsync";

            testBody += "\t\t//Act\n";
            string actions = "\t\tvar response = await client.";
            actions += httpMethod + $"($\"{this.Route}\"";

            // Adds any headers there may be
            if (Headers.Any())
            {
                actions += "\n";
                var headers = Headers.ToArray();
                for (int i = 0; i < headers.Length; i++)
                {
                    if (i == 0)
                        actions += $"\t\t\t+ $\"?{headers[i].Key}={{{headers[i].Value}}}\"\n";
                    else
                        actions += $"\t\t\t+ $\"&{headers[i].Key}={{{headers[i].Value}}}\"\n";
                }
            }

            // Adds the body to the request if there is one
            if (string.IsNullOrEmpty(this.Body) && (this.Method == HttpMethod.Put || this.Method == HttpMethod.Post))
                actions += ", new{ }";
            else if (!string.IsNullOrEmpty(this.Body))
                actions += $", {this.Body}";


            actions += "\t);\n";
            testBody += actions;

            // Adds any assertitions
            testBody += "\t\t//Assert\n";
            string assertions = "";
            foreach (var assertion in Asserts)
                assertions += "\t\t" + assertion + "\n";
            testBody += assertions;

            testBody += "\t}\n";

            return testBody;
        }
    }
}
