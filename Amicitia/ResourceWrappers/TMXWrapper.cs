namespace Amicitia.ResourceWrappers
{
    using System;
    using System.Windows.Forms;
    using AtlusLibSharp.SMT3.ChunkResources.Graphics;
    using System.ComponentModel;
    using AtlusLibSharp.PS2.Graphics;

    internal class TMXWrapper : ResourceWrapper
    {
        public TMXWrapper(string text, TMXChunk tmx) : base(text, tmx) { }

        // Properties
        [Category("Texture info")]
        public ushort Width { get; private set; }

        [Category("Texture info")]
        public ushort Height { get; private set; }

        [Category("Texture info")]
        public PixelFormat PixelFormat { get; private set; }

        [Category("Texture info")]
        public bool UsesPalette { get; private set; }

        [Category("Texture info")]
        public byte PaletteCount { get; private set; }

        [Category("Texture info")]
        public PixelFormat PaletteFormat { get; private set; }

        [Category("Texture info")]
        public byte MipMapCount { get; private set; }

        [Category("Texture settings")]
        public TMXWrapMode HorizontalWrappingMode { get; set; }

        [Category("Texture settings")]
        public TMXWrapMode VerticalWrappingMode { get; set; }

        [Category("Texture settings")]
        public int TextureID { get; set; }

        [Category("Texture settings")]
        public int ClutID { get; set; }

        [Category("Texture settings")]
        public string Comment { get; set; }

        // Context menu properties
        protected internal override bool IsImageResource
        {
            get { return true; }
        }

        // Public Methods
        public override void Export(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDlg = new SaveFileDialog())
            {
                saveFileDlg.FileName = Text;
                saveFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(SupportedFileType.TMX, SupportedFileType.PNG);

                if (saveFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                int supportedFileIndex = SupportedFileHandler.GetSupportedFileIndex(saveFileDlg.FileName);

                if (supportedFileIndex == -1)
                {
                    return;
                }

                TMXChunk tmx = GetWrappedObject<TMXChunk>(GetWrapperOptions.ForceRebuild);

                switch (SupportedFileHandler.GetType(supportedFileIndex))
                {
                    case SupportedFileType.TMX:
                        tmx.Save(saveFileDlg.FileName);
                        break;
                    case SupportedFileType.PNG:
                        tmx.GetBitmap().Save(saveFileDlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
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
                openFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(SupportedFileType.TMX, SupportedFileType.PNG);

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
                    case SupportedFileType.TMX:
                        ReplaceWrappedObjectAndInitialize(TMXChunk.LoadFrom(openFileDlg.FileName));
                        break;
                    case SupportedFileType.PNG:
                        ReplaceWrappedObjectAndInitialize(new TMXChunk(
                            (System.Drawing.Bitmap)System.Drawing.Image.FromFile(openFileDlg.FileName),
                            PixelFormat,
                            Comment));
                        break;
                    default:
                        break;
                }
            }
        }

        // Protected Methods
        protected override void RebuildWrappedObject()
        {
            TMXChunk tmx = GetWrappedObject<TMXChunk>();
            tmx.HorizontalWrappingMode = HorizontalWrappingMode;
            tmx.VerticalWrappingMode = VerticalWrappingMode;
            tmx.UserTextureID = TextureID;
            tmx.UserClutID = ClutID;
            tmx.UserComment = Comment;
            InitializeWrapper();
        }

        protected override void InitializeWrapper()
        {
            TMXChunk tmx = GetWrappedObject<TMXChunk>();
            PaletteCount = tmx.PaletteCount;
            PaletteFormat = tmx.PaletteFormat;
            Width = tmx.Width;
            Height = tmx.Height;
            PixelFormat = tmx.PixelFormat;
            MipMapCount = tmx.MipMapCount;
            HorizontalWrappingMode = tmx.HorizontalWrappingMode;
            VerticalWrappingMode = tmx.VerticalWrappingMode;
            TextureID = tmx.UserTextureID;
            ClutID = tmx.UserClutID;
            Comment = tmx.UserComment;
            UsesPalette = tmx.UsesPalette;

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
