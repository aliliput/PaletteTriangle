using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PaletteTriangle
{
    public static class CollectionUtil
    {
        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
        {
            return new ReadOnlyCollection<T>(source.ToArray());
        }
    }
}
