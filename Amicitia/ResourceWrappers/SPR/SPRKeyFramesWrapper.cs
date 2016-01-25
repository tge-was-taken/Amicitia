namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.SMT3.ChunkResources.Graphics;

    internal class SPRKeyFramesWrapper : ResourceWrapper
    {
        public SPRKeyFramesWrapper(string text, SPRKeyFrame[] keyFrames) : base(text, keyFrames) { }

        // Properties
        protected internal override bool IsExportable
        {
            get { return false; }
        }

        protected internal override bool IsMoveable
        {
            get { return false; }
        }

        protected internal override bool IsRenameable
        {
            get { return false; }
        }

        protected internal override bool IsReplaceable
        {
            get { return false; }
        }

        protected internal override bool IsDeleteable
        {
            get { return false; }
        }

        // Protected Methods
        protected override void RebuildWrappedObject()
        {
            SPRKeyFrame[] keyFrames = new SPRKeyFrame[Nodes.Count];

            for (int i = 0; i < Nodes.Count; i++)
            {
                keyFrames[i] = ((SPRKeyFrameWrapper)Nodes[i]).GetWrappedObject<SPRKeyFrame>(GetWrapperOptions.ForceRebuild);
            }

            ReplaceWrappedObjectAndInitialize(keyFrames);
        }

        protected override void InitializeWrapper()
        {
            Nodes.Clear();
            SPRKeyFrame[] keyFrames = GetWrappedObject<SPRKeyFrame[]>();

            int index = -1;
            foreach (SPRKeyFrame keyFrame in keyFrames)
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
