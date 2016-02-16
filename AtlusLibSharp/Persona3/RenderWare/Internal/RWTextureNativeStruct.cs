namespace AtlusLibSharp.Persona3.RenderWare
{
    using System.IO;
    using PS2.Graphics;
    using Common.Utilities;

    /// <summary>
    /// Stores internal values for an <see cref="RWTextureNative"/> instance.
    /// </summary>
    internal class RWTextureNativeStruct : RWNode
    {
        // Fields
        private RWPlatformID _platformID;
        private uint _flags;

        #region Properties

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

        #endregion

        // Constructors
        public RWTextureNativeStruct(RWNode parent = null)
            : base(RWType.Struct, parent)
        {
            _platformID = RWPlatformID.PS2;
            FilterMode = FilterMode.Linear;
            HorizontalAddressingMode = AddressingMode.Wrap;
            VerticalAddressingMode = AddressingMode.Wrap;
        }

        public RWTextureNativeStruct(
            RWPlatformID rwPlatformID, FilterMode filterMode, 
            AddressingMode horizontalAddrMode, AddressingMode verticalAddrMode,
            RWNode parent = null)
            : base(RWType.Struct, parent)
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

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write((uint)_platformID);
            writer.Write(_flags);
        }
    }
}