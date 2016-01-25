namespace AtlusLibSharp.Utilities
{
    using System.Collections.Generic;

    public static class IListExtension
    {
        /// <summary>
        /// Retrieves last entry of the list.
        /// Syntactic sugar for "list[list.Count - 1];"
        /// </summary>
        public static T GetLast<T>(this IList<T> list)
        {
            return list[list.Count - 1];
        }

        /// <summary>
        /// Retrieves previous entry of the list.
        /// Syntactic sugar for "list[curIndex - 1];"
        /// </summary>
        public static T GetPrevious<T>(this IList<T> list, int curIndex)
        {
            return list[curIndex - 1];
        }
    }
}
