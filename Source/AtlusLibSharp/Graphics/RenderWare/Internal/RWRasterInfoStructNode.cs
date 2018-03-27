namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;
    using AtlusLibSharp.Utilities;
    using PS2.Graphics;
    using PS2.Graphics.Registers;

    /// <summary>
    /// Holds internal info about PS2 RenderWare textures.
    /// </summary>
    internal class RwRasterInfoStructNode : RwNode
    {
        /// <summary>
        /// Gets the pixel width of the texture.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the pixel height of the texture.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the pixel depth of the texture.
        /// </summary>
        public int Depth { get; private set; }

        /// <summary>
        /// Gets the <see cref="RwRasterFormats"/> flags of the texture.
        /// </summary>
        public RwRasterFormats Format { get; private set; }

        /// <summary>
        /// Gets the PS2 <see cref="PS2.Graphics.Registers.Tex0Register"/> values set when this texture is loaded.
        /// </summary>
        public Tex0Register Tex0Register { get; private set; }

        /// <summary>
        /// Gets the PS2 <see cref="PS2.Graphics.Registers.Tex1Register"/> values set when this texture is loaded.
        /// </summary>
        public Tex1Register Tex1Register { get; private set; }

        /// <summary>
        /// Gets the PS2 <see cref="MipTbpRegister"/> values set for the MipTBP1Register when this texture is loaded.
        /// </summary>
        public MipTbpRegister MipTBP1Register { get; private set; }

        /// <summary>
        /// Gets the PS2 <see cref="MipTbpRegister"/> values set for the MipTBP2Register when this texture is loaded.
        /// </summary>
        public MipTbpRegister MipTBP2Register { get; private set; }

        public uint TexelDataLength { get; private set; }

        public uint PaletteDataLength { get; private set; }

        public uint GpuAlignedLength { get; private set; }

        /// <summary>
        /// Initialize a new instance of <see cref="RwRasterInfoStructNode"/> with properties set using a given width, height and <see cref="PS2.Graphics.PS2PixelFormat"/>.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pixelFormat">PS2 pixel format of the given texture data.</param>
        public RwRasterInfoStructNode(int width, int height, PS2PixelFormat pixelFormat)
            : base(RwNodeId.RwStructNode)
        {
            CreateRasterInfo(width, height, pixelFormat);
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RwRasterInfoStructNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            Depth = reader.ReadInt32();
            Format = (RwRasterFormats)reader.ReadUInt32();
            Tex0Register = new Tex0Register(reader);
            Tex1Register = new Tex1Register(reader);
            MipTBP1Register = new MipTbpRegister(reader);
            MipTBP2Register = new MipTbpRegister(reader);
            TexelDataLength = reader.ReadUInt32();
            PaletteDataLength = reader.ReadUInt32();
            GpuAlignedLength = reader.ReadUInt32();
            uint skyMipMapValue = reader.ReadUInt32();
        }

        /// <summary>
        /// Sets the appropriate rasterStructNode info values based on the width, height and pixel format of the texture.
        /// </summary>
        /// <param name="width">Width of the texture.</param>
        /// <param name="height">Height of the texture.</param>
        /// <param name="pixelFormat">PS2 pixel format of the given texture data.</param>
        private void CreateRasterInfo(int width, int height, PS2PixelFormat pixelFormat)
        {
            Width = width;
            Height = height;
            Depth = PS2PixelFormatHelper.GetPixelFormatDepth(pixelFormat);
            Format = GetRasterFormatFromDepth(Depth);

            Tex0Register = new Tex0Register(pixelFormat, width, height);
            Tex1Register = new Tex1Register(); // default settings

            int texSize = width * height;
            if (texSize < 16384) // because yes
            {
                Tex1Register.MaxMipLevel = 7;
                Tex1Register.MipMinFilter = PS2FilterMode.None;

                if (texSize <= 4096)
                {
                    Tex1Register.MipMaxFilter = PS2FilterMode.None;
                }
                else
                {
                    Tex1Register.MipMaxFilter = PS2FilterMode.Nearest;
                }
            }

            MipTBP1Register = new MipTbpRegister(); // default settings
            MipTBP2Register = new MipTbpRegister(); // default settings

            TexelDataLength = (uint)PS2PixelFormatHelper.GetTexelDataSize(pixelFormat, width, height);

            // Add image header size to the length if there is one
            if (Format.HasFlagUnchecked(RwRasterFormats.HasHeaders))
                TexelDataLength += PS2StandardImageHeader.Size;

            PaletteDataLength = 0; // set to 0 if there's no palette present

            // division by 2 might only apply for 8 bit..
            int transferWidth = width / 2;
            int transferHeight = height / 2;
            GpuAlignedLength = (uint)(transferWidth * transferHeight);

            // Calculate the palette data length for indexed textures
            if (Depth == 4 || Depth == 8)
            {
                int numColors = 256;

                if (Depth == 4)
                {
                    numColors = 16;
                }

                PaletteDataLength = (uint)(numColors * 4);

                // division by 4 might only apply for 8 bit..
                GpuAlignedLength += PaletteDataLength / 4;

                // Add image header size to the length if there is one
                if (Format.HasFlagUnchecked(RwRasterFormats.HasHeaders))
                    PaletteDataLength += PS2StandardImageHeader.Size;
            }

            // align to pages of 2048
            GpuAlignedLength = (uint)AlignmentHelper.Align(GpuAlignedLength, 2048);
        }

        /// <summary>
        /// Sets the appropriate rasterStructNode format flags based on the depth of the texture
        /// </summary>
        /// <param name="depth">The depth of the texture.</param>
        /// <returns>The rasterStructNode format flags adjusted to the depth of the texture.</returns>
        private static RwRasterFormats GetRasterFormatFromDepth(int depth)
        {
            RwRasterFormats rasterFormat = RwRasterFormats.Format8888 | RwRasterFormats.Unknown;

            switch (depth)
            {
                case 8:
                    rasterFormat |= RwRasterFormats.Pal8;
                    rasterFormat |= RwRasterFormats.Swizzled;
                    break;

                case 4:
                    rasterFormat |= RwRasterFormats.Pal4;
                    rasterFormat |= RwRasterFormats.Swizzled;
                    break;
            }

            return rasterFormat;
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write(Width);
            writer.Write(Height);
            writer.Write(Depth);
            writer.Write((uint)Format);
            writer.Write(Tex0Register.GetBytes());
            writer.Write(Tex1Register.GetBytes());
            writer.Write(MipTBP1Register.GetBytes());
            writer.Write(MipTBP2Register.GetBytes());
            writer.Write(TexelDataLength);
            writer.Write(PaletteDataLength);
            writer.Write(GpuAlignedLength);
            writer.Write(RwSkyMipMapValueNode.SKY_MIPMAP_VALUE);
        }
    }
}