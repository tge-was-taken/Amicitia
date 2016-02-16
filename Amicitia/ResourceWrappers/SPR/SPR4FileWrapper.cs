using Amicitia.Utilities;

namespace Amicitia.ResourceWrappers
{
    using System;
    using System.Windows.Forms;
    using AtlusLibSharp.SMT3.Graphics;
    using AtlusLibSharp.Common;

    internal class SPR4FileWrapper : ResourceWrapper
    {
        protected internal static readonly SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.SPR4File
        };

        protected internal new SPR4File WrappedObject
        {
            get { return (SPR4File)base.WrappedObject; }
            set { base.WrappedObject = value; }
        }

        public SPR4FileWrapper(string text, SPR4File spr) : base(text, spr) { }

        public int KeyFrameCount
        {
            get { return KeyFramesWrapper.Nodes.Count; }
        }

        public int TextureCount
        {
            get { return TexturesWrapper.Nodes.Count; }
        }

        internal SPRKeyFrameListWrapper KeyFramesWrapper { get; private set; }

        internal SPR4TexturesWrapper TexturesWrapper { get; private set; }

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

                // Rebuild before export
                RebuildWrappedObject();

                switch (FileFilterTypes[saveFileDlg.FilterIndex-1])
                {
                    case SupportedFileType.SPR4File:
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
                    case SupportedFileType.SPR4File:
                        WrappedObject = SPR4File.LoadFrom(openFileDlg.FileName);
                        break;
                }

                // Re-init
                InitializeWrapper();
            }
        }

        protected internal override void RebuildWrappedObject()
        {
            // rebuild before getting the data
            KeyFramesWrapper.RebuildWrappedObject();
            TexturesWrapper.RebuildWrappedObject();

            // create a new spr using the rebuilt data
            WrappedObject = new SPR4File(KeyFramesWrapper.WrappedObject, Array.ConvertAll(TexturesWrapper.WrappedObject, o => o.GetBytes()));
        }

        protected internal override void InitializeWrapper()
        {
            Nodes.Clear();

            KeyFramesWrapper = new SPRKeyFrameListWrapper("KeyFrames", WrappedObject.KeyFrames);
            TexturesWrapper = new SPR4TexturesWrapper("Textures", Array.ConvertAll(WrappedObject.TGATextures, o => new GenericBinaryFile(o)));

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
