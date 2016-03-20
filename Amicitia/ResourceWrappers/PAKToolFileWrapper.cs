namespace Amicitia.ResourceWrappers
{
    using System.IO;
    using System;
    using System.Windows.Forms;
    using AtlusLibSharp.FileSystems.ListArchive;
    using AtlusLibSharp.FileSystems.PAKToolArchive;
    using System.ComponentModel;
    using System.Collections.Generic;

    internal class PAKToolFileWrapper : ResourceWrapper
    {
        /*********************/
        /* File filter types */
        /*********************/
        public static readonly new SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.PAKToolArchiveFile, SupportedFileType.ListArchiveFile
        };

        /*****************************************/
        /* Import / Export delegate dictionaries */
        /*****************************************/
       public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ImportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.PAKToolArchiveFile, (res, path) =>
                res.WrappedObject = new PAKToolArchiveFile(path)
            },
            {
                SupportedFileType.ListArchiveFile, (res, path) =>
                res.WrappedObject = PAKToolArchiveFile.Create(new ListArchiveFile(path))
            }
        };

        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ExportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.PAKToolArchiveFile, (res, path) =>
                (res as PAKToolFileWrapper).WrappedObject.Save(path)
            },
            {
                SupportedFileType.ListArchiveFile, (res, path) =>
                ListArchiveFile.Create((res as PAKToolFileWrapper).WrappedObject).Save(path)
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
        public PAKToolFileWrapper(string text, PAKToolArchiveFile bin) 
            : base(text, bin, SupportedFileType.PAKToolArchiveFile, false)
        {
        }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new PAKToolArchiveFile WrappedObject
        {
            get
            {
                return (PAKToolArchiveFile)m_wrappedObject;
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
            var archive = new PAKToolArchiveFile();
            foreach (ResourceWrapper node in Nodes)
            {
                archive.Entries.Add(new PAKToolArchiveEntry(node.Text, node.GetMemoryStream()));
            }

            m_wrappedObject = archive;
            m_isDirty = false;
        }

        internal override void InitializeWrapper()
        {
            Nodes.Clear();
            foreach (PAKToolArchiveEntry entry in WrappedObject.Entries)
            {
                Nodes.Add(ResourceFactory.GetResource(entry.Name, new MemoryStream(entry.Data)));
            }

            base.InitializeWrapper();
        }
    }
}
