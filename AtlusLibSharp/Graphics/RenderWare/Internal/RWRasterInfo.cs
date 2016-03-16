namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;
    using AtlusLibSharp.Utilities;
    using PS2.Graphics;
    using PS2.Graphics.Registers;

    /// <summary>
    /// Holds internal info about PS2 RenderWare textures.
    /// </summary>
    internal class RWRasterInfo : RWNode
    {
        private int _width;
        private int _height;
        private int _depth;
        private RWRasterFormats _rasterFormat;
        private Tex0Register _tex0;
        private Tex1Register _tex1;
        private MipTBPRegister _mip1;
        private MipTBPRegister _mip2;
        private uint _texelDataLength; // stream size of the mipmap data
        private uint _paletteDataLength; // stream size of the palette data (zero if no palette)
        private uint _gpuAlignedLength; // memory span of the texture mipmap data on the GS (aligned to pages/2048)

        /// <summary>
        /// Gets the pixel width of the texture.
        /// </summary>
        public int Width
        {
            get { return _width; }
        }

        /// <summary>
        /// Gets the pixel height of the texture.
        /// </summary>
        public int Height
        {
            get { return _height; }
        }

        /// <summary>
        /// Gets the pixel depth of the texture.
        /// </summary>
        public int Depth
        {
            get { return _depth; }
        }

        /// <summary>
        /// Gets the <see cref="RWRasterFormats"/> flags of the texture.
        /// </summary>
        public RWRasterFormats Format
        {
            get { return _rasterFormat; }
        }

        /// <summary>
        /// Gets the PS2 <see cref="PS2.Graphics.Registers.Tex0Register"/> values set when this texture is loaded.
        /// </summary>
        public Tex0Register Tex0Register
        {
            get { return _tex0; }
        }

        /// <summary>
        /// Gets the PS2 <see cref="PS2.Graphics.Registers.Tex1Register"/> values set when this texture is loaded.
        /// </summary>
        public Tex1Register Tex1Register
        {
            get { return _tex1; }
        }

        /// <summary>
        /// Gets the PS2 <see cref="PS2.Graphics.Registers.MipTBPRegister"/> values set for the MipTBP1Register when this texture is loaded.
        /// </summary>
        public MipTBPRegister MipTBP1Register
        {
            get { return _mip1; }
        }

        /// <summary>
        /// Gets the PS2 <see cref="PS2.Graphics.Registers.MipTBPRegister"/> values set for the MipTBP2Register when this texture is loaded.
        /// </summary>
        public MipTBPRegister MipTBP2Register
        {
            get { return _mip2; }
        }

        /// <summary>
        /// Initialize a new instance of <see cref="RWRasterInfo"/> with properties set using a given width, height and <see cref="PS2.Graphics.PS2PixelFormat"/>.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pixelFormat">PS2 pixel format of the given texture data.</param>
        public RWRasterInfo(int width, int height, PS2PixelFormat pixelFormat)
            : base(RWNodeType.Struct)
        {
            CreateRasterInfo(width, height, pixelFormat);
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RWRasterInfo(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            _width = reader.ReadInt32();
            _height = reader.ReadInt32();
            _depth = reader.ReadInt32();
            _rasterFormat = (RWRasterFormats)reader.ReadUInt32();
            _tex0 = new Tex0Register(reader);
            _tex1 = new Tex1Register(reader);
            _mip1 = new MipTBPRegister(reader);
            _mip2 = new MipTBPRegister(reader);
            _texelDataLength = reader.ReadUInt32();
            _paletteDataLength = reader.ReadUInt32();
            _gpuAlignedLength = reader.ReadUInt32();
            uint skyMipMapValue = reader.ReadUInt32();
        }

        /// <summary>
        /// Sets the appropriate raster info values based on the width, height and pixel format of the texture.
        /// </summary>
        /// <param name="width">Width of the texture.</param>
        /// <param name="height">Height of the texture.</param>
        /// <param name="pixelFormat">PS2 pixel format of the given texture data.</param>
        private void CreateRasterInfo(int width, int height, PS2PixelFormat pixelFormat)
        {
            _width = width;
            _height = height;
            _depth = PS2PixelFormatHelper.GetPixelFormatDepth(pixelFormat);
            _rasterFormat = GetRasterFormatFromDepth(_depth);

            _tex0 = new Tex0Register(pixelFormat, width, height);
            _tex1 = new Tex1Register(); // default settings

            int texSize = width * height;
            if (texSize < 16384) // because yes
            {
                _tex1.MaxMipLevel = 7;
                _tex1.MipMinFilter = PS2FilterMode.None;

                if (texSize <= 4096)
                {
                    _tex1.MipMaxFilter = PS2FilterMode.None;
                }
                else
                {
                    _tex1.MipMaxFilter = PS2FilterMode.Nearest;
                }
            }

            _mip1 = new MipTBPRegister(); // default settings
            _mip2 = new MipTBPRegister(); // default settings

            _texelDataLength = (uint)PS2PixelFormatHelper.GetTexelDataSize(pixelFormat, width, height);

            // Add image header size to the length if there is one
            if (_rasterFormat.HasFlagUnchecked(RWRasterFormats.HasHeaders))
                _texelDataLength += PS2StandardImageHeader.Size;

            _paletteDataLength = 0; // set to 0 if there's no palette present

            // division by 2 might only apply for 8 bit..
            int transferWidth = width / 2;
            int transferHeight = height / 2;
            _gpuAlignedLength = (uint)(transferWidth * transferHeight);

            // Calculate the palette data length for indexed textures
            if (_depth == 4 || _depth == 8)
            {
                int numColors = 256;

                if (_depth == 4)
                {
                    numColors = 16;
                }

                _paletteDataLength = (uint)(numColors * 4);

                // division by 4 might only apply for 8 bit..
                _gpuAlignedLength += _paletteDataLength / 4;

                // Add image header size to the length if there is one
                if (_rasterFormat.HasFlagUnchecked(RWRasterFormats.HasHeaders))
                    _paletteDataLength += PS2StandardImageHeader.Size;
            }

            // align to pages of 2048
            _gpuAlignedLength = (uint)AlignmentHelper.Align(_gpuAlignedLength, 2048);
        }

        /// <summary>
        /// Sets the appropriate raster format flags based on the depth of the texture
        /// </summary>
        /// <param name="depth">The depth of the texture.</param>
        /// <returns>The raster format flags adjusted to the depth of the texture.</returns>
        private static RWRasterFormats GetRasterFormatFromDepth(int depth)
        {
            RWRasterFormats rasterFormat = RWRasterFormats.Format8888 | RWRasterFormats.Unknown;

            switch (depth)
            {
                case 8:
                    rasterFormat |= RWRasterFormats.Pal8;
                    rasterFormat |= RWRasterFormats.Swizzled;
                    break;

                case 4:
                    rasterFormat |= RWRasterFormats.Pal4;
                    rasterFormat |= RWRasterFormats.Swizzled;
                    break;
            }

            return rasterFormat;
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(_width);
            writer.Write(_height);
            writer.Write(_depth);
            writer.Write((uint)_rasterFormat);
            writer.Write(_tex0.GetBytes());
            writer.Write(_tex1.GetBytes());
            writer.Write(_mip1.GetBytes());
            writer.Write(_mip2.GetBytes());
            writer.Write(_texelDataLength);
            writer.Write(_paletteDataLength);
            writer.Write(_gpuAlignedLength);
            writer.Write(RWSkyMipMapValue.SKY_MIPMAP_VALUE);
        }
    }
}