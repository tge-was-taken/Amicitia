using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlusLibSharp.Utilities
{
    public static class ICollectionExtension
    {
        public static void Add<T>(this ICollection<T> collection, params T[] items)
        {
            foreach (T item in items)
            {
                collection.Add(item);
            }
        }
    }
}
