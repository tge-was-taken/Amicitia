using Amicitia.Utilities;

namespace Amicitia.ResourceWrappers
{
    using System;
    using System.Windows.Forms;
    using AtlusLibSharp.SMT3.ChunkResources.Graphics;
    using AtlusLibSharp;

    internal class SPR4Wrapper : ResourceWrapper
    {
        // Fields
        private static readonly SupportedFileType[] _fileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.SPR4
        };

        // Constructor
        public SPR4Wrapper(string text, SPR4File spr) : base(text, spr) { }

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

        internal SPR4TexturesWrapper TexturesWrapper { get; private set; }

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

                SPR4File spr = GetWrappedObject<SPR4File>(GetWrapperOptions.ForceRebuild);

                switch (_fileFilterTypes[saveFileDlg.FilterIndex-1])
                {
                    case SupportedFileType.SPR4:
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
                    case SupportedFileType.SPR4:
                        ReplaceWrappedObjectAndInitialize(SPR4File.LoadFrom(openFileDlg.FileName));
                        break;
                }
            }
        }

        // Protected Methods
        protected override void RebuildWrappedObject()
        {
            SPRKeyFrame[] keyFrames = KeyFramesWrapper.GetWrappedObject<SPRKeyFrame[]>(GetWrapperOptions.ForceRebuild);
            GenericBinaryFile[] textures = TexturesWrapper.GetWrappedObject<GenericBinaryFile[]>(GetWrapperOptions.ForceRebuild);
            ReplaceWrappedObjectAndInitialize(new SPR4File(keyFrames, Array.ConvertAll(textures, o => o.GetBytes())));
        }

        protected override void InitializeWrapper()
        {
            Nodes.Clear();

            SPR4File spr = GetWrappedObject<SPR4File>();
            KeyFramesWrapper = new SPRKeyFramesWrapper("KeyFrames", spr.KeyFrames);
            TexturesWrapper = new SPR4TexturesWrapper("Textures", Array.ConvertAll(spr.TGATextures, o => new GenericBinaryFile(o)));

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
