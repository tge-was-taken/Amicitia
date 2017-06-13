using System.Windows.Forms;

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
            m_canAdd = true;

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
        public override void Add(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                Nodes.Add(new AMDChunkWrapper(Path.GetFileName(openFileDialog.FileName),
                    new AMDChunk(string.Empty, 0, File.ReadAllBytes(openFileDialog.FileName))));

                RebuildWrappedObject();
            }
        }

        internal override void RebuildWrappedObject()
        {
            var archive = new AMDFile();
            foreach (AMDChunkWrapper node in Nodes)
            {
                if (node.IsDirty)
                    node.RebuildWrappedObject();

                archive.Chunks.Add(node.WrappedObject);
            }

            WrappedObject = archive;
            m_isDirty = false;
        }

        internal override void InitializeWrapper()
        {
            Nodes.Clear();

            int idx = 0;
            foreach (AMDChunk chunk in WrappedObject.Chunks)
            {
                var wrap = new AMDChunkWrapper(chunk.Tag, chunk);
                Nodes.Add(wrap);
            }

            base.InitializeWrapper();
        }
    }
}