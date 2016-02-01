using AtlusLibSharp;

namespace Amicitia.ResourceWrappers
{
    internal class SPR4TexturesWrapper : ResourceWrapper
    {
        public SPR4TexturesWrapper(string text, GenericBinaryFile[] textures) : base(text, textures) { }

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
            GenericBinaryFile[] textures = new GenericBinaryFile[Nodes.Count];
            for (int i = 0; i < Nodes.Count; i++)
            {
                textures[i] = new GenericBinaryFile(((ResourceWrapper)Nodes[i]).GetBytes());
            }

            ReplaceWrappedObjectAndInitialize(textures);
        }

        protected override void InitializeWrapper()
        {
            Nodes.Clear();
            GenericBinaryFile[] textures = GetWrappedObject<GenericBinaryFile[]>();

            int index = -1;
            foreach (GenericBinaryFile texture in textures)
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
