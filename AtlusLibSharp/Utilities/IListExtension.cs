namespace AtlusLibSharp.Utilities
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides convenience methods for objects that implement the <see cref="IList{T}"/> interface.
    /// </summary>
    public static class IListExtension
    {
        /// <summary>
        /// Retrieves the last entry of the list.
        /// </summary>
        public static T GetLast<T>(this IList<T> list)
        {
            return list[list.Count - 1];
        }

        /// <summary>
        /// Retrieves the previous entry of the list given the current index.
        /// </summary>
        public static T GetPrevious<T>(this IList<T> list, int curIndex)
        {
            return list[curIndex - 1];
        }

        public static void AddRange<T>(this IList<T> list, params T[] items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }
    }
}
