namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;
    using PS2.Graphics;
    using AtlusLibSharp.Utilities;

    /// <summary>
    /// Class holding internal values for an <see cref="RWTextureReference"/> instance.
    /// </summary>
    internal class RWTextureReferenceStruct : RWNode
    {
        private uint _flags;

        #region Properties

        public PS2FilterMode FilterMode
        {
            get { return (PS2FilterMode)BitHelper.GetBits(_flags, 8, 0); }
            set { BitHelper.ClearAndSetBits(ref _flags, 8, (uint)value, 0); }
        }

        public PS2AddressingMode HorizontalAdressingMode
        {
            get { return (PS2AddressingMode)BitHelper.GetBits(_flags, 4, 8); }
            set { BitHelper.ClearAndSetBits(ref _flags, 4, (uint)value, 8); }
        }

        public PS2AddressingMode VerticalAdressingMode
        {
            get { return (PS2AddressingMode)BitHelper.GetBits(_flags, 4, 12); }
            set { BitHelper.ClearAndSetBits(ref _flags, 4, (uint)value, 12); }
        }

        public bool HasMipMaps
        {
            get { return BitHelper.IsBitSet(_flags, 16); }
            set
            {
                BitHelper.ClearBit(ref _flags, 16);

                if (value)
                    BitHelper.SetBit(ref _flags, 16);
            }
        }

        #endregion Properties

        public RWTextureReferenceStruct(RWNode parent = null)
            : base(RWNodeType.Struct, parent)
        {
            FilterMode = PS2FilterMode.Linear;
            HorizontalAdressingMode = PS2AddressingMode.Wrap;
            VerticalAdressingMode = PS2AddressingMode.Wrap;
            HasMipMaps = false;
        }

        public RWTextureReferenceStruct(
            PS2FilterMode filterMode, 
            PS2AddressingMode horizontalAddrMode, PS2AddressingMode verticalAddrMode, 
            bool hasMipMaps,
            RWNode parent = null)
            : base(RWNodeType.Struct, parent)
        {
            FilterMode = filterMode;
            HorizontalAdressingMode = horizontalAddrMode;
            VerticalAdressingMode = verticalAddrMode;
            HasMipMaps = hasMipMaps;
        }

        internal RWTextureReferenceStruct(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            _flags = reader.ReadUInt32();
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(_flags);
        }
    }
}