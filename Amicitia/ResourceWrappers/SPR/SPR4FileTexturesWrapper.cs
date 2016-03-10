using AtlusLibSharp.Graphics.TGA;
using System.Collections.Generic;

namespace Amicitia.ResourceWrappers
{
    internal class SPR4TexturesWrapper : ResourceWrapper
    {
        public SPR4TexturesWrapper(string text, List<TGAFile> textures) : base(text, textures) { }

        protected internal new List<TGAFile> WrappedObject
        {
            get { return (List<TGAFile>)base.WrappedObject; }
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
            WrappedObject = new List<TGAFile>();

            for (int i = 0; i < Nodes.Count; i++)
            {
                // rebuild before getting the data
                TGAFileWrapper node = (TGAFileWrapper)Nodes[i];
                node.RebuildWrappedObject();

                // set the wrapped object
                WrappedObject.Add(node.WrappedObject);
            }
        }

        protected internal override void InitializeWrapper()
        {
            Nodes.Clear();

            int index = 0;
            foreach (TGAFile texture in WrappedObject)
            {
                string name = "Texture" + (index++) + ".tga";

                Nodes.Add(new TGAFileWrapper(name, texture));
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
