using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkTestGenerator.Extensions
{
    public static class ParameterExtensions
    {
        public static bool IsType<T>(this ParameterInfo parameter) => parameter.ParameterType == typeof(T);
        public static bool IsType(this ParameterInfo parameter, params Type[] types) => types.Contains(parameter.ParameterType);
        public static bool IsIntegerParameter(this ParameterInfo parameter) =>
            parameter.IsType(typeof(int), typeof(int?),
                typeof(short), typeof(short?),
                typeof(long), typeof(long?),
                typeof(byte), typeof(byte?));
        public static bool IsBooleanParameter(this ParameterInfo parameter) =>
    parameter.IsType(typeof(bool), typeof(bool?));
        public static bool IsDecimalParameter(this ParameterInfo parameter) =>
            parameter.IsType(typeof(decimal), typeof(decimal?));
        public static bool IsDateTimeParameter(this ParameterInfo parameter) =>
            parameter.IsType(typeof(DateTime), typeof(DateTime?));
        public static bool IsStringParameter(this ParameterInfo parameter) =>
            parameter.IsType<string>();
    }
}
