using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public struct RWHierarchyAnimNode
    {
        public uint FrameNameID { get; set; }
        public uint HierarchyIndex { get; set; }
        public RWHierarchyNodeFlag Flags { get; set; }

        public RWHierarchyAnimNode(BinaryReader reader)
        {
            FrameNameID = reader.ReadUInt32();
            HierarchyIndex = reader.ReadUInt32();
            Flags = (RWHierarchyNodeFlag) reader.ReadUInt32();
        }

        public RWHierarchyAnimNode(uint frameNameID, uint hierarchyIndex, RWHierarchyNodeFlag flags)
        {
            FrameNameID = frameNameID;
            HierarchyIndex = hierarchyIndex;
            Flags = flags;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(FrameNameID);
            writer.Write(HierarchyIndex);
            writer.Write((uint)Flags);
        }
    }
}