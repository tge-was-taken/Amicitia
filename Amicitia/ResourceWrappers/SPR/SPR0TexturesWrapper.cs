namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.SMT3.ChunkResources.Graphics;

    internal class SPR0TexturesWrapper : ResourceWrapper
    {
        public SPR0TexturesWrapper(string text, TMXFile[] textures) : base(text, textures) { }

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
            TMXFile[] textures = new TMXFile[Nodes.Count];
            for (int i = 0; i < Nodes.Count; i++)
            {
                textures[i] = ((TMXWrapper)Nodes[i]).GetWrappedObject<TMXFile>(GetWrapperOptions.ForceRebuild);
            }

            ReplaceWrappedObjectAndInitialize(textures);
        }

        protected override void InitializeWrapper()
        {
            Nodes.Clear();
            TMXFile[] textures = GetWrappedObject<TMXFile[]>();

            int index = -1;
            foreach (TMXFile texture in textures)
            {
                ++index;

                string name = "Texture" + index;
                if (!string.IsNullOrEmpty(texture.UserComment))
                {
                    name = texture.UserComment + ".tmx";
                }

                Nodes.Add(new TMXWrapper(name, texture));
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
