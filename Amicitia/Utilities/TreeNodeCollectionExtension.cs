namespace Amicitia.Utilities
{
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
    }
}
