namespace Amicitia.ResourceWrappers
{
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    using Utilities;
    using AtlusLibSharp.Graphics.SPR;

    internal class SPR4FileWrapper : ResourceWrapper
    {
        /*********************/
        /* File filter types */
        /*********************/
        public static readonly new SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.SPR4File
        };

        /*****************************************/
        /* Import / Export delegate dictionaries */
        /*****************************************/
       public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ImportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.SPR4File, (res, path) =>
                res.WrappedObject = SPR4File.Load(path)
            }
        };

        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ExportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.SPR4File, (res, path) =>
                (res as SPR4FileWrapper).WrappedObject.Save(path)
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
        public SPR4FileWrapper(string text, SPR4File spr) : base(text, spr) { }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new SPR4File WrappedObject
        {
            get
            {
                return (SPR4File)m_wrappedObject;
            }

            set
            {
                SetProperty(ref m_wrappedObject, value);
            }
        }

        [Browsable(false)]
        public SPRKeyFrameListWrapper KeyFramesWrapper { get; private set; }

        [Browsable(false)]
        public SPR4FileTexturesWrapper TexturesWrapper { get; private set; }

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

            m_wrappedObject = new SPR4File(TexturesWrapper.WrappedObject, KeyFramesWrapper.WrappedObject);
            m_isDirty = false;
        }

        internal override void InitializeWrapper()
        {
            Nodes.Clear();

            KeyFramesWrapper = new SPRKeyFrameListWrapper("Keyframes", WrappedObject.KeyFrames);
            TexturesWrapper = new SPR4FileTexturesWrapper("Textures", WrappedObject.Textures);

            Nodes.Add(KeyFramesWrapper, TexturesWrapper);

            base.InitializeWrapper();
        }
    }
}
