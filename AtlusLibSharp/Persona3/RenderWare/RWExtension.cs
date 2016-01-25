using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWExtension : RWNode
    {
        public List<RWNode> Plugins
        {
            get { return Children; }
            set
            {
                Children = value;
                if (value == null)
                    return;
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].Parent = this;
                }
            }
        }

        public RWExtension() : base(RWType.Extension) { }

        public RWExtension(params RWNode[] plugins)
            : base(RWType.Extension)
        {
            Plugins = plugins.ToList();
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

        protected override void InternalWriteData(BinaryWriter writer)
        {
            if (Children == null)
                return;

            foreach (RWNode child in Children)
                child.InternalWrite(writer);
        }
    }
}