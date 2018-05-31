using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    public class RwExtensionNode : RwNode
    {
        public RwExtensionNode(RwNode parent, params RwNode[] plugins)
            : base(RwNodeId.RwExtensionNode)
        {
            Children = plugins.ToList();
            foreach (var child in Children)
            {
                child.Parent = this;
            }
        }

        public RwExtensionNode(RwNode parent = null)
            : base(RwNodeId.RwExtensionNode, parent)
        {
            Children = new List<RwNode>();
        }

        internal RwExtensionNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            Children = new List<RwNode>();

            long end = reader.BaseStream.Position + header.Size;
            while (reader.BaseStream.Position != end)
            {
                RwNodeFactory.GetNode(this, reader);
            }
        }

        internal bool TryGetExtensionIndexOfType(RwNodeId id, out int index)
        {
            index = Children.FindIndex(n => n.Id == id);

            if (index != -1)
                return true;
            else
                return false;
        }

        protected internal override void WriteBody(BinaryWriter writer)
        {
            if (Children == null)
                return;

            foreach (RwNode child in Children)
                child.Write(writer);
        }
    }
}