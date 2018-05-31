using System.IO;
using AmicitiaLibrary.IO;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    public class RwHAnimNodeInfo : BinaryBase
    {
        public int NodeId { get; set; }

        public int Index { get; set; }

        public RwHierarchyNodeFlag Flags { get; set; }

        public RwHAnimNodeInfo(int nodeId, int index, RwHierarchyNodeFlag flags)
        {
            NodeId = nodeId;
            Index = index;
            Flags = flags;
        }

        internal RwHAnimNodeInfo(BinaryReader reader)
        {
            NodeId = reader.ReadInt32();
            Index = reader.ReadInt32();
            Flags = (RwHierarchyNodeFlag)reader.ReadUInt32();
        }

        internal override void Write(BinaryWriter writer)
        {
            writer.Write(NodeId);
            writer.Write(Index);
            writer.Write((uint)Flags);
        }
    }
}