namespace Amicitia.ResourceWrappers
{
    using System.IO;
    using System;
    using AtlusLibSharp.FileSystems.ListArchive;
    using AtlusLibSharp.FileSystems.PAKToolArchive;
    using System.ComponentModel;
    using System.Collections.Generic;

    internal class ListArchiveFileWrapper : ResourceWrapper
    {
        /*********************/
        /* File filter types */
        /*********************/
        public static readonly new SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.ListArchiveFile, SupportedFileType.PAKToolArchiveFile
        };

        /*****************************************/
        /* Import / Export delegate dictionaries */
        /*****************************************/
       public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ImportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.ListArchiveFile, (res, path) =>
                res.WrappedObject = new ListArchiveFile(path)
            },
            {
                SupportedFileType.PAKToolArchiveFile, (res, path) =>
                res.WrappedObject = ListArchiveFile.Create(new PAKToolArchiveFile(path))
            }
        };

        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ExportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.ListArchiveFile, (res, path) =>
                (res as ListArchiveFileWrapper).WrappedObject.Save(path)
            },
            {
                SupportedFileType.PAKToolArchiveFile, (res, path) =>
                PAKToolArchiveFile.Create((res as ListArchiveFileWrapper).WrappedObject).Save(path)
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
        public ListArchiveFileWrapper(string text, ListArchiveFile arc) 
            : base(text, arc, SupportedFileType.ListArchiveFile, false)
        {
        }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new ListArchiveFile WrappedObject
        {
            get
            {
                return (ListArchiveFile)m_wrappedObject;
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
            var archive = new ListArchiveFile();
            foreach (ResourceWrapper node in Nodes)
            {
                archive.Entries.Add(new ListArchiveFileEntry(node.Text, node.GetMemoryStream()));
            }

            m_wrappedObject = archive;
            m_isDirty = false;
        }

        internal override void InitializeWrapper()
        {
            Nodes.Clear();
            foreach (ListArchiveFileEntry entry in WrappedObject.Entries)
            {
                Nodes.Add(ResourceFactory.GetResource(entry.Name, new MemoryStream(entry.Data)));
            }

            base.InitializeWrapper();
        }
    }
}
