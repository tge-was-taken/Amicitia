namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.Graphics.SPR;
    using System.Collections.Generic;

    internal class SPRKeyFrameListWrapper : ResourceWrapper
    {
        public SPRKeyFrameListWrapper(string text, List<SPRKeyFrame> keyFrames) : base(text, keyFrames) { }

        protected internal new List<SPRKeyFrame> WrappedObject
        {
            get { return (List<SPRKeyFrame>)base.WrappedObject; }
            set { base.WrappedObject = value; }
        }

        protected internal override bool CanExport
        {
            get { return false; }
        }

        protected internal override bool CanMove
        {
            get { return false; }
        }

        protected internal override bool CanRename
        {
            get { return false; }
        }

        protected internal override bool CanReplace
        {
            get { return false; }
        }

        protected internal override bool CanDelete
        {
            get { return false; }
        }

        protected internal override void RebuildWrappedObject()
        {
            WrappedObject = new List<SPRKeyFrame>(Nodes.Count);

            for (int i = 0; i < Nodes.Count; i++)
            {
                WrappedObject.Add(((SPRKeyFrameWrapper)Nodes[i]).WrappedObject);
            }
        }

        protected internal override void InitializeWrapper()
        {
            Nodes.Clear();

            int index = 0;
            foreach (SPRKeyFrame keyFrame in WrappedObject)
            {
                Nodes.Add(new SPRKeyFrameWrapper(string.Format("KeyFrame{0}", index++), keyFrame));
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
