namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;
    using PS2.Graphics;
    using AtlusLibSharp.Utilities;

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

        public PS2FilterMode FilterMode
        {
            get { return (PS2FilterMode)BitHelper.GetBits(_flags, 8, 0); }
            set { BitHelper.ClearAndSetBits(ref _flags, 8, (uint)value, 0); }
        }

        public PS2AddressingMode HorizontalAddressingMode
        {
            get { return (PS2AddressingMode)BitHelper.GetBits(_flags, 4, 8); }
            set { BitHelper.ClearAndSetBits(ref _flags, 4, (uint)value, 8); }
        }

        public PS2AddressingMode VerticalAddressingMode
        {
            get { return (PS2AddressingMode)BitHelper.GetBits(_flags, 4, 12); }
            set { BitHelper.ClearAndSetBits(ref _flags, 4, (uint)value, 12); }
        }

        #endregion

        // Constructors
        public RWTextureNativeStruct(RWNode parent = null)
            : base(RWNodeType.Struct, parent)
        {
            _platformID = RWPlatformID.PS2;
            FilterMode = PS2FilterMode.Linear;
            HorizontalAddressingMode = PS2AddressingMode.Wrap;
            VerticalAddressingMode = PS2AddressingMode.Wrap;
        }

        public RWTextureNativeStruct(
            RWPlatformID rwPlatformID, PS2FilterMode filterMode, 
            PS2AddressingMode horizontalAddrMode, PS2AddressingMode verticalAddrMode,
            RWNode parent = null)
            : base(RWNodeType.Struct, parent)
        {
            _platformID = rwPlatformID;
            FilterMode = filterMode;
            HorizontalAddressingMode = horizontalAddrMode;
            VerticalAddressingMode = verticalAddrMode;
        }

        internal RWTextureNativeStruct(RWNodeFactory.RWNodeInfo header, BinaryReader reader)
            : base(header)
        {
            _platformID = (RWPlatformID)reader.ReadUInt32();
            _flags = reader.ReadUInt32();
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write((uint)_platformID);
            writer.Write(_flags);
        }
    }
}