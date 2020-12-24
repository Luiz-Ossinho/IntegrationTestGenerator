using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace TestGenerator
{
    public class TestController
    {
        public TestController(string testClassNamespace, string SetupNamespace)
        {
            this.TestClassNamespace = testClassNamespace;
            this.SetupNamespace = SetupNamespace;
        }
        public string TestClassNamespace { get; }
        public string SetupNamespace { get; }
        private List<Type> ValidEndpointsAttributes { get; } = new List<Type>
        {
            typeof(HttpGetAttribute),
            typeof(HttpPostAttribute),
            typeof(HttpPutAttribute),
            typeof(HttpDeleteAttribute)
        };
        public List<Type> LoadTypes(List<FileInfo> files)
        {
            var controllerTypes = new List<Type>();
            foreach (var file in files)
            {
                Console.WriteLine("Loading file: " + file.Name + file.Exists);
                string strnamespace = Path.GetFileNameWithoutExtension(file.Name) + ".Controllers";

                Assembly assembly = Assembly.LoadFrom(file.FullName);

                controllerTypes.AddRange(assembly.GetTypes().Where(a => a.Namespace == strnamespace));
            }
            return controllerTypes;
        }

        public List<TestClass> CreateTests(List<Type> types)
        {
            var testClasses = new List<TestClass>();
            foreach (var controller in types)
            {
                // Incase there is no route prefix, convention dictates it must be an Base Controller
                // there are other ways to check this (for instance, check it is abstract)
                if (controller.GetCustomAttribute<RoutePrefixAttribute>()?.Prefix == null)
                    continue;
                var controllerTestClass = CreateTestClass(controller);
                Console.WriteLine(controllerTestClass.ToString());
                testClasses.Add(controllerTestClass);
            }

            return testClasses;
        }

        private TestClass CreateTestClass(Type controller)
        {
            var routePrefix = controller.GetCustomAttribute<RoutePrefixAttribute>()?.Prefix;

            var realizaLogin = false;
            if (controller.GetCustomAttribute<AuthorizeAttribute>() != null)
                realizaLogin = true;

            var testClass = new TestClass(controller.Name + "Tests", this.TestClassNamespace, this.SetupNamespace, realizaLogin);

            var testMethods = new List<TestMethod>();
            foreach (var endpoint in GetEndpoints(controller))
                testMethods.Add(GenerateTestMethod(endpoint, ref testClass, routePrefix));

            testClass.AddTestMethods(testMethods);

            return testClass;
        }

        private TestMethod GenerateTestMethod(MethodInfo endpoint, ref TestClass testClass, string routePrefix)
        {
            TestMethod testMethod = new TestMethod(endpoint.Name);

            if (HasRoute(endpoint))
                testMethod.Route = $"/{routePrefix}/{endpoint.GetCustomAttribute<RouteAttribute>().Template}";
            else
                testMethod.Route = $"/{routePrefix}";

            var routeParameters = GetRouteParameters(testMethod.Route);

            AddParameters(ref testMethod, endpoint.GetParameters(), routeParameters, ref testClass);

            testMethod.Method = SpecifyHTTPMethod(endpoint);

            return testMethod;
        }

        private IEnumerable<MethodInfo> GetEndpoints(Type controller)
        {
            return controller.GetMethods()
                               .Where(m => ValidEndpointsAttributes
                                   .Intersect(m.GetCustomAttributes()
                                       .Select(a => a.GetType())).Any());
        }

        private void AddParameters(ref TestMethod testMethod, ParameterInfo[] parameters, IEnumerable<string> routeParameters, ref TestClass testClass)
        {
            foreach (var parameter in parameters)
            {
                if (IsNumericParameter(parameter))
                {
                    testMethod.AddInitialization(parameter.Name, "123");
                    if (!routeParameters.Contains(parameter.Name))
                        testMethod.AddHeader(parameter.Name);
                }
                else if (IsDecimalParameter(parameter))
                {
                    testMethod.AddInitialization(parameter.Name, "decimal.Parse(\"123\")");
                    if (!routeParameters.Contains(parameter.Name))
                        testMethod.AddHeader(parameter.Name);
                }
                else if (IsStringParameter(parameter))
                {
                    testMethod.AddInitialization(parameter.Name, "\"ABC\"");
                    if (!routeParameters.Contains(parameter.Name))
                        testMethod.AddHeader(parameter.Name);
                }
                else
                {
                    testMethod.AddInitialization(parameter.Name, $"new {parameter.ParameterType} {{ }}");
                    testClass.AddUsingDirective(parameter.ParameterType.Namespace);
                    testMethod.AddBody(parameter.Name);
                }
            }
        }

        private HttpMethod SpecifyHTTPMethod(MethodInfo endpoint)
        {
            HttpMethod httpMethod = null;
            if (endpoint.GetCustomAttribute<HttpGetAttribute>() != null)
                httpMethod = HttpMethod.Get;
            else if (endpoint.GetCustomAttribute<HttpPostAttribute>() != null)
                httpMethod = HttpMethod.Post;
            else if (endpoint.GetCustomAttribute<HttpPutAttribute>() != null)
                httpMethod = HttpMethod.Put;
            else if (endpoint.GetCustomAttribute<HttpDeleteAttribute>() != null)
                httpMethod = HttpMethod.Delete;

            return httpMethod;
        }

        private IEnumerable<string> GetRouteParameters(string route) => Regex.Matches(route, @"\{(.*?)\}")
                .OfType<Match>().Select(m => m.Value)
                .Select(s => s.Substring(1, s.Length - 2));
        private bool HasRoute(MethodInfo endpoint) => !string.IsNullOrEmpty(endpoint.GetCustomAttribute<RouteAttribute>()?.Template);
        private bool IsNumericParameter(ParameterInfo parameter)
        {
            var isNumeric = false;
            if (parameter.ParameterType == typeof(int) || parameter.ParameterType == typeof(long))
                isNumeric = true;
            return isNumeric;
        }
        private bool IsDecimalParameter(ParameterInfo parameter)
        {
            var isDecimal = false;
            if (parameter.ParameterType == typeof(decimal))
                isDecimal = true;
            return isDecimal;
        }
        private bool IsStringParameter(ParameterInfo parameter)
        {
            var IsString = false;
            if (parameter.ParameterType == typeof(string))
                IsString = true;
            return IsString;
        }
    }
}
