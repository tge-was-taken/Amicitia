namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.SMT3.ChunkResources.Graphics;

    internal class SPRKeyFrameWrapper : ResourceWrapper
    {
        public SPRKeyFrameWrapper(string text, SPRKeyFrame keyFrame) : base(text, keyFrame) { }

        // Properties
        public string Comment { get; set; }

        protected internal override bool IsExportable
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

        // Protected Methods
        protected override void InitializeWrapper()
        {
            SPRKeyFrame keyFrame = GetWrappedObject<SPRKeyFrame>();

            Comment = keyFrame.Comment;

            if (IsInitialized)
            {
                MainForm.Instance.UpdateReferences();
            }
            else
            {
                IsInitialized = true;
            }
        }

        protected override void RebuildWrappedObject()
        {
            // Update the changed properties in the wrapped object
            SPRKeyFrame keyFrame = GetWrappedObject<SPRKeyFrame>();
            keyFrame.Comment = Comment;

            // Re-intialize the wrapper to update it
            InitializeWrapper();
        }
    }
}
