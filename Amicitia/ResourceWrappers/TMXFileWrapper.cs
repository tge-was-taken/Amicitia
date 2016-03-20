namespace Amicitia.ResourceWrappers
{
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using AtlusLibSharp.PS2.Graphics;
    using AtlusLibSharp.Graphics.TMX;

    internal class TMXFileWrapper : ResourceWrapper
    {
        /*********************/
        /* File filter types */
        /*********************/
        public static readonly new SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.TMXFile, SupportedFileType.PNGFile
        };

        /*****************************************/
        /* Import / Export delegate dictionaries */
        /*****************************************/
        public static new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ImportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.TMXFile, (res, path) => 
                res.WrappedObject = TMXFile.Load(path)
            },
            {
                SupportedFileType.PNGFile, (res, path) =>
                res.WrappedObject = new TMXFile(new Bitmap(path), (res as TMXFileWrapper).PixelFormat, (res as TMXFileWrapper).UserComment)
            }
        };

        public static new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ExportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.TMXFile, (res, path) =>
                (res as TMXFileWrapper).WrappedObject.Save(path)
            },
            {
                SupportedFileType.PNGFile, (res, path) =>
                (res as TMXFileWrapper).WrappedObject.GetBitmap().Save(path, ImageFormat.Png)
            }
        };

        // virtual delegate shenanigans
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
        public TMXFileWrapper(string text, TMXFile tmx) 
            : base(text, tmx, SupportedFileType.TMXFile, false)
        {
            m_isImage = true;
        }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new TMXFile WrappedObject
        {
            get
            {
                return (TMXFile)m_wrappedObject;
            }
            set
            {
                SetProperty(ref m_wrappedObject, value);
            }
        }

        public ushort Width
        {
            get { return WrappedObject.Width; }
        }

        public ushort Height
        {
            get { return WrappedObject.Height; }
        }

        public PS2PixelFormat PixelFormat
        {
            get { return WrappedObject.PixelFormat; }
        }

        public bool UsesPalette
        {
            get { return WrappedObject.UsesPalette; }
        }

        public byte PaletteCount
        {
            get { return WrappedObject.PaletteCount; }
        }

        public PS2PixelFormat PaletteFormat
        {
            get { return WrappedObject.PaletteFormat; }
        }

        public byte MipMapCount
        {
            get { return WrappedObject.MipMapCount; }
        }

        public TMXWrapMode HorizontalWrappingMode
        {
            get { return WrappedObject.HorizontalWrappingMode; }
            set { SetProperty(WrappedObject, value); }
        }

        public TMXWrapMode VerticalWrappingMode
        {
            get { return WrappedObject.VerticalWrappingMode; }
            set { SetProperty(WrappedObject, value); }
        }

        public int UserTextureID
        {
            get { return WrappedObject.UserTextureID; }
            set { SetProperty(WrappedObject, value); }
        }

        public int UserClutID
        {
            get { return WrappedObject.UserClutID; }
            set { SetProperty(WrappedObject, value); }
        }

        public string UserComment
        {
            get { return WrappedObject.UserComment; }
            set { SetProperty(WrappedObject, value); }
        }
    }
}
