namespace Amicitia.Utilities
{
    using ResourceWrappers;
    using System.Windows.Forms;

    internal static class TreeNodeCollectionExtension
    {
        public static void Add(this TreeNodeCollection collection, params TreeNode[] treeNodes)
        {
            foreach (TreeNode item in treeNodes)
            {
                collection.Add(item);
            }
        }

        public static void Add(this TreeNodeCollection collection, params IResourceWrapper[] treeNodes)
        {
            foreach (TreeNode item in treeNodes)
            {
                collection.Add(item);
            }
        }
    }
}
