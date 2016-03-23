using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AtlusLibSharp.Graphics.RenderWare
{
    internal class RWExtension : RWNode
    {
        public RWExtension(params RWNode[] plugins)
            : base(RWNodeType.Extension)
        {
            Children = plugins.ToList();
        }

        public RWExtension(RWNode parent = null)
            : base(RWNodeType.Extension, parent)
        {
            Children = new List<RWNode>();
        }

        internal RWExtension(RWNodeFactory.RWNodeInfo header, BinaryReader reader)
            : base(header)
        {
            long end = reader.BaseStream.Position + header.Size;
            while (reader.BaseStream.Position != end)
            {
                RWNodeFactory.GetNode(this, reader);
            }
        }

        internal bool TryGetExtensionIndexOfType(RWNodeType type, out int index)
        {
            index = Children.FindIndex(n => n.Type == type);

            if (index != -1)
                return true;
            else
                return false;
        }

        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            if (Children == null)
                return;

            foreach (RWNode child in Children)
                child.InternalWrite(writer);
        }
    }
}