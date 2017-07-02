using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using AtlusLibSharp.Graphics.TMX;
using AtlusLibSharp.PS2.Graphics;

namespace Amicitia.ResourceWrappers
{
    //[TypeConverter(typeof(PropertySorter))]
    public class TmxFileWrapper : ResourceWrapper<TmxFile>
    {
        [Category("Texture dimensions")]
        [OrderedProperty]
        public ushort Width => Resource.Width;

        [Category("Texture dimensions")]
        [OrderedProperty]
        public ushort Height => Resource.Height;

        [Category("Texture format")]
        [OrderedProperty]
        public PS2PixelFormat PixelFormat => Resource.PixelFormat;

        [Category("Texture format")]
        [OrderedProperty]
        public bool UsesPalette => Resource.UsesPalette;

        [Category("Texture format")]
        [OrderedProperty]
        public byte PaletteCount => Resource.PaletteCount;

        [Category("Texture format")]
        [OrderedProperty]
        public PS2PixelFormat PaletteFormat => Resource.PaletteFormat;

        [Category("Texture format")]
        [OrderedProperty]
        public byte MipMapCount => Resource.MipMapCount;

        [Category("Texture wrapping modes")]
        [OrderedProperty]
        public TmxWrapMode HorizontalWrappingMode
        {
            get => Resource.HorizontalWrappingMode;
            set => SetProperty(Resource, value);
        }

        [Category("Texture wrapping modes")]
        [OrderedProperty]
        public TmxWrapMode VerticalWrappingMode
        {
            get => Resource.VerticalWrappingMode;
            set => SetProperty(Resource, value);
        }

        [Category("Texture metadata")]
        [OrderedProperty]
        public int UserTextureId
        {
            get => Resource.UserTextureId;
            set => SetProperty(Resource, value);
        }

        [Category("Texture metadata")]
        [OrderedProperty]
        public int UserClutId
        {
            get => Resource.UserClutId;
            set => SetProperty(Resource, value);
        }

        [Category("Texture metadata")]
        [OrderedProperty]
        public string UserComment
        {
            get => Resource.UserComment;
            set => SetProperty(Resource, value);
        }

        public TmxFileWrapper(string text, TmxFile resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Move |
                                       CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.TmxFile, (res, path) => res.Save(path));
            RegisterFileExportAction(SupportedFileType.Bitmap, (res, path) => res.GetBitmap().Save(path, ImageFormat.Png));
            RegisterFileReplaceAction(SupportedFileType.TmxFile, (res, path) => TmxFile.Load(path));
            RegisterFileReplaceAction(SupportedFileType.Bitmap, (res, path) => new TmxFile(new Bitmap(path), res.PixelFormat, res.UserComment));
        }

        protected override void PopulateView()
        {
        }
    }
}
