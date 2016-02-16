namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.SMT3.Graphics;

    internal class SPRFileTexturesWrapper : ResourceWrapper
    {
        public SPRFileTexturesWrapper(string text, TMXFile[] textures) : base(text, textures) { }

        protected internal new TMXFile[] WrappedObject
        {
            get { return (TMXFile[])base.WrappedObject; }
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
            WrappedObject = new TMXFile[Nodes.Count];
            for (int i = 0; i < Nodes.Count; i++)
            {
                // rebuild the data
                TMXFileWrapper node = (TMXFileWrapper)Nodes[i];
                node.RebuildWrappedObject();

                // set the wrapped object
                WrappedObject[i] = node.WrappedObject;
            }
        }

        protected internal override void InitializeWrapper()
        {
            Nodes.Clear();

            int index = -1;
            foreach (TMXFile texture in WrappedObject)
            {
                ++index;

                string name = "Texture" + index;
                if (!string.IsNullOrEmpty(texture.UserComment))
                {
                    name = texture.UserComment + ".tmx";
                }

                Nodes.Add(new TMXFileWrapper(name, texture));
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
