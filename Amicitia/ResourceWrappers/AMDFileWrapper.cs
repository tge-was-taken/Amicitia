namespace Amicitia.ResourceWrappers
{
    using System.IO;
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    using AtlusLibSharp.FileSystems.AMD;
    using AtlusLibSharp.IO;

    internal class AMDFileWrapper : ResourceWrapper
    {
        /*********************/
        /* File filter types */
        /*********************/
        public static readonly new SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.AMDFile
        };

        /*****************************************/
        /* Import / Export delegate dictionaries */
        /*****************************************/
       public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ImportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.AMDFile, (res, path) =>
                res.WrappedObject = new AMDFile(path)
            }
        };

        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ExportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.AMDFile, (res, path) =>
                (res as AMDFileWrapper).WrappedObject.Save(path)
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
        public AMDFileWrapper(string text, AMDFile res) 
            : base(text, res, SupportedFileType.AMDFile, true)
        {
            m_canExport = false;
            m_canReplace = false;
            InitializeContextMenuStrip();
        }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new AMDFile WrappedObject
        {
            get { return (AMDFile)m_wrappedObject; }
            set { SetProperty(ref m_wrappedObject, value); }
        }

        /*********************************/
        /* Base wrapper method overrides */
        /*********************************/
        internal override void RebuildWrappedObject()
        {
            var archive = new AMDFile();
            foreach (ResourceWrapper node in Nodes)
            {
                var chunk = node.WrappedObject as AMDChunk;
                archive.Chunks.Add(chunk);
            }

            m_wrappedObject = archive;
            m_isDirty = false;
        }

        internal override void InitializeWrapper()
        {
            Nodes.Clear();

            int idx = 0;
            foreach (AMDChunk chunk in WrappedObject.Chunks)
            {
                var wrap = new ResourceWrapper(string.Format("{0}[{1}]", chunk.Tag, idx++), new GenericBinaryFile(chunk.Data), SupportedFileType.Resource, true);
                wrap.m_canReplace = false;
                wrap.m_canRename = false;
                wrap.InitializeContextMenuStrip();

                Nodes.Add(wrap);
            }

            base.InitializeWrapper();
        }
    }
}