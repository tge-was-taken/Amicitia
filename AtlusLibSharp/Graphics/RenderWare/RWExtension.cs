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

        internal RWExtension(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            long end = reader.BaseStream.Position + header.Size;
            while (reader.BaseStream.Position != end)
            {
                RWNodeFactory.GetNode(this, reader);
            }
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