namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.Persona3.Archives;
    using System.IO;
    using System;
    using System.Windows.Forms;

    internal class BVPArchiveWrapper : ResourceWrapper
    {
        // Fields
        private static readonly SupportedFileType[] _fileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.BVPArchive
        };

        // Constructor
        public BVPArchiveWrapper(string text, BVPArchive arc) : base(text, arc) { }

        // Properties
        public int EntryCount
        {
            get { return Nodes.Count; }
        }

        // Event handlers
        public override void Replace(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDlg = new OpenFileDialog())
            {
                openFileDlg.FileName = Text;
                openFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(_fileFilterTypes);

                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                switch (_fileFilterTypes[openFileDlg.FilterIndex-1])
                {
                    case SupportedFileType.BVPArchive:
                        ReplaceWrappedObjectAndInitialize(new BVPArchive(openFileDlg.FileName));
                        break;
                }
            }
        }

        public override void Export(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDlg = new SaveFileDialog())
            {
                saveFileDlg.FileName = Text;
                saveFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(_fileFilterTypes);

                if (saveFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                BVPArchive arc = GetWrappedObject<BVPArchive>(GetWrapperOptions.ForceRebuild);

                switch (_fileFilterTypes[saveFileDlg.FilterIndex-1])
                {
                    case SupportedFileType.BVPArchive:
                        arc.Save(saveFileDlg.FileName);
                        break;
                }
            }
        }

        // Protected methods
        protected override void RebuildWrappedObject()
        {
            BVPArchive arc = new BVPArchive();
            foreach (ResourceWrapper node in Nodes)
            {
                arc.Entries.Add(new BVPArchiveEntry(node.GetBytes(), node.GetPropertyValue<int>("Flag")));
            }

            ReplaceWrappedObjectAndInitialize(arc);
        }

        protected override void InitializeWrapper()
        {
            Nodes.Clear();
            BVPArchive arc = GetWrappedObject<BVPArchive>();

            int idx = -1;
            foreach (BVPArchiveEntry entry in arc.Entries)
            {
                ++idx;
                ResourceWrapper res = ResourceFactory.GetResource("Entry" + idx + ".bmd", new MemoryStream(entry.Data));
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
