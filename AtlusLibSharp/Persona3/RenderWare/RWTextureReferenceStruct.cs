using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    using PS2.Graphics;

    public class RWTextureReferenceStruct : RWNode
    {
        // Fields
        private uint _flags;

        // Properties
        public FilterMode FilterMode
        {
            get { return (FilterMode)((_flags & 0xFF)); }
            set
            {
                _flags &= ~((uint)0xFF);
                _flags |= ((uint)value & 0xFF);
            }
        }

        public AddressingMode UAdressingMode
        {
            get { return (AddressingMode)((_flags & 0xF00) >> 8); }
            set
            {
                _flags &= ~((uint)0xF << 8);
                _flags |= (((uint)value & 0xF) << 8);
            }
        }

        public AddressingMode VAdressingMode
        {
            get { return (AddressingMode)((_flags & 0xF000) >> 12); }
            set
            {
                _flags &= ~((uint)0xF << 12);
                _flags |= (((uint)value & 0xF) << 12);
            }
        }

        public bool HasMipMaps
        {
            get { return (((_flags & 0x10000) >> 16) == 1); }
            set
            {
                _flags &= ~(((uint)0x1 << 16));
                if (value)
                    _flags |= ((1) << 16);
                else
                    _flags |= ((0) << 16);
            }
        }

        // Constructors
        public RWTextureReferenceStruct()
            : base(RWType.Struct)
        {
            FilterMode = FilterMode.Linear;
            UAdressingMode = AddressingMode.Wrap;
            VAdressingMode = AddressingMode.Wrap;
            HasMipMaps = false;
        }

        public RWTextureReferenceStruct(FilterMode fltr, AddressingMode uA, AddressingMode vA, bool mip)
            : base(RWType.Struct)
        {
            FilterMode = fltr;
            UAdressingMode = uA;
            VAdressingMode = vA;
            HasMipMaps = mip;
        }

        internal RWTextureReferenceStruct(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            _flags = reader.ReadUInt32();
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write(_flags);
        }
    }
}