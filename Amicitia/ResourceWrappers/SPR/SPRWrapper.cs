using Amicitia.Utilities;

namespace Amicitia.ResourceWrappers
{
    using System;
    using System.Windows.Forms;
    using AtlusLibSharp.SMT3.ChunkResources.Graphics;

    internal class SPRWrapper : ResourceWrapper
    {
        public SPRWrapper(string text, SPRChunk spr) : base(text, spr) { }

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

        internal SPRTexturesWrapper TexturesWrapper { get; private set; }

        // Event Handlers
        public override void Export(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDlg = new SaveFileDialog())
            {
                saveFileDlg.FileName = Text;
                saveFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(SupportedFileType.SPR);

                if (saveFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                int supportedFileIndex = SupportedFileHandler.GetSupportedFileIndex(saveFileDlg.FileName);

                if (supportedFileIndex == -1)
                {
                    return;
                }

                SPRChunk spr = GetWrappedObject<SPRChunk>(GetWrapperOptions.ForceRebuild);

                switch (SupportedFileHandler.GetType(supportedFileIndex))
                {
                    case SupportedFileType.SPR:
                        spr.Save(saveFileDlg.FileName);
                        break;
                    default:
                        break;
                }
            }
        }

        public override void Replace(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDlg = new OpenFileDialog())
            {
                openFileDlg.FileName = Text;
                openFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(SupportedFileType.SPR);

                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                int supportedFileIndex = SupportedFileHandler.GetSupportedFileIndex(openFileDlg.FileName);

                if (supportedFileIndex == -1)
                {
                    return;
                }

                switch (SupportedFileHandler.GetType(supportedFileIndex))
                {
                    case SupportedFileType.SPR:
                        ReplaceWrappedObjectAndInitialize(SPRChunk.LoadFrom(openFileDlg.FileName));
                        break;
                    default:
                        break;
                }
            }
        }

        // Protected Methods
        protected override void RebuildWrappedObject()
        {
            SPRKeyFrame[] keyFrames = KeyFramesWrapper.GetWrappedObject<SPRKeyFrame[]>(GetWrapperOptions.ForceRebuild);
            TMXChunk[] textures = TexturesWrapper.GetWrappedObject<TMXChunk[]>(GetWrapperOptions.ForceRebuild);
            ReplaceWrappedObjectAndInitialize(new SPRChunk(keyFrames, textures));
        }

        protected override void InitializeWrapper()
        {
            Nodes.Clear();

            SPRChunk spr = GetWrappedObject<SPRChunk>();
            KeyFramesWrapper = new SPRKeyFramesWrapper("KeyFrames", spr.KeyFrames);
            TexturesWrapper = new SPRTexturesWrapper("Textures", spr.Textures);

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
