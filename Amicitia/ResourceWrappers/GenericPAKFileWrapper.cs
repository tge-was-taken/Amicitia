namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.Generic.Archives;
    using System.IO;
    using System;
    using System.Windows.Forms;
    using AtlusLibSharp.Persona3.Archives;

    internal class GenericPAKFileWrapper : ResourceWrapper
    {
        // Fields
        private static readonly SupportedFileType[] _fileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.GenericPAK, SupportedFileType.ARCArchive
        };

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

                switch (_fileFilterTypes[openFileDlg.FilterIndex-1])
                {
                    case SupportedFileType.GenericPAK:
                        ReplaceWrappedObjectAndInitialize(new GenericPAK(openFileDlg.FileName));
                        break;
                    case SupportedFileType.ARCArchive:
                        ReplaceWrappedObjectAndInitialize(GenericPAK.Create(new GenericVitaArchive(openFileDlg.FileName)));
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

                GenericPAK binArchive = GetWrappedObject<GenericPAK>(GetWrapperOptions.ForceRebuild);

                switch (_fileFilterTypes[saveFileDlg.FilterIndex-1])
                {
                    case SupportedFileType.GenericPAK:
                        binArchive.Save(saveFileDlg.FileName);
                        break;
                    case SupportedFileType.ARCArchive:
                        GenericVitaArchive.Create(binArchive).Save(saveFileDlg.FileName);
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
