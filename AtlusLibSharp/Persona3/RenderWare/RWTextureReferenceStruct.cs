using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    using PS2.Graphics;
    using Utilities;

    public class RWTextureReferenceStruct : RWNode
    {
        // Fields
        private uint _flags;

        // Properties
        public FilterMode FilterMode
        {
            get { return (FilterMode)BitHelper.GetBits(_flags, 8, 0); }
            set { BitHelper.ClearAndSetBits(ref _flags, 8, (uint)value, 0); }
        }

        public AddressingMode HorizontalAdressingMode
        {
            get { return (AddressingMode)BitHelper.GetBits(_flags, 4, 8); }
            set { BitHelper.ClearAndSetBits(ref _flags, 4, (uint)value, 8); }
        }

        public AddressingMode VerticalAdressingMode
        {
            get { return (AddressingMode)BitHelper.GetBits(_flags, 4, 12); }
            set { BitHelper.ClearAndSetBits(ref _flags, 4, (uint)value, 12); }
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
            HorizontalAdressingMode = AddressingMode.Wrap;
            VerticalAdressingMode = AddressingMode.Wrap;
            HasMipMaps = false;
        }

        public RWTextureReferenceStruct(FilterMode fltr, AddressingMode uA, AddressingMode vA, bool mip)
            : base(RWType.Struct)
        {
            FilterMode = fltr;
            HorizontalAdressingMode = uA;
            VerticalAdressingMode = vA;
            HasMipMaps = mip;
        }

        internal RWTextureReferenceStruct(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            _flags = reader.ReadUInt32();
        }

        // methods
        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write(_flags);
        }
    }
}