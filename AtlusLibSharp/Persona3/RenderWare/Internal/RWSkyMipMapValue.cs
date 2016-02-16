namespace AtlusLibSharp.Persona3.RenderWare
{
    using System.IO;

    /// <summary>
    /// Represents a RenderWare node holding the Sky Mipmap value. Usage is unknown.
    /// </summary>
    internal class RWSkyMipMapValue : RWNode
    {
        internal const int SKY_MIPMAP_VALUE = 0x00000FC0;

        public RWSkyMipMapValue() : base(RWType.SkyMipMapValue) { }

        internal RWSkyMipMapValue(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            int skyMipMapValue = reader.ReadInt32();
        }

        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(SKY_MIPMAP_VALUE);
        }
    }
}
