namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.Persona3.Archives;
    using System.IO;
    using System;
    using System.Windows.Forms;
    using AtlusLibSharp.Generic.Archives;

    internal class ARCArchiveWrapper : ResourceWrapper
    {
        // Fields
        private static readonly SupportedFileType[] _fileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.ARCArchive, SupportedFileType.GenericPAK
        };

        // Constructor
        public ARCArchiveWrapper(string text, GenericPSVitaArchive arc) : base(text, arc) { }

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

                int supportedFileIndex = SupportedFileHandler.GetSupportedFileIndex(openFileDlg.FileName);

                if (supportedFileIndex == -1)
                {
                    return;
                }

                switch (_fileFilterTypes[openFileDlg.FilterIndex-1])
                {
                    case SupportedFileType.ARCArchive:
                        ReplaceWrappedObjectAndInitialize(new GenericPSVitaArchive(openFileDlg.FileName));
                        break;
                    case SupportedFileType.GenericPAK:
                        ReplaceWrappedObjectAndInitialize(GenericPSVitaArchive.Create(new GenericPAK(openFileDlg.FileName)));
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

                GenericPSVitaArchive arc = GetWrappedObject<GenericPSVitaArchive>(GetWrapperOptions.ForceRebuild);

                switch (_fileFilterTypes[saveFileDlg.FilterIndex-1])
                {
                    case SupportedFileType.ARCArchive:
                        arc.Save(saveFileDlg.FileName);
                        break;
                    case SupportedFileType.GenericPAK:
                        GenericPAK.Create(arc).Save(saveFileDlg.FileName);
                        break;
                }
            }
        }

        // Protected methods
        protected override void RebuildWrappedObject()
        {
            GenericPSVitaArchive arc = new GenericPSVitaArchive();
            foreach (ResourceWrapper node in Nodes)
            {
                arc.Entries.Add(new GenericVitaArchiveEntry(node.Text, node.GetBytes()));
            }

            ReplaceWrappedObjectAndInitialize(arc);
        }

        protected override void InitializeWrapper()
        {
            Nodes.Clear();
            GenericPSVitaArchive bin = GetWrappedObject<GenericPSVitaArchive>();
            foreach (GenericVitaArchiveEntry entry in bin.Entries)
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
