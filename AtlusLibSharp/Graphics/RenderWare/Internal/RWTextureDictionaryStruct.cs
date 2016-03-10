namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;

    /// <summary>
    /// Stores internal values of an <see cref="RWTextureDictionary"/> instance.
    /// </summary>
    internal class RWTextureDictionaryStruct : RWNode
    {
        public ushort TextureCount { get; internal set; }
        public RWDeviceID DeviceID { get; internal set; }

        internal RWTextureDictionaryStruct(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
                : base(header)
        {
            TextureCount = reader.ReadUInt16();
            DeviceID = (RWDeviceID)reader.ReadUInt16();
        }

        internal RWTextureDictionaryStruct(RWTextureDictionary texDictionary)
            : base(RWType.Struct, texDictionary)
        {
            TextureCount = (ushort)texDictionary.TextureCount;
            DeviceID = texDictionary.DeviceID;
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(TextureCount);
            writer.Write((ushort)DeviceID);
        }
    }
}