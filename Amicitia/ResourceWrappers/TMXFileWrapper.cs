namespace Amicitia.ResourceWrappers
{
    using System;
    using System.Windows.Forms;
    using System.ComponentModel;
    using AtlusLibSharp.PS2.Graphics;
    using AtlusLibSharp.Graphics.TMX;

    internal class TMXFileWrapper : ResourceWrapper
    {
        protected internal static readonly SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.TMXFile, SupportedFileType.PNGFile
        };

        public TMXFileWrapper(string text, TMXFile tmx) 
            : base(text, tmx)
        {
        }

        public SupportedFileType FileType
        {
            get { return SupportedFileType.TMXFile; }
        }

        [Category("Texture info")]
        public ushort Width
        {
            get { return WrappedObject.Width; }
        }

        [Category("Texture info")]
        public ushort Height
        {
            get { return WrappedObject.Height; }
        }

        [Category("Texture info")]
        public PS2PixelFormat PixelFormat
        {
            get { return WrappedObject.PixelFormat; }
        }

        [Category("Texture info")]
        public bool UsesPalette
        {
            get { return WrappedObject.UsesPalette; }
        }

        [Category("Texture info")]
        public byte PaletteCount
        {
            get { return WrappedObject.PaletteCount; }
        }

        [Category("Texture info")]
        public PS2PixelFormat PaletteFormat
        {
            get { return WrappedObject.PaletteFormat; }
        }

        [Category("Texture info")]
        public byte MipMapCount
        {
            get { return WrappedObject.MipMapCount; }
        }

        [Category("Texture settings")]
        public TMXWrapMode HorizontalWrappingMode
        {
            get { return WrappedObject.HorizontalWrappingMode; }
            set { WrappedObject.HorizontalWrappingMode = value; }
        }

        [Category("Texture settings")]
        public TMXWrapMode VerticalWrappingMode
        {
            get { return WrappedObject.VerticalWrappingMode; }
            set { WrappedObject.VerticalWrappingMode = value; }
        }

        [Category("Texture settings")]
        public int TextureID
        {
            get { return WrappedObject.UserTextureID; }
            set { WrappedObject.UserTextureID = value; }
        }

        [Category("Texture settings")]
        public int ClutID
        {
            get { return WrappedObject.UserClutID; }
            set { WrappedObject.UserClutID = value; }
        }

        [Category("Texture settings")]
        public string Comment
        {
            get { return WrappedObject.UserComment; }
            set { WrappedObject.UserComment = value; }
        }

        protected internal new TMXFile WrappedObject
        {
            get { return (TMXFile)base.WrappedObject; }
            set { base.WrappedObject = value; }
        }

        protected internal override bool IsImageResource
        {
            get { return true; }
        }

        // TODO: Implement texture encoder form
        /*
        protected internal override bool CanEncode
        {
            get { return true; }
        }
        */

        public override void Export(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDlg = new SaveFileDialog())
            {
                saveFileDlg.FileName = Text;
                saveFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(FileFilterTypes);

                if (saveFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // rebuild before export
                RebuildWrappedObject();

                switch (FileFilterTypes[saveFileDlg.FilterIndex-1])
                {
                    case SupportedFileType.TMXFile:
                        WrappedObject.Save(saveFileDlg.FileName);
                        break;
                    case SupportedFileType.PNGFile:
                        WrappedObject.GetBitmap().Save(saveFileDlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                }
            }
        } 

        public override void Replace(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDlg = new OpenFileDialog())
            {
                openFileDlg.FileName = Text;
                openFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(FileFilterTypes);

                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                switch (FileFilterTypes[openFileDlg.FilterIndex-1])
                {
                    case SupportedFileType.TMXFile:
                        WrappedObject = TMXFile.Load(openFileDlg.FileName);
                        break;
                    case SupportedFileType.PNGFile:
                        WrappedObject = new TMXFile(
                            (System.Drawing.Bitmap)System.Drawing.Image.FromFile(openFileDlg.FileName),
                            PixelFormat,
                            Comment);
                        break;
                }

                // re-init wrapper
                InitializeWrapper();
            }
        }
    }
}
