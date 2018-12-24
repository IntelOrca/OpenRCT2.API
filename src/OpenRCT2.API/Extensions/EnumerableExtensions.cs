using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenRCT2.API.Extensions
{
    public static class EnumerableExtensions
    {
        public static IOrderedEnumerable<T> ThenByNaturalDescending<T>(this IOrderedEnumerable<T> source, Func<T, string> selector)
        {
            var max = source
                .SelectMany(i => Regex.Matches(selector(i), @"\d+").Cast<Match>().Select(m => (int?)m.Value.Length))
                .Max() ?? 0;

            return source.ThenByDescending(i => Regex.Replace(selector(i), @"\d+", m => m.Value.PadLeft(max, '0')));
        }
    }
}
