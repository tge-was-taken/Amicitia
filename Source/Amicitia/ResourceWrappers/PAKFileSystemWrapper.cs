using System.IO;
using System.Windows.Forms;
using AtlusFileSystemLibrary;
using AtlusFileSystemLibrary.FileSystems.PAK;

namespace Amicitia.ResourceWrappers
{
    public class PAKFileSystemWrapper : ResourceWrapper<PAKFileSystem>
    {
        public int EntryCount => Nodes.Count;

        public PAKFileSystemWrapper(string text, PAKFileSystem resource ) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Add |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.PakToolArchiveFile, (res, path) => res.Save(path));
            RegisterFileReplaceAction( SupportedFileType.PakToolArchiveFile, ( res, path ) =>
            {
                var pak = new PAKFileSystem();
                pak.Load( path );
                return pak;
            } );
            RegisterFileAddAction(SupportedFileType.Resource, DefaultFileAddAction);
            RegisterRebuildAction((wrap) =>
            {
                var file = new PAKFileSystem( Resource.Version );

                foreach (IResourceWrapper node in Nodes)
                {
                    file.AddFile(node.Text, node.GetResourceMemoryStream(), true, ConflictPolicy.Ignore);
                }

                return file;
            });
        }

        protected override void PopulateView()
        {
            foreach (var file in Resource.EnumerateFiles())
            {
                Nodes.Add( ( TreeNode ) ResourceWrapperFactory.GetResourceWrapper( file, Resource.OpenFile( file ) ) );
            }
        }
    }
}
