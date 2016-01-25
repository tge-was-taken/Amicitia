using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWHierarchyAnimPlugin : RWNode
    {
        private uint _animVersion;

        public float AnimVersion
        {
            get { return (float)_animVersion / 0x100; }
        }

        public int FrameNameID { get; set; }
        public int NodeCount { get; set; }
        public RWHierarchyAnimFlag Flags { get; set; }
        public int KeyFrameSize { get; set; }
        public List<RWHierarchyAnimNode> NodeList { get; set; }

        public RWHierarchyAnimPlugin(uint size, uint version, RWNode parent, BinaryReader reader)
                : base(RWType.HierarchyAnimPlugin, size, version, parent)
        {
            _animVersion = reader.ReadUInt32();
            FrameNameID = reader.ReadInt32();
            NodeCount = reader.ReadInt32();
            if (NodeCount == 0)
                return;
            Flags = (RWHierarchyAnimFlag) reader.ReadUInt32();
            KeyFrameSize = reader.ReadInt32();
            NodeList = new List<RWHierarchyAnimNode>(NodeCount);
            for (int i = 0; i < NodeCount; i++)
                NodeList.Add(new RWHierarchyAnimNode(reader));
        }

        public RWHierarchyAnimPlugin(int boneNameID, RWHierarchyAnimFlag flags, List<RWHierarchyAnimNode> nodes)
            : base(RWType.HierarchyAnimPlugin)
        {
            _animVersion = 0x100;
            FrameNameID = boneNameID;
            NodeCount = nodes.Count;
            if (NodeCount == 0)
                return;
            Flags = flags;
            KeyFrameSize = 36;
            NodeList = nodes;
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write(_animVersion);
            writer.Write(FrameNameID);
            writer.Write(NodeCount);
            if (NodeCount == 0)
                return;
            writer.Write((uint)Flags);
            writer.Write(KeyFrameSize);
            for (int i = 0; i < NodeCount; i++)
                NodeList[i].Write(writer);
        }
    }
}
