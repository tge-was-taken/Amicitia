using System.IO;
using System.Windows.Forms;
using AtlusLibSharp.FileSystems.PAKToolArchive;

namespace Amicitia.ResourceWrappers
{
    public class PakToolArchiveFileWrapper : ResourceWrapper<PakToolArchiveFile>
    {
        public int EntryCount => Nodes.Count;

        public PakToolArchiveFileWrapper(string text, PakToolArchiveFile resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Add |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.PakToolArchiveFile, (res, path) => res.Save(path));
            RegisterFileReplaceAction(SupportedFileType.PakToolArchiveFile, (res, path) => new PakToolArchiveFile(path));
            RegisterFileAddAction(SupportedFileType.Resource, DefaultFileAddAction);
            RegisterRebuildAction((wrap) =>
            {
                PakToolArchiveFile file = new PakToolArchiveFile();

                foreach (IResourceWrapper node in Nodes)
                {
                    file.Entries.Add(new PakToolArchiveEntry(node.Text, node.GetResourceBytes()));
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
