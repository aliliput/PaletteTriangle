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
    }
}
