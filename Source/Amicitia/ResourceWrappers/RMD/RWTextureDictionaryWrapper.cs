using System;
using AtlusLibSharp.Graphics.RenderWare;
using System.Windows.Forms;
using System.IO;
using AtlusLibSharp.PS2.Graphics;
using System.ComponentModel;
using System.Collections.Generic;

namespace Amicitia.ResourceWrappers
{
    internal class RWTextureDictionaryWrapper : ResourceWrapper
    {
        /*********************/
        /* File filter types */
        /*********************/
        public static readonly new SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.RWTextureDictionary
        };

        /*****************************************/
        /* Import / Export delegate dictionaries */
        /*****************************************/
        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ImportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.RWTextureDictionary, (res, path) =>
                res.WrappedObject = RWNode.Load(path)
            }
        };

        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ExportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.RWTextureDictionary, (res, path) =>
                (res as RWTextureDictionaryWrapper).WrappedObject.Save(path)
            }
        };

        /************************************/
        /* Import / export method overrides */
        /************************************/
        protected override Dictionary<SupportedFileType, Action<ResourceWrapper, string>> GetImportDelegates()
        {
            return ImportDelegates;
        }

        protected override Dictionary<SupportedFileType, Action<ResourceWrapper, string>> GetExportDelegates()
        {
            return ExportDelegates;
        }

        protected override SupportedFileType[] GetSupportedFileTypes()
        {
            return FileFilterTypes;
        }

        /***************/
        /* Constructor */
        /***************/
        public RWTextureDictionaryWrapper(string text, RWTextureDictionary textureDictionary) 
            : base(text, textureDictionary, SupportedFileType.RWTextureDictionary, true)
        {
            m_canMove = false;
            m_canRename = false;
            m_canDelete = false;
            m_canAdd = true;
            InitializeContextMenuStrip();
        }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new RWTextureDictionary WrappedObject
        {
            get
            {
                return (RWTextureDictionary)m_wrappedObject;
            }
            set
            {
                SetProperty(ref m_wrappedObject, value);
            }
        }

        public RWDeviceID DeviceID
        {
            get { return WrappedObject.DeviceID; }
        }

        public int TextureCount
        {
            get { return Nodes.Count; }
        }

        /*********************************/
        /* Base wrapper method overrides */
        /*********************************/
        public override void Add(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDlg = new OpenFileDialog())
            {
                openFileDlg.Filter = SupportedFileManager.GetFilteredFileFilter(RWTextureNativeWrapper.FileFilterTypes);

                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                int supportedFileIndex = SupportedFileManager.GetSupportedFileIndex(openFileDlg.FileName);

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

                IsDirty = true;

                // re-init the wrapper
                InitializeWrapper();
            }
        }

        internal override void RebuildWrappedObject()
        {
            var txd = new RWTextureDictionary();
            foreach (RWTextureNativeWrapper node in Nodes)
            {
                if (node.IsDirty)
                    node.RebuildWrappedObject();
                txd.Textures.Add(node.WrappedObject);
            }

            m_wrappedObject = txd;
            m_isDirty = false;
        }

        internal override void InitializeWrapper()
        {
            Nodes.Clear();

            foreach (RWTextureNative texture in WrappedObject.Textures)
            {
                Nodes.Add(new RWTextureNativeWrapper(texture));
            }

            base.InitializeWrapper();
        }
    }
}
