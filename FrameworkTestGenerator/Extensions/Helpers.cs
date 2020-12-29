using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FrameworkTestGenerator.Extensions
{
    public static class Helpers
    {
        public static IEnumerable<string> GetRouteParameters(this string route) => 
            Regex.Matches(route, @"\{(.*?)\}")
            .OfType<Match>().Select(m => m.Value)
            .Select(s => s.Substring(1, s.Length - 2));

        public static bool ContainsIgnoreCase(this IEnumerable<string> source, string toCheck, StringComparison comp = StringComparison.OrdinalIgnoreCase) =>
            source.Any(s => s.Equals(toCheck, StringComparison.OrdinalIgnoreCase));
    }

}
