namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.SMT3.ChunkResources.Graphics;

    internal class SPRTexturesWrapper : ResourceWrapper
    {
        public SPRTexturesWrapper(string text, TMXChunk[] textures) : base(text, textures) { }

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
            TMXChunk[] textures = new TMXChunk[Nodes.Count];
            for (int i = 0; i < Nodes.Count; i++)
            {
                textures[i] = ((TMXWrapper)Nodes[i]).GetWrappedObject<TMXChunk>(GetWrapperOptions.ForceRebuild);
            }

            ReplaceWrappedObjectAndInitialize(textures);
        }

        protected override void InitializeWrapper()
        {
            Nodes.Clear();
            TMXChunk[] textures = GetWrappedObject<TMXChunk[]>();

            int index = -1;
            foreach (TMXChunk texture in textures)
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
