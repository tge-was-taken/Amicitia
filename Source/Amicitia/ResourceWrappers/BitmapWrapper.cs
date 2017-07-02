using System.Drawing;

namespace Amicitia.ResourceWrappers
{
    public class BitmapWrapper : ResourceWrapper<Bitmap>
    {
        public int Width => Resource.Width;

        public int Height => Resource.Height;

        public BitmapWrapper(string text, Bitmap resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Move |
                                       CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.Bitmap, (res, path) => res.Save(path));
            RegisterFileReplaceAction(SupportedFileType.Bitmap, (res, path) => new Bitmap(path));
        }

        protected override void PopulateView()
        {
        }
    }
}
