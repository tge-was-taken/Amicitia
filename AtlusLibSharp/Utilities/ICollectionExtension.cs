namespace AtlusLibSharp.Utilities
{
    using System.Collections.Generic;

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
