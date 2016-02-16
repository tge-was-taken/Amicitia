namespace AtlusLibSharp.Persona3.RenderWare
{
    using System.IO;
    using PS2.Graphics;
    using Common.Utilities;

    /// <summary>
    /// Class holding internal values for an <see cref="RWTextureReference"/> instance.
    /// </summary>
    internal class RWTextureReferenceStruct : RWNode
    {
        private uint _flags;

        #region Properties

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
            : base(RWType.Struct, parent)
        {
            FilterMode = FilterMode.Linear;
            HorizontalAdressingMode = AddressingMode.Wrap;
            VerticalAdressingMode = AddressingMode.Wrap;
            HasMipMaps = false;
        }

        public RWTextureReferenceStruct(
            FilterMode filterMode, 
            AddressingMode horizontalAddrMode, AddressingMode verticalAddrMode, 
            bool hasMipMaps,
            RWNode parent = null)
            : base(RWType.Struct, parent)
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