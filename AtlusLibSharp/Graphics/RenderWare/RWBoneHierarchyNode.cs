using System.IO;

namespace AtlusLibSharp.Graphics.RenderWare
{
    public struct RWBoneHierarchyNode
    {
        private int _nameID;
        private int _hIndex;
        private RWHierarchyNodeFlag _flag;

        public int FrameNameID
        {
            get { return _nameID; }
        }

        public int HierarchyIndex
        {
            get { return _hIndex; }
        }

        public RWHierarchyNodeFlag Flags
        {
            get { return _flag; }
        }

        public RWBoneHierarchyNode(int boneNameID, int hierarchyIndex, RWHierarchyNodeFlag flag)
        {
            _nameID = boneNameID;
            _hIndex = hierarchyIndex;
            _flag = flag;
        }

        internal RWBoneHierarchyNode(BinaryReader reader)
        {
            _nameID = reader.ReadInt32();
            _hIndex = reader.ReadInt32();
            _flag = (RWHierarchyNodeFlag)reader.ReadUInt32();
        }

        internal void InternalWrite(BinaryWriter writer)
        {
            writer.Write(_nameID);
            writer.Write(_hIndex);
            writer.Write((uint)_flag);
        }
    }
}