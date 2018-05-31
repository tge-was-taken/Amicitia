using System.IO;
using System.Windows.Forms;
using AmicitiaLibrary.FileSystems.ListArchive;

namespace Amicitia.ResourceWrappers
{
    public class ListArchiveFileWrapper : ResourceWrapper<ListArchiveFile>
    {
        public int EntryCount => Nodes.Count;

        public ListArchiveFileWrapper(string text, ListArchiveFile resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Add |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.PakToolArchiveFile, (res, path) => res.Save(path));
            RegisterFileReplaceAction(SupportedFileType.PakToolArchiveFile, (res, path) => new ListArchiveFile(path));
            RegisterFileAddAction(SupportedFileType.Resource, DefaultFileAddAction);
            RegisterRebuildAction((wrap) =>
            {
                ListArchiveFile file = new ListArchiveFile();
                file.BigEndian = wrap.Resource.BigEndian;

                foreach (IResourceWrapper node in Nodes)
                {
                    file.Entries.Add(new ListArchiveEntry(node.Text, node.GetResourceBytes()));
                }

                return file;
            });         
        }

        protected override void PopulateView()
        {
            foreach (var entry in Resource.Entries)
            {
                Nodes.Add((TreeNode)ResourceWrapperFactory.GetResourceWrapper(entry.Name, new MemoryStream(entry.Data)));
            }
        }
    }
}
