namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.Graphics.SPR;
    using System.Collections.Generic;
    using System.ComponentModel;

    internal class SPRKeyFrameListWrapper : ResourceWrapper
    {
        /***************/
        /* Constructor */
        /***************/
        public SPRKeyFrameListWrapper(string text, List<SPRKeyFrame> keyFrames) 
            : base(text, keyFrames, SupportedFileType.Resource, true)
        {
            m_canExport = false;
            m_canMove = false;
            m_canRename = false;
            m_canReplace = false;
            m_canDelete = false;
            InitializeContextMenuStrip();
        }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new List<SPRKeyFrame> WrappedObject
        {
            get
            {
                return (List<SPRKeyFrame>)m_wrappedObject;
            }
            set
            {
                SetProperty(ref m_wrappedObject, value);
            }
        }

        /*********************************/
        /* Base wrapper method overrides */
        /*********************************/
        internal override void RebuildWrappedObject()
        {
            var list = new List<SPRKeyFrame>(Nodes.Count);

            for (int i = 0; i < Nodes.Count; i++)
            {
                WrappedObject.Add(((SPRKeyFrameWrapper)Nodes[i]).WrappedObject);
            }

            m_wrappedObject = list;
            m_isDirty = false;
        }

        internal override void InitializeWrapper()
        {
            Nodes.Clear();

            int index = 0;
            foreach (SPRKeyFrame keyFrame in WrappedObject)
            {
                Nodes.Add(new SPRKeyFrameWrapper(string.Format("KeyFrameData[{0}]", index++), keyFrame));
            }

            base.InitializeWrapper();
        }
    }
}
