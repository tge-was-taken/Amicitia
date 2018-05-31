using System.Windows.Forms;
using AmicitiaLibrary.FileSystems.ACX;
using AmicitiaLibrary.IO;

namespace Amicitia.ResourceWrappers
{
    public class AcxFileWrapper : ResourceWrapper<AcxFile>
    {
        public AcxFileWrapper( string text, AcxFile resource ) : base( text, resource )
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Add |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction( SupportedFileType.AcxFile, ( res, path ) => res.Save( path ) );
            RegisterFileReplaceAction( SupportedFileType.AcxFile, ( res, path ) => new AcxFile( path ) );
            RegisterFileAddAction( SupportedFileType.Resource, DefaultFileAddAction );
            RegisterRebuildAction( ( wrap ) =>
            {
                AcxFile file = new AcxFile();

                foreach ( IResourceWrapper entry in Nodes )
                {
                    file.Entries.Add( new BinaryFile(entry.GetResourceBytes()) );
                }

                return file;
            } );
        }

        protected override void PopulateView()
        {
            for (int i = 0; i < Resource.Entries.Count; i++ )
            {
                Nodes.Add( ( TreeNode )ResourceWrapperFactory.GetResourceWrapper( $"Sound{i:D2}.adx", Resource.Entries[i] ) );
            }
        }
    }
}
