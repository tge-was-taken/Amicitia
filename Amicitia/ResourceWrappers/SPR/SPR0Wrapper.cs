using Amicitia.Utilities;

namespace Amicitia.ResourceWrappers
{
    using System;
    using System.Windows.Forms;
    using AtlusLibSharp.SMT3.ChunkResources.Graphics;

    internal class SPRWrapper : ResourceWrapper
    {
        // Fields
        private static readonly SupportedFileType[] _fileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.SPR0
        };

        // Constructor
        public SPRWrapper(string text, SPRFile spr) : base(text, spr) { }

        // Properties
        public int KeyFrameCount
        {
            get { return KeyFramesWrapper.Nodes.Count; }
        }

        public int TextureCount
        {
            get { return TexturesWrapper.Nodes.Count; }
        }

        internal SPRKeyFramesWrapper KeyFramesWrapper { get; private set; }

        internal SPR0TexturesWrapper TexturesWrapper { get; private set; }

        // Event Handlers
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

                SPRFile spr = GetWrappedObject<SPRFile>(GetWrapperOptions.ForceRebuild);

                switch (_fileFilterTypes[saveFileDlg.FilterIndex-1])
                {
                    case SupportedFileType.SPR0:
                        spr.Save(saveFileDlg.FileName);
                        break;
                }
            }
        }

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
                    case SupportedFileType.SPR0:
                        ReplaceWrappedObjectAndInitialize(SPRFile.LoadFrom(openFileDlg.FileName));
                        break;
                }
            }
        }

        // Protected Methods
        protected override void RebuildWrappedObject()
        {
            SPRKeyFrame[] keyFrames = KeyFramesWrapper.GetWrappedObject<SPRKeyFrame[]>(GetWrapperOptions.ForceRebuild);
            TMXFile[] textures = TexturesWrapper.GetWrappedObject<TMXFile[]>(GetWrapperOptions.ForceRebuild);
            ReplaceWrappedObjectAndInitialize(new SPRFile(keyFrames, textures));
        }

        protected override void InitializeWrapper()
        {
            Nodes.Clear();

            SPRFile spr = GetWrappedObject<SPRFile>();
            KeyFramesWrapper = new SPRKeyFramesWrapper("KeyFrames", spr.KeyFrames);
            TexturesWrapper = new SPR0TexturesWrapper("Textures", spr.Textures);

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
