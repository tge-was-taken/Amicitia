namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.Generic.Archives;
    using System.IO;
    using System;
    using System.Windows.Forms;
    using AtlusLibSharp.Persona3.Archives;

    internal class GenericPAKFileWrapper : ResourceWrapper
    {
        // Constructor
        public GenericPAKFileWrapper(string text, GenericPAK bin) : base(text, bin) { }

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
                openFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(SupportedFileType.GenericPAK, SupportedFileType.ARCArchive);

                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                switch (openFileDlg.FilterIndex)
                {
                    case 1:
                        ReplaceWrappedObjectAndInitialize(new GenericPAK(openFileDlg.FileName));
                        break;
                    case 2:
                        ReplaceWrappedObjectAndInitialize(GenericPAK.From(new GenericVitaArchive(openFileDlg.FileName)));
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
                saveFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(SupportedFileType.GenericPAK, SupportedFileType.ARCArchive);

                if (saveFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                GenericPAK binArchive = GetWrappedObject<GenericPAK>(GetWrapperOptions.ForceRebuild);

                switch (saveFileDlg.FilterIndex)
                {
                    case 1:
                        binArchive.Save(saveFileDlg.FileName);
                        break;
                    case 2:
                        GenericVitaArchive.From(binArchive).Save(saveFileDlg.FileName);
                        break;
                    default:
                        break;
                }
            }
        }

        // Protected methods
        protected override void RebuildWrappedObject()
        {
            GenericPAK binArchive = new GenericPAK();
            foreach (ResourceWrapper node in Nodes)
            {
                binArchive.Entries.Add(new GenericPAKEntry(node.Text, node.GetBytes()));
            }

            ReplaceWrappedObjectAndInitialize(binArchive);
        }

        protected override void InitializeWrapper()
        {
            Nodes.Clear();
            GenericPAK bin = GetWrappedObject<GenericPAK>();
            foreach (GenericPAKEntry entry in bin.Entries)
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
