namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;

    /// <summary>
    /// Stores internal values of an <see cref="RwTextureDictionaryNode"/> instance.
    /// </summary>
    internal class RwTextureDictionaryStructNode : RwNode
    {
        public ushort TextureCount { get; set; }

        public RwDeviceId DeviceId { get; set; }

        internal RwTextureDictionaryStructNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
                : base(header)
        {
            TextureCount = reader.ReadUInt16();
            DeviceId = (RwDeviceId)reader.ReadUInt16();
        }

        internal RwTextureDictionaryStructNode(RwTextureDictionaryNode texDictionaryNode)
            : base(RwNodeId.RwStructNode, texDictionaryNode)
        {
            TextureCount = (ushort)texDictionaryNode.TextureCount;
            DeviceId = texDictionaryNode.DeviceId;
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write(TextureCount);
            writer.Write((ushort)DeviceId);
        }
    }
}