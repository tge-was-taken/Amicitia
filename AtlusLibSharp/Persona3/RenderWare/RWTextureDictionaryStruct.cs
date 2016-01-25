using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWTextureDictionaryStruct : RWNode
    {
        public ushort TextureCount { get; private set; }
        public RWDeviceID DeviceID { get; private set; }

        internal RWTextureDictionaryStruct(uint size, uint version, RWNode parent, BinaryReader reader)
                : base(RWType.Struct, size, version, parent)
        {
            TextureCount = reader.ReadUInt16();
            DeviceID = (RWDeviceID)reader.ReadUInt16();
        }

        internal RWTextureDictionaryStruct(RWTextureDictionary txd)
            : base(RWType.Struct)
        {
            TextureCount = (ushort)txd.Textures.Length;
            DeviceID = RWDeviceID.PS2;
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write(TextureCount);
            writer.Write((ushort)DeviceID);
        }
    }
}