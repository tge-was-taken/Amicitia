using System.Drawing.Imaging;
using AtlusLibSharp.Graphics.TGA;

namespace Amicitia.ResourceWrappers
{
    public class TgaFileWrapper : ResourceWrapper<TgaFile>
    {
        public int Width => Resource.Width;

        public int Height => Resource.Height;

        public TgaEncoding Encoding => Resource.Encoding;

        public int BitsPerPixel => Resource.BitsPerPixel;

        public int PaletteDepth => Resource.PaletteDepth;

        public bool IsIndexed => Resource.IsIndexed;

        public TgaFileWrapper(string text, TgaFile resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Move |
                                       CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.TgaFile, (res, path) => res.Save(path));
            RegisterFileExportAction(SupportedFileType.Bitmap, (res, path) => res.GetBitmap().Save(path, ImageFormat.Png));
            RegisterFileReplaceAction(SupportedFileType.TgaFile, (res, path) => new TgaFile(path));
            RegisterFileReplaceAction(SupportedFileType.Bitmap, (res, path) => new TgaFile(path, res.Encoding, res.BitsPerPixel, res.PaletteDepth));
        }

        protected override void PopulateView()
        {
        }
    }
}
