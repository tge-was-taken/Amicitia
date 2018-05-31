namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.IO;

    internal class RwGeometryListStructNode : RwNode
    {
        private int mGeometryCount;

        public int GeometryCount
        {
            get { return mGeometryCount; }
            set { mGeometryCount = value; }
        }

        internal RwGeometryListStructNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
                : base(header)
        {
            mGeometryCount = reader.ReadInt32();
        }

        internal RwGeometryListStructNode(RwGeometryListNode listNode)
            : base(RwNodeId.RwStructNode, listNode)
        {
            mGeometryCount = listNode.Count;
        }

        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write(mGeometryCount);
        }
    }
}