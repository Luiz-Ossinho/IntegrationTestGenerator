using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace FrameworkTestGenerator.Extensions
{
    public static class MethodExtensios
    {
        public static bool HasAttribute<T>(this MethodInfo method) where T : Attribute =>
            method.GetCustomAttribute<T>() != null;
        public static bool HasRoute(this MethodInfo endpoint) =>
            !string.IsNullOrEmpty(endpoint.GetCustomAttribute<RouteAttribute>()?.Template);
        public static IEnumerable<MethodInfo> GetEndpoints(this IEnumerable<MethodInfo> methods) =>
            methods.Where(m => ValidEndpointsAttributes
                                   .Intersect(m.GetCustomAttributes()
                                       .Select(a => a.GetType())).Any());
        public static HttpMethod GetHTTPMethod(this MethodInfo endpoint)
        {
            HttpMethod httpMethod = null;
            if (endpoint.HasAttribute<HttpGetAttribute>())
                httpMethod = HttpMethod.Get;
            else if (endpoint.HasAttribute<HttpPostAttribute>())
                httpMethod = HttpMethod.Post;
            else if (endpoint.HasAttribute<HttpPutAttribute>())
                httpMethod = HttpMethod.Put;
            else if (endpoint.HasAttribute<HttpDeleteAttribute>())
                httpMethod = HttpMethod.Delete;
            return httpMethod;
        }
        public static List<Type> ValidEndpointsAttributes { get; } = new List<Type>
        {
            typeof(HttpGetAttribute),
            typeof(HttpPostAttribute),
            typeof(HttpPutAttribute),
            typeof(HttpDeleteAttribute)
        };
    }
}
