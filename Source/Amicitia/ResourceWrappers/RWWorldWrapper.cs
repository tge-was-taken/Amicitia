using AmicitiaLibrary.Graphics.RenderWare;

namespace Amicitia.ResourceWrappers
{
    public class RwWorldWrapper : RwNodeWrapperBase<RwWorld>
    {
        public RwWorldWrapper( string text, RwNode resource ) : base( text, resource )
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction( SupportedFileType.AssimpModelFile, ( res, path ) => res.ExportToDae(path) );
        }

        protected override void PopulateView()
        {
        }
    }
}
