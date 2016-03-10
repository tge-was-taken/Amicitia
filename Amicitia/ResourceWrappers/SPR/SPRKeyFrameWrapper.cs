namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.Graphics.SPR;

    internal class SPRKeyFrameWrapper : ResourceWrapper
    {
        public SPRKeyFrameWrapper(string text, SPRKeyFrame keyFrame) : base(text, keyFrame) { }

        public SPRKeyFrameWrapper()
            : base(string.Empty, null)
        {

        }

        protected internal new SPRKeyFrame WrappedObject
        {
            get { return (SPRKeyFrame)base.WrappedObject; }
            set { base.WrappedObject = value; }
        }

        public string Comment
        {
            get { return WrappedObject.Comment; }
            set { WrappedObject.Comment = value; }
        }

        protected internal override bool CanExport
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
    }
}
