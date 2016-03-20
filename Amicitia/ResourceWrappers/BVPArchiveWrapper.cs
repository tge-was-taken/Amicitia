namespace Amicitia.ResourceWrappers
{
    using System.IO;
    using System;
    using AtlusLibSharp.FileSystems.BVP;
    using System.ComponentModel;
    using System.Collections.Generic;

    internal class BVPFileWrapper : ResourceWrapper
    {
        /*********************/
        /* File filter types */
        /*********************/
        public static readonly new SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.BVPArchiveFile
        };

        /*****************************************/
        /* Import / Export delegate dictionaries */
        /*****************************************/
       public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ImportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.BVPArchiveFile, (res, path) =>
                res.WrappedObject = new BVPFile(path)
            }
        };

        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ExportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.BVPArchiveFile, (res, path) =>
                (res as BVPFileWrapper).WrappedObject.Save(path)
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
        public BVPFileWrapper(string text, BVPFile arc) 
            : base(text, arc, SupportedFileType.BVPArchiveFile, false)
        {
        }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new BVPFile WrappedObject
        {
            get
            {
                return (BVPFile)m_wrappedObject;
            }
            set
            {
                SetProperty(ref m_wrappedObject, value);
            }
        }

        public int EntryCount
        {
            get { return Nodes.Count; }
        }

        /*********************************/
        /* Base wrapper method overrides */
        /*********************************/
        internal override void RebuildWrappedObject()
        {
            var archive = new BVPFile();
            foreach (ResourceWrapper node in Nodes)
                archive.Entries.Add(new BVPEntry(node.GetBytes()));

            m_wrappedObject = archive;
            m_isDirty = false;
        }

        internal override void InitializeWrapper()
        {
            Nodes.Clear();

            int idx = 0;
            foreach (BVPEntry entry in WrappedObject.Entries)
                Nodes.Add(ResourceFactory.GetResource(string.Format("MessageData{0}", idx++), new MemoryStream(entry.Data)));

            base.InitializeWrapper();
        }
    }
}
