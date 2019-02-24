using System.Collections.Generic;

namespace System.Linq
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> sequence, T item)
        {
            yield return item;
            foreach (var x in sequence)
                yield return x;
        }
    }
}
