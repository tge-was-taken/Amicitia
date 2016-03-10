using System;
using System.Windows.Forms;
using AtlusLibSharp.Graphics.RenderWare;
using AtlusLibSharp.PS2.Graphics;

namespace Amicitia.ResourceWrappers
{
    internal class RWTextureNativeWrapper : ResourceWrapper
    {
        protected internal static readonly SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.RWTextureNative, SupportedFileType.PNGFile
        };

        public RWTextureNativeWrapper(RWTextureNative textureNative) 
            : base(textureNative.Name, textureNative)
        {
        }

        public SupportedFileType FileType
        {
            get { return SupportedFileType.RWTextureNative; }
        }

        public RWPlatformID PlatformID
        {
            get { return WrappedObject.PlatformID; }
        }

        public PS2FilterMode FilterMode
        {
            get { return WrappedObject.FilterMode; }
            set { WrappedObject.FilterMode = value; }
        }

        public PS2AddressingMode WrapModeX
        {
            get { return WrappedObject.HorrizontalAddressingMode; }
            set { WrappedObject.HorrizontalAddressingMode = value; }
        }

        public PS2AddressingMode WrapModeY
        {
            get { return WrappedObject.VerticalAddressingMode; }
            set { WrappedObject.VerticalAddressingMode = value; }
        }

        public int Width
        {
            get { return WrappedObject.Width; }
        }

        public int Height
        {
            get { return WrappedObject.Height; }
        }

        public int Depth
        {
            get { return WrappedObject.Depth; }
        }

        public PS2PixelFormat PixelFormat
        {
            get { return WrappedObject.Tex0Register.TexturePixelFormat; }
        }

        protected internal new RWTextureNative WrappedObject
        {
            get { return (RWTextureNative)base.WrappedObject; }
            set { base.WrappedObject = value; }
        }

        protected internal override bool IsImageResource
        {
            get { return true; }
        }

        // TODO: Implement a texture encoder form.
        /*
        protected internal override bool CanEncode
        {
            get { return true; }
        }
        */

        protected internal override void RebuildWrappedObject()
        {
            WrappedObject.Name = Text;
        }

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
                    case SupportedFileType.RWTextureNative:
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

#if !DEBUG
                try
                {
#endif
                    switch (FileFilterTypes[openFileDlg.FilterIndex - 1])
                    {
                        case SupportedFileType.RWTextureNative:
                            WrappedObject = (RWTextureNative)RWNode.LoadFromFile(openFileDlg.FileName);
                            break;

                        case SupportedFileType.PNGFile:
                            WrappedObject = new RWTextureNative(openFileDlg.FileName, WrappedObject.Tex0Register.TexturePixelFormat);
                            break;
                    }
#if !DEBUG
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Error occured.");
                }
#endif

                // re-init wrapper
                InitializeWrapper();
            }
        }
    }
}
