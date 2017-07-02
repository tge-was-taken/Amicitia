namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;

    internal class RwClumpStructNode : RwNode
    {
        private int mAtomicCount;
        private int mLightCount;
        private int mCameraCount;

        public int AtomicCount
        {
            get { return mAtomicCount; }
        }

        public int LightCount
        {
            get { return mLightCount; }
        }

        public int CameraCount
        {
            get { return mCameraCount; }
        }

        internal RwClumpStructNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            mAtomicCount = reader.ReadInt32();
            mLightCount = reader.ReadInt32(); 
            mCameraCount = reader.ReadInt32();
        }

        internal RwClumpStructNode(RwClumpNode clumpNode)
            : base(new RwNodeFactory.RwNodeHeader { Parent = clumpNode, Id = RwNodeId.RwStructNode, Version = ExportVersion })
        {
            mAtomicCount = clumpNode.Atomics.Count;
            mLightCount = 0;
            mCameraCount = 0;
        }

        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write(mAtomicCount);
            writer.Write(mLightCount);
            writer.Write(mCameraCount);
        }
    }
}
