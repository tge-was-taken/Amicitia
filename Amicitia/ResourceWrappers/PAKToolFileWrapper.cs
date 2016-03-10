namespace Amicitia.ResourceWrappers
{
    using System.IO;
    using System;
    using System.Windows.Forms;
    using AtlusLibSharp.FileSystems.ListArchive;
    using AtlusLibSharp.FileSystems.PAKToolArchive;

    internal class PAKToolFileWrapper : ResourceWrapper
    {
        protected internal static readonly SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.PAKToolFile, SupportedFileType.ListArchiveFile
        };

        public PAKToolFileWrapper(string text, PAKToolArchiveFile bin) : base(text, bin) { }

        public SupportedFileType FileType
        {
            get { return SupportedFileType.PAKToolFile; }
        }

        public int EntryCount
        {
            get { return Nodes.Count; }
        }

        protected internal new PAKToolArchiveFile WrappedObject
        {
            get { return (PAKToolArchiveFile)base.WrappedObject; }
            set { base.WrappedObject = value; }
        }

        public override void Replace(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDlg = new OpenFileDialog())
            {
                openFileDlg.FileName = Text;
                openFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(SupportedFileType.PAKToolFile, SupportedFileType.ListArchiveFile);

                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                switch (FileFilterTypes[openFileDlg.FilterIndex-1])
                {
                    case SupportedFileType.PAKToolFile:
                        WrappedObject = new PAKToolArchiveFile(openFileDlg.FileName);
                        break;
                    case SupportedFileType.ListArchiveFile:
                        WrappedObject = PAKToolArchiveFile.Create(new ListArchiveFile(openFileDlg.FileName));
                        break;
                }

                // re-init the wrapper
                InitializeWrapper();
            }
        }

        public override void Export(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDlg = new SaveFileDialog())
            {
                saveFileDlg.FileName = Text;
                saveFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(FileFilterTypes);

                if (saveFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // rebuild the wrapped object
                RebuildWrappedObject(); 

                switch (FileFilterTypes[saveFileDlg.FilterIndex-1])
                {
                    case SupportedFileType.PAKToolFile:
                        WrappedObject.Save(saveFileDlg.FileName);
                        break;
                    case SupportedFileType.ListArchiveFile:
                        ListArchiveFile.Create(WrappedObject).Save(saveFileDlg.FileName);
                        break;
                }
            }
        }

        protected internal override void RebuildWrappedObject()
        {
            WrappedObject.Entries.Clear();
            foreach (ResourceWrapper node in Nodes)
            {
                node.RebuildWrappedObject();
                WrappedObject.Entries.Add(new PAKToolArchiveEntry(node.Text, node.GetBytes()));
            }
        }

        protected internal override void InitializeWrapper()
        {
            Nodes.Clear();
            foreach (PAKToolArchiveEntry entry in WrappedObject.Entries)
            {
                Nodes.Add(ResourceFactory.GetResource(entry.Name, new MemoryStream(entry.Data)));
            }

            if (IsInitialized)
            {
                MainForm.Instance.UpdateReferences();
            }
            else
            {
                IsInitialized = true;
            }
        }
    }
}
