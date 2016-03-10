namespace Amicitia.ResourceWrappers
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;
    using AtlusLibSharp.Graphics.TGA;

    internal class TGAFileWrapper : ResourceWrapper
    {
        protected internal static readonly SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.TGAFile, SupportedFileType.PNGFile
        };

        public TGAFileWrapper(string text, TGAFile tga) 
            : base(text, tga)
        {
        }

        public SupportedFileType FileType
        {
            get { return SupportedFileType.TGAFile; }
        }

        [Category("Texture info")]
        public int Width
        {
            get { return WrappedObject.Width; }
        }

        [Category("Texture info")]
        public int Height
        {
            get { return WrappedObject.Height; }
        }

        [Category("Texture info")]
        public TGAEncoding PixelFormat
        {
            get { return WrappedObject.Encoding; }
        }

        [Category("Texture info")]
        public int BitsPerPixel
        {
            get { return WrappedObject.BitsPerPixel; }
        }

        [Category("Texture info")]
        public int PaletteDepth
        {
            get { return WrappedObject.PaletteDepth; }
        }

        [Category("Texture info")]
        public bool IsIndexed
        {
            get { return WrappedObject.IsIndexed; }
        }

        protected internal new TGAFile WrappedObject
        {
            get { return (TGAFile)base.WrappedObject; }
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

                switch (FileFilterTypes[saveFileDlg.FilterIndex - 1])
                {
                    case SupportedFileType.TGAFile:
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

                switch (FileFilterTypes[openFileDlg.FilterIndex - 1])
                {
                    case SupportedFileType.TGAFile:
                        WrappedObject = new TGAFile(openFileDlg.FileName);
                        break;
                        
                    case SupportedFileType.PNGFile:
                        WrappedObject = new TGAFile(
                            (System.Drawing.Bitmap)System.Drawing.Image.FromFile(openFileDlg.FileName),
                            PixelFormat);
                        break;
                        
                }

                // re-init wrapper
                InitializeWrapper();
            }
        }
    }
}
