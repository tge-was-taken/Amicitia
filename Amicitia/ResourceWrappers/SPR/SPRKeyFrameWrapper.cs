namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.SMT3.Graphics;

    internal class SPRKeyFrameWrapper : ResourceWrapper
    {
        public SPRKeyFrameWrapper(string text, SPRKeyFrame keyFrame) : base(text, keyFrame) { }

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
