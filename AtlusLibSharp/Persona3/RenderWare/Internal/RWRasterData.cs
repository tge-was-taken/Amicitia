namespace AtlusLibSharp.Persona3.RenderWare
{
    using System;
    using System.Drawing;
    using System.IO;
    using PS2.Graphics;
    using Common.Utilities;

    /// <summary>
    /// Holds the image data for PS2 RenderWare textures.
    /// </summary>
    internal class RWRasterData : RWNode
    {
        private StandardImageHeader _imageHeader;
        private byte[] _indices;
        private StandardImageHeader _paletteHeader;
        private Color[] _palette;
        private byte[] _mipMapData;

        /// <summary>
        /// Gets the <see cref="RWRasterInfo"/> corresponding to this <see cref="RWRasterData"/>.
        /// </summary>
        internal RWRasterInfo RasterInfo
        {
            get { return ((RWRaster)Parent).Info; }
        }

        /// <summary>
        /// Gets or sets the per-pixel palette color indices of the texture.
        /// </summary>
        public byte[] PixelIndices
        {
            get { return _indices; }
            set { _indices = value; }
        }

        /// <summary>
        /// Gets or sets the palette of the texture.
        /// </summary>
        public Color[] Palette
        {
            get { return _palette; }
            set { _palette = value; }
        }

        /// <summary>
        /// Initialize a new instanc eof <see cref="RWRasterData"/> using a given bitmap and a PS2 <see cref="PS2.Graphics.PixelFormat"/> to encode the bitmap to.
        /// </summary>
        /// <param name="bitmap">Bitmap to be encoded using the given pixel format.</param>
        /// <param name="pixelFormat">The pixel format the bitmap will be encoded to and stored in the texture data.</param>
        public RWRasterData(Bitmap bitmap, PixelFormat pixelFormat)
            : base(RWType.Struct)
        {
            if (PixelFormatHelper.IsIndexedPixelFormat(pixelFormat))
            {
                BitmapHelper.QuantizeBitmap(bitmap, PixelFormatHelper.GetIndexedColorCount(pixelFormat), out _indices, out _palette);
            }
            else
            {
                throw new NotImplementedException();
            }

            _mipMapData = new byte[0];
        }

        /// <summary>
        /// Initialize a new instance of <see cref="RWRasterData"/> using given palette, pixel indices and pixel format. 
        /// </summary>
        /// <param name="palette">Palette of the texture.</param>
        /// <param name="indices">Per-Pixel indices into the palette colors of the texture.</param>
        /// <param name="pixelFormat">PS2 pixel format of the given texture data.</param>
        public RWRasterData(Color[] palette, byte[] indices, PixelFormat pixelFormat)
            : base(RWType.Struct)
        {
            _indices = indices;
            _palette = palette;
            _mipMapData = new byte[0];
        }

        /// <summary>
        /// Initializer only to be called by the <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RWRasterData(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            long start = reader.BaseStream.Position;

            if (RasterInfo.Format.HasFlagFast(RWRasterFormats.HasHeaders))
                _imageHeader = new StandardImageHeader(reader);

            ReadIndices(reader);

            if (RasterInfo.Format.HasFlagFast(RWRasterFormats.HasHeaders))
                _paletteHeader = new StandardImageHeader(reader);

            ReadPalette(reader);

            long end = reader.BaseStream.Position;

            _mipMapData = reader.ReadBytes((int)((start + Size) - end));
        }

        /// <summary>
        /// Read the pixel indices from the stream using given info, unswizzle it, and set the data as the pixel indices member value.
        /// </summary>
        private void ReadIndices(BinaryReader reader)
        {
            PixelFormatHelper.ReadPixelData(RasterInfo.Tex0Register.TexturePixelFormat, reader, RasterInfo.Width, RasterInfo.Height, out _indices);
            PixelFormatHelper.UnSwizzle8(RasterInfo.Width, RasterInfo.Height, _indices, out _indices);
        }

        /// <summary>
        /// Make a copy of the pixel indices, swizzle it, and write it to the stream.
        /// </summary>
        private void WriteIndices(BinaryWriter writer)
        {
            byte[] outIndices;
            switch (RasterInfo.Tex0Register.TexturePixelFormat)
            {
                case PixelFormat.PSMT8H:
                case PixelFormat.PSMT8:
                    PixelFormatHelper.Swizzle8(RasterInfo.Width, RasterInfo.Height, _indices, out outIndices);
                    PixelFormatHelper.WritePSMT8(writer, RasterInfo.Width, RasterInfo.Height, outIndices);
                    break;
                case PixelFormat.PSMT4HL:
                case PixelFormat.PSMT4HH:
                case PixelFormat.PSMT4:
                    PixelFormatHelper.Swizzle8(RasterInfo.Width, RasterInfo.Height, _indices, out outIndices);
                    PixelFormatHelper.WritePSMT4(writer, RasterInfo.Width, RasterInfo.Height, outIndices);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Read the palette from the stream using given pixel format, untile it, and set the data as the palette member value.
        /// </summary>
        private void ReadPalette(BinaryReader reader)
        {
            int paletteWH = PixelFormatHelper.GetPaletteDimension(RasterInfo.Tex0Register.TexturePixelFormat);

            PixelFormatHelper.ReadPixelData(RasterInfo.Tex0Register.CLUTPixelFormat, reader, paletteWH, paletteWH, out _palette);

            if (paletteWH == 16)
            {
                PixelFormatHelper.TilePalette(_palette, out _palette);
            }
        }
        
        /// <summary>
        /// Make a copy of the palette, tile it, and write the palette to the stream using set pixel format.
        /// </summary>
        private void WritePalette(BinaryWriter writer)
        {
            int paletteWH = PixelFormatHelper.GetPaletteDimension(RasterInfo.Tex0Register.TexturePixelFormat);

            Color[] outPalette;

            if (paletteWH == 16)
            {
                PixelFormatHelper.TilePalette(_palette, out outPalette);
            }
            else
            {
                outPalette = _palette;
            }

            switch (RasterInfo.Tex0Register.CLUTPixelFormat)
            {
                case PixelFormat.PSMZ32:
                case PixelFormat.PSMCT32:
                    PixelFormatHelper.WritePSMCT32(writer, paletteWH, paletteWH, outPalette);
                    break;
                case PixelFormat.PSMZ24:
                case PixelFormat.PSMCT24:
                    PixelFormatHelper.WritePSMCT24(writer, paletteWH, paletteWH, outPalette);
                    break;
                case PixelFormat.PSMZ16:
                case PixelFormat.PSMCT16:
                    PixelFormatHelper.WritePSMCT16(writer, paletteWH, paletteWH, outPalette);
                    break;
                case PixelFormat.PSMZ16S:
                case PixelFormat.PSMCT16S:
                    PixelFormatHelper.WritePSMCT16S(writer, paletteWH, paletteWH, outPalette);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary> 
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            if (RasterInfo.Format.HasFlagFast(RWRasterFormats.HasHeaders))
            {
                writer.Write(_imageHeader.GetBytes());
            }

            WriteIndices(writer);

            if (RasterInfo.Format.HasFlagFast(RWRasterFormats.HasHeaders))
            {
                writer.Write(_paletteHeader.GetBytes());
            }

            WritePalette(writer);

            writer.Write(_mipMapData);
        }
    }
}