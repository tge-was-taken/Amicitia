namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;

    internal class RWUVAnimationDictionaryStructNode : RwNode
    {
        public int UvAnimationCount { get; set; }

        public RWUVAnimationDictionaryStructNode(int uvAnimationCount, RwNode parent = null)
            : base(RwNodeId.RwStructNode, parent)
        {
            UvAnimationCount = uvAnimationCount;
        }

        internal RWUVAnimationDictionaryStructNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            UvAnimationCount = reader.ReadInt32();
        }

        public RWUVAnimationDictionaryStructNode(RwNode parent, int count) : base(RwNodeId.RwStructNode, parent)
        {
            UvAnimationCount = count;
        }

        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write(UvAnimationCount);
        }
    }
}
