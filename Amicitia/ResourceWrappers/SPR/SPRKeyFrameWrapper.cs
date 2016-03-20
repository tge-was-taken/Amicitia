namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.Graphics.SPR;
    using System.ComponentModel;

    internal class SPRKeyFrameWrapper : ResourceWrapper
    {
        /***************/
        /* Constructor */
        /***************/
        public SPRKeyFrameWrapper(string text, SPRKeyFrame keyFrame) 
            : base(text, keyFrame, SupportedFileType.Resource, true)
        {
            m_canExport = false;
            m_canRename = false;
            m_canReplace = false;
            InitializeContextMenuStrip();
        }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new SPRKeyFrame WrappedObject
        {
            get
            {
                return (SPRKeyFrame)m_wrappedObject;
            }
            set
            {
                SetProperty(m_wrappedObject, value);
            }
        }

        public string Comment
        {
            get { return WrappedObject.Comment; }
            set { SetProperty(WrappedObject, value); }
        }
    }
}
