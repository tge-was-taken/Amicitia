namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.IO;

    /// <summary>
    /// Represents a RenderWare node holding the Sky Mipmap value. Usage is unknown.
    /// </summary>
    internal class RwSkyMipMapValueNode : RwNode
    {
        internal const int SKY_MIPMAP_VALUE = 0x00000FC0;

        public int Value { get; }

        public RwSkyMipMapValueNode() : base(RwNodeId.RwSkyMipMapValueNode)
        {
            Value = SKY_MIPMAP_VALUE;
        }

        internal RwSkyMipMapValueNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            Value = reader.ReadInt32();
        }

        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write(Value);
        }
    }
}
