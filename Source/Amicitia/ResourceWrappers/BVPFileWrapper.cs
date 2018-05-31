using System.IO;
using System.Windows.Forms;
using AmicitiaLibrary.FileSystems.BVP;

namespace Amicitia.ResourceWrappers
{
    public class BvpFileWrapper : ResourceWrapper<BvpFile>
    {
        public int EntryCount => Nodes.Count;

        public BvpFileWrapper(string text, BvpFile resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Add |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.BvpArchiveFile, (res, path) => res.Save(path));
            RegisterFileReplaceAction(SupportedFileType.BvpArchiveFile, (res, path) => new BvpFile(path));
            RegisterFileAddAction(SupportedFileType.Resource, DefaultFileAddAction);
            RegisterRebuildAction((wrap) =>
            {
                BvpFile file = new BvpFile();

                foreach (IResourceWrapper node in Nodes)
                {
                    file.Entries.Add(new BvpEntry(node.GetResourceBytes()));
                }

                return file;
            });            
        }

        protected override void PopulateView()
        {
            for (var i = 0; i < Resource.Entries.Count; i++)
            {
                Nodes.Add((TreeNode)ResourceWrapperFactory.GetResourceWrapper($"MessageScript{i:0000}", new MemoryStream(Resource.Entries[i].Data)));
            }
        }
    }
}
