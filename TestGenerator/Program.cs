﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Reflection;

namespace TestGenerator
{
    public class Program
    {
        public static FileController fileController = new FileController();
        public static TestController testController = new TestController();
        public static void Main(string[] args)
        {
            var controllers = fileController.ReadControllers();

            var controllersTests = testController.CreateTests(controllers);

            fileController.WriteTests(controllersTests);
        }

        static IEnumerable<Assembly> GetAssemblies(string mainPath) => new List<Assembly>();
        static void MM(string[] args)
        {
            var controllersClasses = GetAssemblies(args[0])
                .Select(d => d.GetControllerTypes())
                .Where(d => args[1].Split(',').Contains(d.Name));

            var testes = controllersClasses.Select(d => d.GetTestsFromController());
        }
    }

    public static class AssemblyExtensions
    {
        public static Type GetControllerTypes(this Assembly assembly)
        {
            return null;
        }
    }

    public static class TypeExtensions
    {
        public static IEnumerable<TestMethod2> GetTestsFromController(this Type controllerType)
        {
            var methods = controllerType.GetMethods()
                .Where(d => d.GetCustomAttribute<GetAttribute>() is object);
            foreach (var method in methods)
                yield return new TestMethod2(controllerType, method);
        }
    }

    public class GetAttribute : Attribute { }

    public class TestMethod2
    {
        public Type ControllerType { get; }
        public TestMethod2(Type controller, MethodInfo methodInfo)
        {
            ControllerType = controller;
        }
    }

    public class MinhaTestClass
    {
        public IEnumerable<string> Usings { get; }
        public IEnumerable<TestMethod2> TestMethods { get; }

        public MinhaTestClass(IEnumerable<TestMethod2> testMethods)
        {
            
        }
    }
}
