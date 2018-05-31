namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.IO;
    using PS2.Graphics;
    using AmicitiaLibrary.Utilities;

    /// <summary>
    /// Class holding internal values for an <see cref="RwTextureReferenceNode"/> instance.
    /// </summary>
    internal class RwTextureReferenceStruct : RwNode
    {
        private uint mFlags;

        #region Properties

        public PS2FilterMode FilterMode
        {
            get { return (PS2FilterMode)BitHelper.GetBits(mFlags, 8, 0); }
            set { BitHelper.ClearAndSetBits(ref mFlags, 8, (uint)value, 0); }
        }

        public PS2AddressingMode HorizontalAdressingMode
        {
            get { return (PS2AddressingMode)BitHelper.GetBits(mFlags, 4, 8); }
            set { BitHelper.ClearAndSetBits(ref mFlags, 4, (uint)value, 8); }
        }

        public PS2AddressingMode VerticalAdressingMode
        {
            get { return (PS2AddressingMode)BitHelper.GetBits(mFlags, 4, 12); }
            set { BitHelper.ClearAndSetBits(ref mFlags, 4, (uint)value, 12); }
        }

        public bool HasMipMaps
        {
            get { return BitHelper.IsBitSet(mFlags, 16); }
            set
            {
                BitHelper.ClearBit(ref mFlags, 16);

                if (value)
                    BitHelper.SetBit(ref mFlags, 16);
            }
        }

        #endregion Properties

        public RwTextureReferenceStruct(RwNode parent = null)
            : base(RwNodeId.RwStructNode, parent)
        {
            FilterMode = PS2FilterMode.Linear;
            HorizontalAdressingMode = PS2AddressingMode.Wrap;
            VerticalAdressingMode = PS2AddressingMode.Wrap;
            HasMipMaps = false;
        }

        public RwTextureReferenceStruct(
            PS2FilterMode filterMode, 
            PS2AddressingMode horizontalAddrMode, PS2AddressingMode verticalAddrMode, 
            bool hasMipMaps,
            RwNode parent = null)
            : base(RwNodeId.RwStructNode, parent)
        {
            FilterMode = filterMode;
            HorizontalAdressingMode = horizontalAddrMode;
            VerticalAdressingMode = verticalAddrMode;
            HasMipMaps = hasMipMaps;
        }

        internal RwTextureReferenceStruct(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            mFlags = reader.ReadUInt32();
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write(mFlags);
        }
    }
}