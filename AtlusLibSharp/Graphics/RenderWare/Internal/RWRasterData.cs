namespace AtlusLibSharp.Graphics.RenderWare
{
    using System;
    using System.Drawing;
    using System.IO;
    using PS2.Graphics;
    using AtlusLibSharp.Utilities;

    /// <summary>
    /// Holds the image data for PS2 RenderWare textures.
    /// </summary>
    internal class RWRasterData : RWNode
    {
        private byte[] _mipMapData;
        private PS2StandardImageHeader _imageHeader;

        // indexed format fields
        private byte[] _indices;
        private PS2StandardImageHeader _paletteHeader;
        private Color[] _palette;

        // non-indexed format fields
        private Color[] _pixels;

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
        /// Gets or sets the pixels of the texture.
        /// </summary>
        public Color[] Pixels
        {
            get { return _pixels; }
            set { _pixels = value; }
        }

        /// <summary>
        /// Initialize a new instanc eof <see cref="RWRasterData"/> using a given bitmap and a PS2 <see cref="PS2.Graphics.PS2PixelFormat"/> to encode the bitmap to.
        /// </summary>
        /// <param name="bitmap">Bitmap to be encoded using the given pixel format.</param>
        /// <param name="pixelFormat">The pixel format the bitmap will be encoded to and stored in the texture data.</param>
        public RWRasterData(Bitmap bitmap, PS2PixelFormat pixelFormat)
            : base(RWType.Struct)
        {
            if (PS2PixelFormatHelper.IsIndexedPixelFormat(pixelFormat))
            {
                BitmapHelper.QuantizeBitmap(bitmap, PS2PixelFormatHelper.GetIndexedColorCount(pixelFormat), out _indices, out _palette);
            }
            else
            {
                _pixels = BitmapHelper.GetColors(bitmap);
            }

            _mipMapData = new byte[0];
        }

        /// <summary>
        /// Initialize a new instance of <see cref="RWRasterData"/> using given palette, pixel indices and pixel format. 
        /// </summary>
        /// <param name="palette">Palette of the texture.</param>
        /// <param name="indices">Per-Pixel indices into the palette colors of the texture.</param>
        /// <param name="pixelFormat">PS2 pixel format of the given texture data.</param>
        public RWRasterData(Color[] palette, byte[] indices, PS2PixelFormat pixelFormat)
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

            if (RasterInfo.Format.HasFlagUnchecked(RWRasterFormats.HasHeaders))
                _imageHeader = new PS2StandardImageHeader(reader);

            if (PS2PixelFormatHelper.IsIndexedPixelFormat(RasterInfo.Tex0Register.TexturePixelFormat))
            {
                ReadIndices(reader);

                if (RasterInfo.Format.HasFlagUnchecked(RWRasterFormats.HasHeaders))
                    _paletteHeader = new PS2StandardImageHeader(reader);

                ReadPalette(reader);
            }
            else
            {
                ReadPixels(reader);
            }

            long end = reader.BaseStream.Position;

            _mipMapData = reader.ReadBytes((int)((start + Size) - end));
        }

        /// <summary>
        /// Read the pixel indices from the stream using given info, unswizzle it, and set the data as the pixel indices member value.
        /// </summary>
        private void ReadIndices(BinaryReader reader)
        {
            PS2PixelFormatHelper.ReadPixelData(RasterInfo.Tex0Register.TexturePixelFormat, reader, RasterInfo.Width, RasterInfo.Height, out _indices);
            PS2PixelFormatHelper.UnSwizzle8(RasterInfo.Width, RasterInfo.Height, _indices, out _indices);
        }

        /// <summary>
        /// Make a copy of the pixel indices, swizzle it, and write it to the stream.
        /// </summary>
        private void WriteIndices(BinaryWriter writer)
        {
            byte[] outIndices;
            switch (RasterInfo.Tex0Register.TexturePixelFormat)
            {
                case PS2PixelFormat.PSMT8H:
                case PS2PixelFormat.PSMT8:
                    PS2PixelFormatHelper.Swizzle8(RasterInfo.Width, RasterInfo.Height, _indices, out outIndices);
                    PS2PixelFormatHelper.WritePSMT8(writer, RasterInfo.Width, RasterInfo.Height, outIndices);
                    break;
                case PS2PixelFormat.PSMT4HL:
                case PS2PixelFormat.PSMT4HH:
                case PS2PixelFormat.PSMT4:
                    PS2PixelFormatHelper.Swizzle8(RasterInfo.Width, RasterInfo.Height, _indices, out outIndices);
                    PS2PixelFormatHelper.WritePSMT4(writer, RasterInfo.Width, RasterInfo.Height, outIndices);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Read the pixel data from the stream using given info.
        /// </summary>
        /// <param name="reader"></param>
        private void ReadPixels(BinaryReader reader)
        {
            PS2PixelFormatHelper.ReadPixelData(RasterInfo.Tex0Register.TexturePixelFormat, reader, RasterInfo.Width, RasterInfo.Height, out _pixels);
        }

        private void WritePixels(BinaryWriter writer)
        {
            PS2PixelFormatHelper.WritePixelData(RasterInfo.Tex0Register.TexturePixelFormat, writer, RasterInfo.Width, RasterInfo.Height, _pixels);
        }

        /// <summary>
        /// Read the palette from the stream using given pixel format, untile it, and set the data as the palette member value.
        /// </summary>
        private void ReadPalette(BinaryReader reader)
        {
            int paletteWH = PS2PixelFormatHelper.GetPaletteDimension(RasterInfo.Tex0Register.TexturePixelFormat);

            PS2PixelFormatHelper.ReadPixelData(RasterInfo.Tex0Register.CLUTPixelFormat, reader, paletteWH, paletteWH, out _palette);

            if (paletteWH == 16)
            {
                PS2PixelFormatHelper.TilePalette(_palette, out _palette);
            }
        }
        
        /// <summary>
        /// Make a copy of the palette, tile it, and write the palette to the stream using set pixel format.
        /// </summary>
        private void WritePalette(BinaryWriter writer)
        {
            int paletteWH = PS2PixelFormatHelper.GetPaletteDimension(RasterInfo.Tex0Register.TexturePixelFormat);

            Color[] outPalette;

            if (paletteWH == 16)
            {
                PS2PixelFormatHelper.TilePalette(_palette, out outPalette);
            }
            else
            {
                outPalette = _palette;
            }

            switch (RasterInfo.Tex0Register.CLUTPixelFormat)
            {
                case PS2PixelFormat.PSMZ32:
                case PS2PixelFormat.PSMCT32:
                    PS2PixelFormatHelper.WritePSMCT32(writer, paletteWH, paletteWH, outPalette);
                    break;
                case PS2PixelFormat.PSMZ24:
                case PS2PixelFormat.PSMCT24:
                    PS2PixelFormatHelper.WritePSMCT24(writer, paletteWH, paletteWH, outPalette);
                    break;
                case PS2PixelFormat.PSMZ16:
                case PS2PixelFormat.PSMCT16:
                    PS2PixelFormatHelper.WritePSMCT16(writer, paletteWH, paletteWH, outPalette);
                    break;
                case PS2PixelFormat.PSMZ16S:
                case PS2PixelFormat.PSMCT16S:
                    PS2PixelFormatHelper.WritePSMCT16S(writer, paletteWH, paletteWH, outPalette);
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
            if (RasterInfo.Format.HasFlagUnchecked(RWRasterFormats.HasHeaders))
            {
                writer.Write(_imageHeader.GetBytes());
            }

            if (PS2PixelFormatHelper.IsIndexedPixelFormat(RasterInfo.Tex0Register.TexturePixelFormat))
            {
                WriteIndices(writer);

                if (RasterInfo.Format.HasFlagUnchecked(RWRasterFormats.HasHeaders))
                {
                    writer.Write(_paletteHeader.GetBytes());
                }

                WritePalette(writer);
            }
            else
            {
                WritePixels(writer);
            }

            writer.Write(_mipMapData);
        }
    }
}