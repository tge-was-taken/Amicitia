namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.SMT3.Graphics;

    internal class SPRKeyFrameListWrapper : ResourceWrapper
    {
        public SPRKeyFrameListWrapper(string text, SPRKeyFrame[] keyFrames) : base(text, keyFrames) { }

        protected internal new SPRKeyFrame[] WrappedObject
        {
            get { return (SPRKeyFrame[])base.WrappedObject; }
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
            WrappedObject = new SPRKeyFrame[Nodes.Count];

            for (int i = 0; i < Nodes.Count; i++)
            {
                WrappedObject[i] = ((SPRKeyFrameWrapper)Nodes[i]).WrappedObject;
            }
        }

        protected internal override void InitializeWrapper()
        {
            Nodes.Clear();

            int index = -1;
            foreach (SPRKeyFrame keyFrame in WrappedObject)
            {
                ++index;
                Nodes.Add(new SPRKeyFrameWrapper("KeyFrame" + index, keyFrame));
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
