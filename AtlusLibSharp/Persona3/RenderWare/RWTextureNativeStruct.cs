using System.IO;
using PS2;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWTextureNativeStruct : RWNode
    {
        private RWPlatformID _platformID;
        private uint _flags;

        public RWPlatformID PlatformID
        {
            get { return _platformID; }
        }

        public PS2FilterMode FilterMode
        {
            get
            {
                return (PS2FilterMode)(_flags & 0xFF);
            }
            set
            {
                _flags &= ~((uint)0xFF);
                _flags |= ((uint)value & 0xFF);
            }
        }

        public PS2AddressingMode UAdressingMode
        {
            get
            {
                return (PS2AddressingMode)((_flags & (0xF << 8)) >> 8);
            }
            set
            {
                _flags &= ~((uint)(0xF << 8));
                _flags |= (((uint)value & 0xF) << 8);
            }
        }

        public PS2AddressingMode VAddressingMode
        {
            get
            {
                return (PS2AddressingMode)((_flags & (0xF << 12)) >> 12);
            }
            set
            {
                _flags &= ~((uint)0xF << 12);
                _flags |= (((uint)value & 0xF) << 12);
            }
        }

        internal RWTextureNativeStruct(uint size, uint version, RWNode parent, BinaryReader reader)
            : base(RWType.Struct, size, version, parent)
        {
            _platformID = (RWPlatformID)reader.ReadUInt32();
            _flags = reader.ReadUInt32();
        }

        public RWTextureNativeStruct(RWPlatformID pfID = RWPlatformID.PS2, PS2FilterMode flt = PS2FilterMode.Linear, 
                                     PS2AddressingMode u = PS2AddressingMode.Wrap, PS2AddressingMode v = PS2AddressingMode.Wrap)
            : base(RWType.Struct)
        {
            _platformID = pfID;
            FilterMode = flt;
            UAdressingMode = u;
            VAddressingMode = v;
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write((uint)_platformID);
            writer.Write(_flags);
        }
    }
}