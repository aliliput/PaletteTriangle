using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PaletteTriangle
{
    public static class CollectionUtil
    {
        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
        {
            var readOnlyCollection = source as ReadOnlyCollection<T>;
            if (readOnlyCollection != null) return readOnlyCollection;

            var list = source as IList<T>;
            if (list != null) return new ReadOnlyCollection<T>(list);

            return new ReadOnlyCollection<T>(source.ToArray());
        }

        public static IEnumerable<TResult> ThroughError<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (var elm in source)
            {
                var result = default(TResult);
                var raisedException = false;
                try
                {
                    result = selector(elm);
                }
                catch
                {
                    raisedException = true;
                }

                if (!raisedException)
                    yield return result;
            }
        }
    }
}
