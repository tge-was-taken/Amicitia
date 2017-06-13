
using AtlusLibSharp.FileSystems.AMD;

namespace Amicitia.ResourceWrappers
{
    using System.IO;
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;

    internal class AMDChunkWrapper : ResourceWrapper
    {
        /*********************/
        /* File filter types */
        /*********************/
        public static readonly new SupportedFileType[] FileFilterTypes = {
            SupportedFileType.Resource
        };

        /*****************************************/
        /* Import / Export delegate dictionaries */
        /*****************************************/
       public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ImportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.Resource, (res, path) =>
                {
                    var amdChunk = (AMDChunk)res.WrappedObject;
                    res.WrappedObject = new AMDChunk(amdChunk.Tag, amdChunk.Flags, File.ReadAllBytes(path));
                }
            }
        };

        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ExportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.Resource, (res, path) =>
                File.WriteAllBytes(path, (res as AMDChunkWrapper).WrappedObject.Data)
            },
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
        public AMDChunkWrapper(string text, AMDChunk res) 
            : base(text, res, SupportedFileType.Resource, false /* set to true if context menu states were modified */ )
        {
			// set additional states here
        }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new AMDChunk WrappedObject
        {
            get { return (AMDChunk)m_wrappedObject; }
            set { SetProperty(ref m_wrappedObject, value); }
        }

        public string Tag
        {
            get { return WrappedObject.Tag; }
            set { WrappedObject.Tag = value; }
        }

        public int Flags
        {
            get { return WrappedObject.Flags; }
            set { WrappedObject.Flags = value; }
        }

        public int Size
        {
            get { return WrappedObject.Size; }
        }
    }
}