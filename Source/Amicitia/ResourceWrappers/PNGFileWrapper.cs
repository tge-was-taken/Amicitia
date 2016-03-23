using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace Amicitia.ResourceWrappers
{
    // TODO: make this into a generic Bitmap wrapper
    internal class PNGFileWrapper : ResourceWrapper
    {
        /*********************/
        /* File filter types */
        /*********************/
        public static readonly new SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.PNGFile
        };

        /*****************************************/
        /* Import / Export delegate dictionaries */
        /*****************************************/
       public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ImportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.PNGFile, (res, path) =>
                res.WrappedObject = new Bitmap(path)
            }
        };

        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ExportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.PNGFile, (res, path) =>
                (res as PNGFileWrapper).WrappedObject.Save(path, System.Drawing.Imaging.ImageFormat.Png)
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
        public PNGFileWrapper(string text, Bitmap bitmap) 
            : base(text, bitmap, SupportedFileType.PNGFile, false)
        {
            m_isImage = true;
        }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new Bitmap WrappedObject
        {
            get
            {
                return (Bitmap)m_wrappedObject;
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
    }
}
