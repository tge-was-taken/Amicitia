using Amicitia.Utilities;

namespace Amicitia.ResourceWrappers
{
    using System;
    using System.Windows.Forms;
    using AtlusLibSharp.SMT3.Graphics;

    internal class SPRFileWrapper : ResourceWrapper
    {
        protected internal static readonly SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.SPRFile
        };

        public SPRFileWrapper(string text, SPRFile spr) : base(text, spr) { }

        protected internal new SPRFile WrappedObject
        {
            get { return (SPRFile)base.WrappedObject; }
            set { base.WrappedObject = value; }
        }

        public int KeyFrameCount
        {
            get { return KeyFramesWrapper.Nodes.Count; }
        }

        public int TextureCount
        {
            get { return TexturesWrapper.Nodes.Count; }
        }

        internal SPRKeyFrameListWrapper KeyFramesWrapper { get; private set; }

        internal SPRFileTexturesWrapper TexturesWrapper { get; private set; }

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

                // rebuild wrapped object before export
                RebuildWrappedObject();

                switch (FileFilterTypes[saveFileDlg.FilterIndex-1])
                {
                    case SupportedFileType.SPRFile:
                        WrappedObject.Save(saveFileDlg.FileName);
                        break;
                }
            }
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
                    case SupportedFileType.SPRFile:
                        WrappedObject = SPRFile.LoadFrom(openFileDlg.FileName);
                        break;
                }

                // re-init
                InitializeWrapper();
            }
        }

        protected internal override void RebuildWrappedObject()
        {
            // rebuild the data
            KeyFramesWrapper.RebuildWrappedObject();
            TexturesWrapper.RebuildWrappedObject();

            // set the wrapped object
            WrappedObject = new SPRFile(KeyFramesWrapper.WrappedObject, TexturesWrapper.WrappedObject);
        }

        protected internal override void InitializeWrapper()
        {
            Nodes.Clear();

            KeyFramesWrapper = new SPRKeyFrameListWrapper("KeyFrames", WrappedObject.KeyFrames);
            TexturesWrapper = new SPRFileTexturesWrapper("Textures", WrappedObject.Textures);

            Nodes.Add(KeyFramesWrapper, TexturesWrapper);

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
