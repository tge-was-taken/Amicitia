namespace Amicitia.ResourceWrappers
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using AtlusLibSharp.Graphics.TGA;

    internal class SPR4FileTexturesWrapper : ResourceWrapper
    {
        /***************/
        /* Constructor */
        /***************/
        public SPR4FileTexturesWrapper(string text, List<TGAFile> textures) 
            : base(text, textures, SupportedFileType.Resource, true)
        {
            m_canExport = false;
            m_canMove = false;
            m_canRename = false;
            m_canReplace = false;
            m_canDelete = false;
            InitializeContextMenuStrip();
        }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new List<TGAFile> WrappedObject
        {
            get
            {
                return (List<TGAFile>)m_wrappedObject;
            }
            set
            {
                m_wrappedObject = value;
            }
        }

        /*********************************/
        /* Base wrapper method overrides */
        /*********************************/
        internal override void RebuildWrappedObject()
        {
            var list = new List<TGAFile>(Nodes.Count);
            foreach (TGAFileWrapper node in Nodes)
            {
                list.Add(node.WrappedObject);
            }

            m_wrappedObject = list;
            m_isDirty = false;
        }

        internal override void InitializeWrapper()
        {
            Nodes.Clear();

            int index = 0;
            foreach (TGAFile texture in WrappedObject)
            {
                string name = string.Format("Texture[{0}]", index++);
                Nodes.Add(new TGAFileWrapper(name, texture));
            }

            base.InitializeWrapper();
        }
    }
}
