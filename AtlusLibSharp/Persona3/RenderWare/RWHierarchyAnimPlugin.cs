using System;
using System.Collections.Generic;
using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWHierarchyAnimPlugin : RWNode
    {
        private const int VERSION = 0x100;
        private const int KEYFRAME_SIZE = 36;

        private int _nameID;
        private RWHierarchyAnimFlag _flag;
        private List<RWHierarchyAnimNode> _nodes = new List<RWHierarchyAnimNode>();

        public int FrameNameID
        {
            get { return _nameID; }
        }

        public int NodeCount
        {
            get { return _nodes.Count; }
        }

        public RWHierarchyAnimFlag Flags
        {
            get { return _flag; }
        }

        public List<RWHierarchyAnimNode> Nodes
        {
            get { return _nodes; }
        }

        public RWHierarchyAnimPlugin(int frameNameID)
            : base(RWType.HierarchyAnimPlugin)
        {
            _nameID = frameNameID;
            _flag = 0;
        }

        public RWHierarchyAnimPlugin(int frameNameID, RWHierarchyAnimFlag flags, List<RWHierarchyAnimNode> nodes)
            : base(RWType.HierarchyAnimPlugin)
        {
            _nameID = frameNameID;

            if (nodes.Count == 0)
                throw new ArgumentException("List of hierarchy anim nodes cannot be empty!");

            _flag = flags;
            _nodes = nodes;
        }

        internal RWHierarchyAnimPlugin(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
                : base(header)
        {
            int version = reader.ReadInt32();

            if (version != VERSION)
            {
                throw new NotImplementedException("Unexpected version for RWHierarchyAnimPlugin");
            }

            _nameID = reader.ReadInt32();
            int numNodes = reader.ReadInt32();

            if (numNodes == 0)
                return;

            _flag = (RWHierarchyAnimFlag)reader.ReadUInt32();
            int keyFrameSize = reader.ReadInt32();
            _nodes = new List<RWHierarchyAnimNode>(numNodes);

            for (int i = 0; i < numNodes; i++)
            {
                _nodes.Add(new RWHierarchyAnimNode(reader));
            }
        }

        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(VERSION);
            writer.Write(_nameID);
            writer.Write(NodeCount);

            if (NodeCount == 0)
                return;

            writer.Write((uint)_flag);
            writer.Write(KEYFRAME_SIZE);

            for (int i = 0; i < NodeCount; i++)
                _nodes[i].InternalWrite(writer);
        }
    }
}
