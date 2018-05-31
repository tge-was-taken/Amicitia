using AmicitiaLibrary.Utilities;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.IO;
    using PS2.Graphics;
    using AmicitiaLibrary.Utilities;

    /// <summary>
    /// Stores internal values for an <see cref="RwTextureNativeNode"/> instance.
    /// </summary>
    internal class RwTextureNativeStructNode : RwNode
    {
        // Fields
        private uint mFlags;

        public RwPlatformId PlatformId { get; }

        public PS2FilterMode FilterMode
        {
            get { return (PS2FilterMode)BitHelper.GetBits(mFlags, 8, 0); }
            set { BitHelper.ClearAndSetBits(ref mFlags, 8, (uint)value, 0); }
        }

        public PS2AddressingMode HorizontalAddressingMode
        {
            get { return (PS2AddressingMode)BitHelper.GetBits(mFlags, 4, 8); }
            set { BitHelper.ClearAndSetBits(ref mFlags, 4, (uint)value, 8); }
        }

        public PS2AddressingMode VerticalAddressingMode
        {
            get { return (PS2AddressingMode)BitHelper.GetBits(mFlags, 4, 12); }
            set { BitHelper.ClearAndSetBits(ref mFlags, 4, (uint)value, 12); }
        }

        // Constructors
        public RwTextureNativeStructNode(RwNode parent = null)
            : base(RwNodeId.RwStructNode, parent)
        {
            PlatformId = RwPlatformId.PS2;
            FilterMode = PS2FilterMode.Linear;
            HorizontalAddressingMode = PS2AddressingMode.Wrap;
            VerticalAddressingMode = PS2AddressingMode.Wrap;
        }

        public RwTextureNativeStructNode(
            RwPlatformId rwPlatformId, PS2FilterMode filterMode, 
            PS2AddressingMode horizontalAddrMode, PS2AddressingMode verticalAddrMode,
            RwNode parent = null)
            : base(RwNodeId.RwStructNode, parent)
        {
            PlatformId = rwPlatformId;
            FilterMode = filterMode;
            HorizontalAddressingMode = horizontalAddrMode;
            VerticalAddressingMode = verticalAddrMode;
        }

        internal RwTextureNativeStructNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            PlatformId = (RwPlatformId)reader.ReadUInt32();
            mFlags = reader.ReadUInt32();
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write((uint)PlatformId);
            writer.Write(mFlags);
        }
    }
}