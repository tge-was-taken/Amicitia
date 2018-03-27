namespace AtlusLibSharp.Graphics.RenderWare
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class RwHAnimFrameExtensionNode : RwNode
    {
        private const int VERSION = 0x100;
        private const int KEYFRAME_SIZE = 36;

        public int NameId { get; }

        public bool IsRootBone
        {
            get { return Hierarchy != null; }
        }

        public RwHAnimHierarchy Hierarchy { get; }

        public RwHAnimFrameExtensionNode(int boneName)
            : base(RwNodeId.RwHAnimFrameExtensionNode)
        {
            NameId = boneName;
        }

        public RwHAnimFrameExtensionNode(int boneName, RwHAnimHierarchy hierarchy)
            : base(RwNodeId.RwHAnimFrameExtensionNode)
        {
            NameId = boneName;

            Hierarchy = hierarchy ?? throw new ArgumentNullException("hierarchy");
        }

        internal RwHAnimFrameExtensionNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
                : base(header)
        {
            int version = reader.ReadInt32();

            if (version != VERSION)
            {
                throw new NotImplementedException("Unexpected version for RWHierarchyAnimPlugin");
            }

            NameId = reader.ReadInt32();
            int numNodes = reader.ReadInt32();

            if (numNodes == 0)
                return;

            RwHAnimHierarchyFlags flags = (RwHAnimHierarchyFlags)reader.ReadUInt32();
            int keyFrameSize = reader.ReadInt32();
            List< RwHAnimNodeInfo> nodes = new List<RwHAnimNodeInfo>(numNodes);

            for (int i = 0; i < numNodes; i++)
            {
                nodes.Add(new RwHAnimNodeInfo(reader));
            }

            Hierarchy = new RwHAnimHierarchy(flags, nodes);
        }

        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write(VERSION);
            writer.Write(NameId);

            if (!IsRootBone)
            {
                writer.Write(0);
                return;
            }
            else
            {
                writer.Write(Hierarchy.Nodes.Count);
            }

            writer.Write((uint)Hierarchy.Flags);
            writer.Write(KEYFRAME_SIZE);

            for (int i = 0; i < Hierarchy.Nodes.Count; i++)
                Hierarchy.Nodes[i].Write(writer);
        }
    }
}
