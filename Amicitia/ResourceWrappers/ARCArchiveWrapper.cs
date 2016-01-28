namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.Persona3.Archives;
    using System.IO;
    using System;
    using System.Windows.Forms;
    using AtlusLibSharp.Generic.Archives;

    internal class ARCArchiveWrapper : ResourceWrapper
    {
        // Constructor
        public ARCArchiveWrapper(string text, GenericVitaArchive arc) : base(text, arc) { }

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
                openFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(SupportedFileType.ARCArchive, SupportedFileType.GenericPAK);

                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                int supportedFileIndex = SupportedFileHandler.GetSupportedFileIndex(openFileDlg.FileName);

                if (supportedFileIndex == -1)
                {
                    return;
                }

                switch (openFileDlg.FilterIndex)
                {
                    case 1:
                        ReplaceWrappedObjectAndInitialize(GenericVitaArchive.From(new GenericPAK(openFileDlg.FileName)));
                        break;
                    case 2:
                        ReplaceWrappedObjectAndInitialize(new GenericVitaArchive(openFileDlg.FileName));
                        break;
                    default:
                        break;
                }
            }
        }

        public override void Export(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDlg = new SaveFileDialog())
            {
                saveFileDlg.FileName = Text;
                saveFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(SupportedFileType.ARCArchive, SupportedFileType.GenericPAK);

                if (saveFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                GenericVitaArchive arc = GetWrappedObject<GenericVitaArchive>(GetWrapperOptions.ForceRebuild);

                switch (saveFileDlg.FilterIndex)
                {
                    case 1:
                        GenericPAK.From(arc).Save(saveFileDlg.FileName);
                        break;
                    case 2:
                        arc.Save(saveFileDlg.FileName);
                        break;
                    default:
                        break;
                }
            }
        }

        // Protected methods
        protected override void RebuildWrappedObject()
        {
            GenericVitaArchive arc = new GenericVitaArchive();
            foreach (ResourceWrapper node in Nodes)
            {
                arc.Entries.Add(new GenericVitaArchiveEntry(node.Text, node.GetBytes()));
            }

            ReplaceWrappedObjectAndInitialize(arc);
        }

        protected override void InitializeWrapper()
        {
            Nodes.Clear();
            GenericVitaArchive bin = GetWrappedObject<GenericVitaArchive>();
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
