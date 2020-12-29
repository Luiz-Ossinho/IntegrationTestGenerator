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
using FrameworkTestGenerator.Extensions;

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
                if (controller.Name == "BaseController")
                    continue;
                var controllerTestClass = CreateTestClass(controller);
                Console.WriteLine("Test class: " + controllerTestClass.Filename + ".cs");
                Console.WriteLine(controllerTestClass.ToString());
                Console.WriteLine();
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
            foreach (var endpoint in controller.GetMethods().GetEndpoints())
                testMethods.Add(GenerateTestMethod(endpoint, ref testClass, routePrefix));

            testClass.AddTestMethods(testMethods);

            return testClass;
        }

        private TestMethod GenerateTestMethod(MethodInfo endpoint, ref TestClass testClass, string routePrefix)
        {
            TestMethod testMethod = new TestMethod(endpoint.Name);

            if (endpoint.HasRoute())
                testMethod.Route = $"{routePrefix}/{endpoint.GetCustomAttribute<RouteAttribute>().Template}";
            else
                testMethod.Route = $"{routePrefix}";

            var routeParameters = testMethod.Route.GetRouteParameters();

            AddParameters(ref testMethod, endpoint.GetParameters(), routeParameters, ref testClass);

            testMethod.Method = endpoint.GetHTTPMethod();

            return testMethod;
        }
        private void AddParameters(ref TestMethod testMethod, ParameterInfo[] parameters, IEnumerable<string> routeParameters, ref TestClass testClass)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.IsIntegerParameter())
                {
                    testMethod.AddInitialization(parameter.Name, "123");
                    if (!routeParameters.ContainsIgnoreCase(parameter.Name))
                        testMethod.AddHeader(parameter.Name);
                }
                else if (parameter.IsDecimalParameter())
                {
                    testMethod.AddInitialization(parameter.Name, "decimal.Parse(\"123\")");
                    if (!routeParameters.ContainsIgnoreCase(parameter.Name))
                        testMethod.AddHeader(parameter.Name);
                }
                else if (parameter.IsStringParameter())
                {
                    testMethod.AddInitialization(parameter.Name, "\"ABC\"");
                    if (!routeParameters.ContainsIgnoreCase(parameter.Name))
                        testMethod.AddHeader(parameter.Name);
                }
                else if (parameter.IsDateTimeParameter())
                {
                    testMethod.AddInitialization(parameter.Name, "new DateTime(1999,1,1)");
                    if (!routeParameters.ContainsIgnoreCase(parameter.Name))
                        testMethod.AddHeader(parameter.Name);
                }
                else if (parameter.IsBooleanParameter())
                {
                    testMethod.AddInitialization(parameter.Name, "false");
                    if (!routeParameters.ContainsIgnoreCase(parameter.Name))
                        testMethod.AddHeader(parameter.Name);
                }
                else
                {
                    if (parameter.ParameterType.IsGenericType)
                    {
                        var baseType = parameter.ParameterType.Name.Remove(parameter.ParameterType.Name.IndexOf('`'));
                        var genericType = parameter.ParameterType.GetGenericArguments()[0];
                        testMethod.AddInitialization(parameter.Name, $"new {baseType}<{genericType.Name}> {{ }}");
                        testClass.AddUsingDirective(genericType.Namespace);
                    }
                    else
                    {
                        testMethod.AddInitialization(parameter.Name, $"new {parameter.ParameterType.Name} {{ }}");
                        testClass.AddUsingDirective(parameter.ParameterType.Namespace);
                    }
                        

                    
                    testMethod.AddBody(parameter.Name);
                }
            }
        }
    }
}
