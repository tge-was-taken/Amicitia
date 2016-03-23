namespace Amicitia.ResourceWrappers
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;
    using AtlusLibSharp.Graphics.TGA;
    using System.Collections.Generic;

    internal class TGAFileWrapper : ResourceWrapper
    {
        /*********************/
        /* File filter types */
        /*********************/
        public static readonly new SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.TGAFile, SupportedFileType.PNGFile
        };

        /*****************************************/
        /* Import / Export delegate dictionaries */
        /*****************************************/
       public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ImportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.TGAFile, (res, path) =>
                res.WrappedObject = new TGAFile(path)
            },
            {
                SupportedFileType.PNGFile, (res, path) =>
                res.WrappedObject = new TGAFile(path, (res as TGAFileWrapper).WrappedObject.Encoding, (res as TGAFileWrapper).BitsPerPixel, (res as TGAFileWrapper).PaletteDepth)
            }
        };

        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ExportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.TGAFile, (res, path) =>
                (res as TGAFileWrapper).WrappedObject.Save(path)
            },
            {
                SupportedFileType.PNGFile, (res, path) =>
                (res as TGAFileWrapper).WrappedObject.GetBitmap().Save(path, System.Drawing.Imaging.ImageFormat.Png)
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
        public TGAFileWrapper(string text, TGAFile tga) 
            : base(text, tga, SupportedFileType.TGAFile, false)
        {
            m_isImage = true;
        }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new TGAFile WrappedObject
        {
            get
            {
                return (TGAFile)m_wrappedObject;
            }
            set
            {
                SetProperty(ref m_wrappedObject, value);
            }
        }

        public int Width
        {
            get { return WrappedObject.Width; }
        }

        public int Height
        {
            get { return WrappedObject.Height; }
        }

        public TGAEncoding PixelFormat
        {
            get { return WrappedObject.Encoding; }
        }

        public int BitsPerPixel
        {
            get { return WrappedObject.BitsPerPixel; }
        }

        public int PaletteDepth
        {
            get { return WrappedObject.PaletteDepth; }
        }

        public bool IsIndexed
        {
            get { return WrappedObject.IsIndexed; }
        }
    }
}
