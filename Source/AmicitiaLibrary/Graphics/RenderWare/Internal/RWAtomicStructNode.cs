namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.IO;

    internal class RwAtomicStructNode : RwNode
    {
        private int mFrameIndex;
        private int mGeometryIndex;
        private int mFlag1;
        private int mFlag2;
        
        public int FrameIndex
        {
            get { return mFrameIndex; }
            set { mFrameIndex = value; }
        }

        public int GeometryIndex
        {
            get { return mGeometryIndex; }
            set { mGeometryIndex = value; }
        }

        public int Flag1
        {
            get { return mFlag1; }
            set { mFlag1 = value; }
        }

        public int Flag2
        {
            get { return mFlag2; }
            set { mFlag2 = value; }
        }

        public RwAtomicStructNode(int frameIndex, int geometryIndex, int flag1, int flag2, RwNode parent = null)
            : base(RwNodeId.RwStructNode, parent)
        {
            mFrameIndex = frameIndex;
            mGeometryIndex = geometryIndex;
            mFlag1 = flag1;
            mFlag2 = flag2;
        }

        internal RwAtomicStructNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            mFrameIndex = reader.ReadInt32();
            mGeometryIndex = reader.ReadInt32();
            mFlag1 = reader.ReadInt32();
            mFlag2 = reader.ReadInt32();
        }

        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write(mFrameIndex);
            writer.Write(mGeometryIndex);
            writer.Write(mFlag1);
            writer.Write(mFlag2);
        }
    }
}