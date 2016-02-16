namespace Amicitia.ResourceWrappers
{
    using System.IO;
    using System;
    using System.Windows.Forms;
    using AtlusLibSharp.Persona3.FileSystem.Archives;

    internal class BVPArchiveFileWrapper : ResourceWrapper
    {
        protected internal static readonly SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.BVPArchiveFile
        };

        public BVPArchiveFileWrapper(string text, BVPArchiveFile arc) : base(text, arc) { }

        public SupportedFileType FileType
        {
            get { return SupportedFileType.BVPArchiveFile; }
        }

        public int EntryCount
        {
            get { return Nodes.Count; }
        }

        protected internal new BVPArchiveFile WrappedObject
        {
            get { return (BVPArchiveFile)base.WrappedObject; }
            set { base.WrappedObject = value; }
        }

        public override void Replace(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDlg = new OpenFileDialog())
            {
                openFileDlg.FileName = Text;
                openFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(FileFilterTypes);

                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                switch (FileFilterTypes[openFileDlg.FilterIndex-1])
                {
                    case SupportedFileType.BVPArchiveFile:
                        WrappedObject = new BVPArchiveFile(openFileDlg.FileName);
                        break;
                }

                // Reinitialize
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

                // Rebuild the wrapper before export
                RebuildWrappedObject();

                switch (FileFilterTypes[saveFileDlg.FilterIndex-1])
                {
                    case SupportedFileType.BVPArchiveFile:
                        WrappedObject.Save(saveFileDlg.FileName);
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
                WrappedObject.Entries.Add(new BVPArchiveEntry(node.GetBytes(), node.GetPropertyValue<int>("Flag")));
            }
        }

        protected internal override void InitializeWrapper()
        {
            Nodes.Clear();

            int idx = -1;
            foreach (BVPArchiveEntry entry in WrappedObject.Entries)
            {
                ++idx;
                ResourceWrapper res = ResourceFactory.GetResource("Entry" + idx + ".bmd", new MemoryStream(entry.Data));

                // TODO: this thing doesn't even show up on the property grid
                res.ResourceProperties = new ResourceProperty[] { new ResourceProperty("Flag", entry.Flag) };
                Nodes.Add(res); ;
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
