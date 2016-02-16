using AtlusLibSharp.Common;

namespace Amicitia.ResourceWrappers
{
    internal class SPR4TexturesWrapper : ResourceWrapper
    {
        public SPR4TexturesWrapper(string text, GenericBinaryFile[] textures) : base(text, textures) { }

        protected internal new GenericBinaryFile[] WrappedObject
        {
            get { return (GenericBinaryFile[])base.WrappedObject; }
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
            WrappedObject = new GenericBinaryFile[Nodes.Count];

            for (int i = 0; i < Nodes.Count; i++)
            {
                // rebuild before getting the data
                ResourceWrapper node = (ResourceWrapper)Nodes[i];
                node.RebuildWrappedObject();

                // set the wrapped object
                WrappedObject[i] = new GenericBinaryFile(node.GetBytes());
            }
        }

        protected internal override void InitializeWrapper()
        {
            Nodes.Clear();

            int index = -1;
            foreach (GenericBinaryFile texture in WrappedObject)
            {
                ++index;

                string name = "Texture" + index + ".tga";

                Nodes.Add(new ResourceWrapper(name, texture));
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
