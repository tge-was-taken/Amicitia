using System;
using AtlusLibSharp.Graphics.RenderWare;
using System.Windows.Forms;
using System.IO;
using AtlusLibSharp.PS2.Graphics;

namespace Amicitia.ResourceWrappers
{
    internal class RWTextureDictionaryWrapper : ResourceWrapper
    {
        protected internal static readonly SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.RWTextureDictionary
        };

        public RWTextureDictionaryWrapper(string text, RWTextureDictionary textureDictionary) : base(text, textureDictionary) { }

        public SupportedFileType FileType
        {
            get { return SupportedFileType.RWTextureDictionary; }
        }

        public RWDeviceID DeviceID
        {
            get { return WrappedObject.DeviceID; }
        }

        public int TextureCount
        {
            get { return Nodes.Count; }
        }

        protected internal new RWTextureDictionary WrappedObject
        {
            get { return (RWTextureDictionary)base.WrappedObject; }
            set { base.WrappedObject = value; }
        }

        protected internal override bool CanMove
        {
            get { return false; }
        }

        protected internal override bool CanRename
        {
            get { return false; }
        }

        protected internal override bool CanDelete
        {
            get { return false; }
        }

        protected internal override bool CanAdd
        {
            get { return true; }
        }

        public override void Replace(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDlg = new OpenFileDialog())
            {
                openFileDlg.FileName = Parent != null ? Path.GetFileNameWithoutExtension(Parent.Text) + ".txd" : Text;
                openFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(FileFilterTypes);

                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                int supportedFileIndex = SupportedFileHandler.GetSupportedFileIndex(openFileDlg.FileName);

                if (supportedFileIndex == -1)
                {
                    return;
                }

                switch (FileFilterTypes[openFileDlg.FilterIndex - 1])
                {
                    case SupportedFileType.RWTextureDictionary:
                        WrappedObject = (RWTextureDictionary)RWNode.Load(openFileDlg.FileName);
                        break;
                }

                // re-init the wrapper
                InitializeWrapper();
            }
        }

        public override void Export(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDlg = new SaveFileDialog())
            {
                saveFileDlg.FileName = Parent != null ? Path.GetFileNameWithoutExtension(Parent.Text) + ".txd" : Text;
                saveFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(FileFilterTypes);

                if (saveFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // rebuild wrapped object before export
                RebuildWrappedObject();

                switch (FileFilterTypes[saveFileDlg.FilterIndex - 1])
                {
                    case SupportedFileType.RWTextureDictionary:
                        WrappedObject.Save(saveFileDlg.FileName);
                        break;
                }
            }
        }

        public override void Add(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDlg = new OpenFileDialog())
            {
                openFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(RWTextureNativeWrapper.FileFilterTypes);

                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                int supportedFileIndex = SupportedFileHandler.GetSupportedFileIndex(openFileDlg.FileName);

                if (supportedFileIndex == -1)
                {
                    return;
                }

                try
                {
                    switch (RWTextureNativeWrapper.FileFilterTypes[openFileDlg.FilterIndex - 1])
                    {
                        case SupportedFileType.RWTextureNative:
                            WrappedObject.Textures.Add((RWTextureNative)RWNode.Load(openFileDlg.FileName));
                            break;

                        case SupportedFileType.PNGFile:
                            WrappedObject.Textures.Add(new RWTextureNative(openFileDlg.FileName, PS2PixelFormat.PSMT8));
                            break;
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Error occured.");
                }

                // re-init the wrapper
                InitializeWrapper();
            }
        }

        protected internal override void RebuildWrappedObject()
        {
            WrappedObject.Textures.Clear();
            for (int i = 0; i < Nodes.Count; i++)
            {
                // rebuild the data
                RWTextureNativeWrapper node = (RWTextureNativeWrapper)Nodes[i];
                node.RebuildWrappedObject();

                // set the wrapped object
                WrappedObject.Textures.Add(node.WrappedObject);
            }
        }

        protected internal override void InitializeWrapper()
        {
            Nodes.Clear();

            foreach (RWTextureNative texture in WrappedObject.Textures)
            {
                Nodes.Add(new RWTextureNativeWrapper(texture));
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
