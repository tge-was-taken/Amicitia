namespace Amicitia.ResourceWrappers
{
    using System;
    using Utilities;
    using AtlusLibSharp.Graphics.SPR;
    using System.ComponentModel;
    using System.Collections.Generic;

    internal class SPRFileWrapper : ResourceWrapper
    {
        /*********************/
        /* File filter types */
        /*********************/
        public static readonly new SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.SPRFile
        };

        /*****************************************/
        /* Import / Export delegate dictionaries */
        /*****************************************/
        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ImportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.SPRFile, (res, path) =>
                res.WrappedObject = SPRFile.Load(path)
            }
        };

        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ExportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.SPRFile, (res, path) =>
                (res as SPRFileWrapper).WrappedObject.Save(path)
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
        public SPRFileWrapper(string text, SPRFile spr) 
            : base(text, spr, SupportedFileType.SPRFile, false)
        {
        }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new SPRFile WrappedObject
        {
            get
            {
                return (SPRFile)m_wrappedObject;
            }
            set
            {
                SetProperty(ref m_wrappedObject, true);
            }
        }

        [Browsable(false)]
        public SPRKeyFrameListWrapper KeyFramesWrapper { get; private set; }

        [Browsable(false)]
        public SPRFileTexturesWrapper TexturesWrapper { get; private set; }

        public int KeyFrameCount
        {
            get { return KeyFramesWrapper.Nodes.Count; }
        }

        public int TextureCount
        {
            get { return TexturesWrapper.Nodes.Count; }
        }

        /*********************************/
        /* Base wrapper method overrides */
        /*********************************/
        internal override void RebuildWrappedObject()
        {
            if (TexturesWrapper.IsDirty)
                TexturesWrapper.RebuildWrappedObject();

            if (KeyFramesWrapper.IsDirty)
                KeyFramesWrapper.RebuildWrappedObject();

            m_wrappedObject = new SPRFile(TexturesWrapper.WrappedObject, KeyFramesWrapper.WrappedObject);
            m_isDirty = false;
        }

        internal override void InitializeWrapper()
        {
            Nodes.Clear();

            KeyFramesWrapper = new SPRKeyFrameListWrapper("Keyframes", WrappedObject.KeyFrames);
            TexturesWrapper = new SPRFileTexturesWrapper("Textures", WrappedObject.Textures);

            Nodes.Add(KeyFramesWrapper, TexturesWrapper);

            base.InitializeWrapper();
        }
    }
}
