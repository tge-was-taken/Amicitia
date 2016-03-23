namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.Graphics.TMX;
    using System.Collections.Generic;
    using System.ComponentModel;

    internal class SPRFileTexturesWrapper : ResourceWrapper
    {
        /***************/
        /* Constructor */
        /***************/
        public SPRFileTexturesWrapper(string text, List<TMXFile> textures) 
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
        public new List<TMXFile> WrappedObject
        {
            get
            {
                return (List<TMXFile>)m_wrappedObject;
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
            var list = new List<TMXFile>(Nodes.Count);
            foreach (TMXFileWrapper node in Nodes)
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
            foreach (TMXFile texture in WrappedObject)
            {
                string name = "Texture" + index++;
                if (!string.IsNullOrEmpty(texture.UserComment))
                {
                    name = texture.UserComment;
                }

                Nodes.Add(new TMXFileWrapper(name, texture));
            }

            base.InitializeWrapper();
        }
    }
}
