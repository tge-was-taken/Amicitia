namespace Amicitia.ResourceWrappers
{
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    using AtlusLibSharp.Graphics.RenderWare;
    using AtlusLibSharp.PS2.Graphics;

    internal class RWTextureNativeWrapper : ResourceWrapper
    {
        /*********************/
        /* File filter types */
        /*********************/
        public static readonly new SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.RWTextureNative, SupportedFileType.PNGFile
        };

        /*****************************************/
        /* Import / Export delegate dictionaries */
        /*****************************************/
        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ImportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.RWTextureNative, (res, path) =>
                res.WrappedObject = RWNode.Load(path)
            },
            {
                SupportedFileType.PNGFile, (res, path) =>
                res.WrappedObject = new RWTextureNative(path, (res as RWTextureNativeWrapper).WrappedObject.Tex0Register.TexturePixelFormat)
            }
        };

        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ExportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.RWTextureNative, (res, path) =>
                (res as RWTextureNativeWrapper).WrappedObject.Save(path)
            },
            {
                SupportedFileType.PNGFile, (res, path) =>
                (res as RWTextureNativeWrapper).WrappedObject.GetBitmap().Save(path, System.Drawing.Imaging.ImageFormat.Png)
            }
        };

        /************************************/
        /* Import / export method overrides */
        /************************************/
        protected override Dictionary<SupportedFileType, Action<ResourceWrapper, string>> GetImportDelegates()
        {
            return ImportDelegates;
        }

        protected override Dictionary<SupportedFileType, Action<ResourceWrapper, string>> GetExportDelegates()
        {
            return ExportDelegates;
        }

        protected override SupportedFileType[] GetSupportedFileTypes()
        {
            return FileFilterTypes;
        }

        /***************/
        /* Constructor */
        /***************/
        public RWTextureNativeWrapper(RWTextureNative textureNative) 
            : base(textureNative.Name, textureNative, SupportedFileType.RWTextureNative, false)
        {
            m_isImage = true;
        }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new RWTextureNative WrappedObject
        {
            get
            {
                return (RWTextureNative)m_wrappedObject;
            }
            set
            {
                SetProperty(ref m_wrappedObject, value);
            }
        }

        public RWPlatformID PlatformID
        {
            get { return WrappedObject.PlatformID; }
        }

        public PS2FilterMode FilterMode
        {
            get { return WrappedObject.FilterMode; }
            set { SetProperty(WrappedObject, value); }
        }

        public PS2AddressingMode WrapModeX
        {
            get { return WrappedObject.HorrizontalAddressingMode; }
            set { SetProperty(WrappedObject, value); }
        }

        public PS2AddressingMode WrapModeY
        {
            get { return WrappedObject.VerticalAddressingMode; }
            set { SetProperty(WrappedObject, value); }
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

        /*********************************/
        /* Base wrapper method overrides */
        /*********************************/
        internal override void RebuildWrappedObject()
        {
            (m_wrappedObject as RWTextureNative).Name = Text;
            m_isDirty = false;
        }
    }
}
