using System.Collections.Generic;

namespace System.Linq
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Append<T>(this IEnumerable<T> sequence, T item)
        {
            foreach (var x in sequence)
                yield return x;
            yield return item;
        }

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> sequence, T item)
        {
            yield return item;
            foreach (var x in sequence)
                yield return x;
        }
    }
}
