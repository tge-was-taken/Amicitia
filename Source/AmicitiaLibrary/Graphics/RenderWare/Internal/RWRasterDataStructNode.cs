using System.Linq;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System;
    using System.Drawing;
    using System.IO;
    using PS2.Graphics;
    using AmicitiaLibrary.Utilities;

    /// <summary>
    /// Holds the image data for PS2 RenderWare textures.
    /// </summary>
    internal class RwRasterDataStructNode : RwNode
    {
        private byte[] mMipMapData;
        private PS2StandardImageHeader mImageHeader;

        // indexed format fields
        private byte[] mIndices;
        private PS2StandardImageHeader mPaletteHeader;
        private Color[] mPalette;

        // non-indexed format fields
        private Color[] mPixels;

        /// <summary>
        /// Gets the <see cref="RwRasterInfoStructNode"/> corresponding to this <see cref="RwRasterDataStructNode"/>.
        /// </summary>
        internal RwRasterInfoStructNode RasterInfoStructNode
        {
            get { return ((RwRasterStructNode)Parent).InfoStructNode; }
        }

        /// <summary>
        /// Gets or sets the per-pixel palette color indices of the texture.
        /// </summary>
        public byte[] PixelIndices
        {
            get { return mIndices; }
            set { mIndices = value; }
        }

        /// <summary>
        /// Gets or sets the palette of the texture.
        /// </summary>
        public Color[] Palette
        {
            get { return mPalette; }
            set { mPalette = value; }
        }

        /// <summary>
        /// Gets or sets the pixels of the texture.
        /// </summary>
        public Color[] Pixels
        {
            get { return mPixels; }
            set { mPixels = value; }
        }

        /// <summary>
        /// Initialize a new instanc eof <see cref="RwRasterDataStructNode"/> using a given bitmap and a PS2 <see cref="PS2.Graphics.PS2PixelFormat"/> to encode the bitmap to.
        /// </summary>
        /// <param name="bitmap">Bitmap to be encoded using the given pixel format.</param>
        /// <param name="pixelFormat">The pixel format the bitmap will be encoded to and stored in the texture data.</param>
        public RwRasterDataStructNode(Bitmap bitmap, PS2PixelFormat pixelFormat)
            : base(RwNodeId.RwStructNode)
        {
            if (PS2PixelFormatHelper.IsIndexedPixelFormat(pixelFormat))
            {
                BitmapHelper.QuantizeBitmap(bitmap, PS2PixelFormatHelper.GetIndexedColorCount(pixelFormat), out mIndices, out mPalette);
            }
            else
            {
                mPixels = BitmapHelper.GetColors( bitmap )
                                      .Select( x => Color.FromArgb( PS2PixelFormatHelper.ScaleFullRangeAlphaToHalfRange( x.A ), x.R, x.G, x.B ) )
                                      .ToArray();
            }

            mMipMapData = new byte[0];
        }

        /// <summary>
        /// Initialize a new instance of <see cref="RwRasterDataStructNode"/> using given palette, pixel indices and pixel format. 
        /// </summary>
        /// <param name="palette">Palette of the texture.</param>
        /// <param name="indices">Per-Pixel indices into the palette colors of the texture.</param>
        /// <param name="pixelFormat">PS2 pixel format of the given texture data.</param>
        public RwRasterDataStructNode(Color[] palette, byte[] indices, PS2PixelFormat pixelFormat)
            : base(RwNodeId.RwStructNode)
        {
            mIndices = indices;
            mPalette = palette;
            mMipMapData = new byte[0];
        }

        /// <summary>
        /// Initializer only to be called by the <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RwRasterDataStructNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            long start = reader.BaseStream.Position;

            if (RasterInfoStructNode.Format.HasFlagUnchecked(RwRasterFormats.HasHeaders))
                mImageHeader = new PS2StandardImageHeader(reader);

            if (PS2PixelFormatHelper.IsIndexedPixelFormat(RasterInfoStructNode.Tex0Register.TexturePixelFormat))
            {
                ReadIndices(reader);

                if (RasterInfoStructNode.Format.HasFlagUnchecked(RwRasterFormats.HasHeaders))
                    mPaletteHeader = new PS2StandardImageHeader(reader);

                ReadPalette(reader);
            }
            else
            {
                ReadPixels(reader);
            }

            long end = reader.BaseStream.Position;

            mMipMapData = reader.ReadBytes((int)((start + Size) - end));
        }

        /// <summary>
        /// Read the pixel indices from the stream using given info, unswizzle it, and set the data as the pixel indices member value.
        /// </summary>
        private void ReadIndices(BinaryReader reader)
        {
            PS2PixelFormatHelper.ReadPixelData(RasterInfoStructNode.Tex0Register.TexturePixelFormat, reader, RasterInfoStructNode.Width, RasterInfoStructNode.Height, out mIndices);
            PS2PixelFormatHelper.UnSwizzle8(RasterInfoStructNode.Width, RasterInfoStructNode.Height, mIndices, out mIndices);
        }

        /// <summary>
        /// Make a copy of the pixel indices, swizzle it, and write it to the stream.
        /// </summary>
        private void WriteIndices(BinaryWriter writer)
        {
            byte[] outIndices;
            switch (RasterInfoStructNode.Tex0Register.TexturePixelFormat)
            {
                case PS2PixelFormat.PSMT8H:
                case PS2PixelFormat.PSMT8:
                    PS2PixelFormatHelper.Swizzle8(RasterInfoStructNode.Width, RasterInfoStructNode.Height, mIndices, out outIndices);
                    PS2PixelFormatHelper.WritePSMT8(writer, RasterInfoStructNode.Width, RasterInfoStructNode.Height, outIndices);
                    break;
                case PS2PixelFormat.PSMT4HL:
                case PS2PixelFormat.PSMT4HH:
                case PS2PixelFormat.PSMT4:
                    PS2PixelFormatHelper.Swizzle8(RasterInfoStructNode.Width, RasterInfoStructNode.Height, mIndices, out outIndices);
                    PS2PixelFormatHelper.WritePSMT4(writer, RasterInfoStructNode.Width, RasterInfoStructNode.Height, outIndices);
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
            PS2PixelFormatHelper.ReadPixelData(RasterInfoStructNode.Tex0Register.TexturePixelFormat, reader, RasterInfoStructNode.Width, RasterInfoStructNode.Height, out mPixels);
        }

        private void WritePixels(BinaryWriter writer)
        {
            PS2PixelFormatHelper.WritePixelData(RasterInfoStructNode.Tex0Register.TexturePixelFormat, writer, RasterInfoStructNode.Width, RasterInfoStructNode.Height, mPixels);
        }

        /// <summary>
        /// Read the palette from the stream using given pixel format, untile it, and set the data as the palette member value.
        /// </summary>
        private void ReadPalette(BinaryReader reader)
        {
            int paletteWH = PS2PixelFormatHelper.GetPaletteDimension(RasterInfoStructNode.Tex0Register.TexturePixelFormat);

            PS2PixelFormatHelper.ReadPixelData(RasterInfoStructNode.Tex0Register.ClutPixelFormat, reader, paletteWH, paletteWH, out mPalette);

            if (paletteWH == 16)
            {
                PS2PixelFormatHelper.TilePalette(mPalette, out mPalette);
            }
        }
        
        /// <summary>
        /// Make a copy of the palette, tile it, and write the palette to the stream using set pixel format.
        /// </summary>
        private void WritePalette(BinaryWriter writer)
        {
            int paletteWH = PS2PixelFormatHelper.GetPaletteDimension(RasterInfoStructNode.Tex0Register.TexturePixelFormat);

            Color[] outPalette;

            if (paletteWH == 16)
            {
                PS2PixelFormatHelper.TilePalette(mPalette, out outPalette);
            }
            else
            {
                outPalette = mPalette;
            }

            switch (RasterInfoStructNode.Tex0Register.ClutPixelFormat)
            {
                case PS2PixelFormat.PSMZ32:
                case PS2PixelFormat.PSMTC32:
                    PS2PixelFormatHelper.WritePSMCT32(writer, paletteWH, paletteWH, outPalette);
                    break;
                case PS2PixelFormat.PSMZ24:
                case PS2PixelFormat.PSMTC24:
                    PS2PixelFormatHelper.WritePSMCT24(writer, paletteWH, paletteWH, outPalette);
                    break;
                case PS2PixelFormat.PSMZ16:
                case PS2PixelFormat.PSMTC16:
                    PS2PixelFormatHelper.WritePSMCT16(writer, paletteWH, paletteWH, outPalette);
                    break;
                case PS2PixelFormat.PSMZ16S:
                case PS2PixelFormat.PSMTC16S:
                    PS2PixelFormatHelper.WritePSMCT16S(writer, paletteWH, paletteWH, outPalette);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary> 
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            if (RasterInfoStructNode.Format.HasFlagUnchecked(RwRasterFormats.HasHeaders))
            {
                writer.Write(mImageHeader.GetBytes());
            }

            if (PS2PixelFormatHelper.IsIndexedPixelFormat(RasterInfoStructNode.Tex0Register.TexturePixelFormat))
            {
                WriteIndices(writer);

                if (RasterInfoStructNode.Format.HasFlagUnchecked(RwRasterFormats.HasHeaders))
                {
                    writer.Write(mPaletteHeader.GetBytes());
                }

                WritePalette(writer);
            }
            else
            {
                WritePixels(writer);
            }

            writer.Write(mMipMapData);
        }
    }
}