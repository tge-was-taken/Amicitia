using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    using PS2.Graphics;
    using Utilities;

    public class RWTextureNativeStruct : RWNode
    {
        // Fields
        private RWPlatformID _platformID;
        private uint _flags;

        // Properties
        public RWPlatformID PlatformID
        {
            get { return _platformID; }
        }

        public FilterMode FilterMode
        {
            get { return (FilterMode)BitHelper.GetBits(_flags, 8, 0); }
            set { BitHelper.ClearAndSetBits(ref _flags, 8, (uint)value, 0); }
        }

        public AddressingMode HorizontalAddressingMode
        {

            get { return (AddressingMode)BitHelper.GetBits(_flags, 4, 8); }
            set { BitHelper.ClearAndSetBits(ref _flags, 4, (uint)value, 8); }
        }

        public AddressingMode VerticalAddressingMode
        {
            get { return (AddressingMode)BitHelper.GetBits(_flags, 4, 12); }
            set { BitHelper.ClearAndSetBits(ref _flags, 4, (uint)value, 12); }
        }

        // Constructors
        public RWTextureNativeStruct()
            : base(RWType.Struct)
        {
            _platformID = RWPlatformID.PS2;
            FilterMode = FilterMode.Linear;
            HorizontalAddressingMode = AddressingMode.Wrap;
            VerticalAddressingMode = AddressingMode.Wrap;
        }

        public RWTextureNativeStruct(RWPlatformID rwPlatformID, FilterMode filterMode, 
                                     AddressingMode horizontalAddrMode, AddressingMode verticalAddrMode)
            : base(RWType.Struct)
        {
            _platformID = rwPlatformID;
            FilterMode = filterMode;
            HorizontalAddressingMode = horizontalAddrMode;
            VerticalAddressingMode = verticalAddrMode;
        }

        internal RWTextureNativeStruct(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            _platformID = (RWPlatformID)reader.ReadUInt32();
            _flags = reader.ReadUInt32();
        }
        
        // Methods
        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write((uint)_platformID);
            writer.Write(_flags);
        }
    }
}