namespace AtlusLibSharp.Graphics.TGA
{
    using System.IO;
    using System.Drawing;
    using IO;
    using Utilities;
    using System;


    // TODO: refactor methods to remove some of the copy pasta going on in the pixel/index reading and writing code
    // also implement de/encoding (low priority)

    /// <summary>
    /// Encapsulates a TrueVision TARGA image file.
    /// </summary>
    public class TgaFile : BinaryBase, ITextureFile
    {
        /****************************/
        /* TGA header struct fields */
        /****************************/
        private byte mIdLength;
        private byte mColorMapType;
        private TgaEncoding mDataType;
        private short mPaletteStartIndex;
        private short mPaletteLength;
        private byte mPaletteDepth;
        private short mXOrigin;
        private short mYOrigin;
        private short mWidth;
        private short mHeight;
        private byte mBpp;
        private byte mImageDescriptor;

        // palette and palette indices are used for indexed formats, otherwise they have value null
        private Color[] mPalette;
        private byte[] mPaletteIndices;

        // pixels is used for non-indexed formats, otherwise null
        private Color[] mPixels;

        // last created bitmap backing store
        private Bitmap mBitmap;

        // pixel de/encoder delegate
        private delegate byte[] TgaDataDecoder(byte[] encoded);
        private delegate byte[] TgaDataEncoder(byte[] unencoded);

        /// <summary>
        /// Gets the pixel data encoding id of the image data.
        /// </summary>
        public TgaEncoding Encoding
        {
            get { return mDataType; }
        }

        /// <summary>
        /// Gets a <see cref="System.Boolean"/> value indicating if this TGA uses a color palette together with per-pixel palette indices.
        /// </summary>
        public bool IsIndexed
        {
            get { return mColorMapType == 1; }
        }

        /// <summary>
        /// Gets the color depth of the palette. Returns 0 if the image is indexed.
        /// </summary>
        public int PaletteDepth
        {
            get { return mPaletteDepth; }
        }

        /// <summary>
        /// Gets the width of the image.
        /// </summary>
        public int Width
        {
            get { return mWidth; }
        }

        /// <summary>
        /// Gets the height of the image.
        /// </summary>
        public int Height
        {
            get { return mHeight; }
        }

        /// <summary>
        /// Gets the number of bits per pixel of the image.
        /// </summary>
        public int BitsPerPixel
        {
            get { return mBpp; }
        }

        /// <summary>
        /// Gets the color palette of the image. Returns null if the image is not indexed.
        /// </summary>
        public Color[] Palette
        {
            get { return mPalette; }
        }

        /// <summary>
        /// Gets the per-pixel palette color indices of the image. Returns null if the image is not indexed.
        /// </summary>
        public byte[] PixelIndices
        {
            get { return mPaletteIndices; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TgaFile"/> class from the specified file.
        /// </summary>
        /// <param name="filename">The tga file name and path.</param>
        public TgaFile(string filename)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filename)))
                Read(reader);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TgaFile"/> class from the specified stream.
        /// </summary>
        /// <param name="stream">The data stream used to load the image.</param>
        /// <param name="leaveStreamOpen">Specifies if the stream shouldn't be disposed after use.</param>
        public TgaFile(Stream stream, bool leaveStreamOpen = false)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveStreamOpen))
                Read(reader);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TgaFile"/> class from the specified bitmap file path, and encoding parameters to encode the bitmap with.
        /// </summary>
        /// <param name="filename">The bitmap file path used for encoding.</param>
        /// <param name="encodingType">Specifies the pixel data encoding to use.</param>
        /// <param name="bitsPerPixel">Specifies the amount of bits per pixel. 
        /// 4 and 8 are reserved for indexed encodings, while 16, 24 and 32 are used for non-indexed images.</param>
        /// <param name="paletteDepth">Specifies the amount of bits per color in the palette. Ignored if the encoding id is of an indexed encoding.</param>
        public TgaFile(string filename, TgaEncoding encodingType = TgaEncoding.Indexed, int bitsPerPixel = 8, int paletteDepth = 32)
            : this(new Bitmap(filename), encodingType, bitsPerPixel, paletteDepth)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TgaFile"/> class from the specified <see cref="Bitmap"/> class, and encoding parameters to encode the bitmap with.
        /// </summary>
        /// <param name="bitmap">The bitmap instance used for encoding.</param>
        /// <param name="encodingType">Specifies the pixel data encoding to use.</param>
        /// <param name="bitsPerPixel">Specifies the amount of bits per pixel. 
        /// 4 and 8 are reserved for indexed encodings, while 16, 24 and 32 are used for non-indexed images.</param>
        /// <param name="paletteDepth">Specifies the amount of bits per color in the palette. Ignored if the encoding id is of an indexed encoding.</param>
        public TgaFile(Bitmap bitmap, TgaEncoding encodingType = TgaEncoding.Indexed, int bitsPerPixel = 8, int paletteDepth = 32)
        {
            // set header
            SetHeaderInfo(bitmap.Width, bitmap.Height, bitsPerPixel, paletteDepth, encodingType);

            // done if the encoding id is set to none
            if (encodingType == TgaEncoding.None)
                return;

            // set bitmap data
            if (IsIndexed)
            {
                BitmapHelper.QuantizeBitmap(bitmap, 1 << bitsPerPixel, out mPaletteIndices, out mPalette);
            }
            else
            {
                mPixels = BitmapHelper.GetColors(bitmap);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TgaFile"/> class from specified dimensions, color palette, pixel indices and encoding parameters that suit the provided image data.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="palette">The color palette of the image</param>
        /// <param name="pixelIndices">The per-pixel palette color indices of the image.</param>
        /// <param name="encodingType">Specifies the pixel data encoding to use.</param>
        /// <param name="bitsPerPixel">Specifies the amount of bits per pixel. 
        /// 4 and 8 are reserved for indexed encodings, while 16, 24 and 32 are used for non-indexed images.</param>
        /// <param name="paletteDepth">Specifies the amount of bits per color in the palette. Ignored if the encoding id is of an indexed encoding.</param>
        public TgaFile(int width, int height, Color[] palette, byte[] pixelIndices, 
            TgaEncoding encodingType = TgaEncoding.Indexed, int bitsPerPixel = 8, int paletteDepth = 32)
        {
            // set header
            SetHeaderInfo(width, height, bitsPerPixel, paletteDepth, encodingType);

            // done if the encoding id is set to none
            if (encodingType == TgaEncoding.None)
                return;

            /*
            // check if input data is correct
            if (_palette.Length != _paletteLength || (width * height * (float)(bitsPerPixel / 8)) != pixelIndices.Length)
            {
                throw new ArgumentException("Palette or pixel index data was not in the expected format");
            }
            */

            
            // should probably throw a format exception somewhere as this is pretty insane ;p
            if (IsIndexed)
            {
                if (palette.Length != (1 << bitsPerPixel))
                {
                    BitmapHelper.QuantizeBitmap(BitmapHelper.Create(palette, pixelIndices, width, height), 1 << bitsPerPixel, out mPaletteIndices, out mPalette);
                }
                else
                {
                    // set palette and pixel data
                    mPalette = palette;
                    mPaletteIndices = pixelIndices;
                }
            }
            else
            {
                mPixels = BitmapHelper.GetColors(BitmapHelper.Create(palette, pixelIndices, width, height));
            }
        }

        /// <summary>
        /// <para>Construct a bitmap from the data in this <see cref="TgaFile"/> instance.</para>
        /// <para>Subsequent calls to this method will return the same bitmap instance without constructing a new one.</para>
        /// </summary>
        public Bitmap GetBitmap()
        {
            // Check if the bitmap hasn't been created already
            if (mBitmap == null || (mBitmap.Width != mWidth && mBitmap.Height != mHeight))
            {
                CreateBitmap();
            }         

            return mBitmap;
        }


        public Color[] GetPixels()
        {
            if (IsIndexed && mPixels == null)
            {
                mPixels = new Color[mWidth * mHeight];
                for (int y = 0; y < mHeight; y++)
                    for (int x = 0; x < mWidth; x++)
                        mPixels[x + y * mWidth] = mPalette[mPaletteIndices[x + y * mWidth]];
            }

            return mPixels;
        }

        internal void Read(BinaryReader reader)
        {
            mIdLength = reader.ReadByte();
            mColorMapType = reader.ReadByte();
            mDataType = (TgaEncoding)reader.ReadByte();
            mPaletteStartIndex = reader.ReadInt16();
            mPaletteLength = reader.ReadInt16();
            mPaletteDepth = reader.ReadByte();
            mXOrigin = reader.ReadInt16();
            mYOrigin = reader.ReadInt16();
            mWidth = reader.ReadInt16();
            mHeight = reader.ReadInt16();
            mBpp = reader.ReadByte();
            mImageDescriptor = reader.ReadByte();

            // format check
            if (EncodingUsesRleOrCmp(mDataType) || mDataType == TgaEncoding.Grayscale)
            {
                throw new NotImplementedException($"DataStructNode id not supported: {mDataType}");
            }
            else if (mDataType == TgaEncoding.None)
            {
                return;
            }

            // skip user data
            reader.Seek(mIdLength, SeekOrigin.Current);

            if (IsIndexed)
            {
                // read palette and indices
                mPalette = ReadPalette(reader, mPaletteLength, mBpp, mPaletteDepth);
                mPaletteIndices = ReadIndices(reader, mDataType, mBpp, mWidth, mHeight);
            }
            else
            {
                // read pixels
                mPixels = ReadPixels(reader, mDataType, mBpp, mWidth, mHeight);
            }
        }

        internal override void Write(BinaryWriter writer)
        {
            writer.Write(mIdLength);
            writer.Write(mColorMapType);
            writer.Write((byte)mDataType);
            writer.Write(mPaletteStartIndex);
            writer.Write(mPaletteLength);
            writer.Write(mPaletteDepth);
            writer.Write(mXOrigin);
            writer.Write(mYOrigin);
            writer.Write(mWidth);
            writer.Write(mHeight);
            writer.Write(mBpp);
            writer.Write(mImageDescriptor);

            if (mDataType == TgaEncoding.None)
                return;

            if (IsIndexed)
            {
                // write palette and indices
                WritePalette(writer, mPalette, mPaletteLength, mBpp, mPaletteDepth);
                WriteIndices(writer, mPaletteIndices, mWidth, mHeight, mDataType, mBpp);
            }
            else
            {
                // write pixels
                WritePixels(writer, mDataType, mPixels, mBpp, mWidth, mHeight, false);
            }
        }

        // de/encoder delegate factory methdos
        private static TgaDataDecoder GetDecoder(TgaEncoding type)
        {
            switch (type)
            {
                case TgaEncoding.IndexedRLE:
                case TgaEncoding.RGBRLE:
                case TgaEncoding.GrayScaleCmp:
                case TgaEncoding.IndexedHDRLE:
                case TgaEncoding.IndexedHDRLEQ:
                    throw new NotImplementedException($"Decoder for data id: {type} is not implemented.");
                default:
                    throw new ArgumentException($"DataStructNode id: {type} is not encoded.");
            }
        }

        private static TgaDataEncoder GetEncoder(TgaEncoding type)
        {
            switch (type)
            {
                case TgaEncoding.IndexedRLE:
                case TgaEncoding.RGBRLE:
                case TgaEncoding.GrayScaleCmp:
                case TgaEncoding.IndexedHDRLE:
                case TgaEncoding.IndexedHDRLEQ:
                    throw new NotImplementedException($"Encoder for data id: {type} is not implemented.");
                default:
                    throw new ArgumentException($"DataStructNode id: {type} is not encoded.");
            }
        }

        // set header info on creation
        private void SetHeaderInfo(int w, int h, int bpp, int palDepth, TgaEncoding enc)
        {
            // check if the encoding uses rle or compression and throw an exception if it does
            if (EncodingUsesRleOrCmp(enc))
            { 
                throw new NotImplementedException("Encodings using RLE encoding or compression are not supported.");
            }
            else if (enc == TgaEncoding.Grayscale)
            {
                throw new NotImplementedException("Grayscale encoding is not supported.");
            }

            bool isIndexed = IsIndexedEncoding(enc);

            mIdLength = 0;
            mColorMapType = isIndexed ? (byte)1 : (byte)0;
            mDataType = enc;

            if (isIndexed)
            {
                mPaletteStartIndex = 0;
                mPaletteLength = (short)(1 << bpp);
                mPaletteDepth = (byte)palDepth;
            }

            mXOrigin = 0;
            mYOrigin = 0;
            mWidth = (short)w;
            mHeight = (short)h;
            mBpp = (byte)bpp;
            mImageDescriptor = 0;
        }

        private void CreateBitmap()
        {
            if (IsIndexed)
            {
                mBitmap = BitmapHelper.Create(mPalette, mPaletteIndices, mWidth, mHeight);
            }
            else
            {
                mBitmap = BitmapHelper.Create(mPixels, mWidth, mHeight);
            }
        }

        // encoding id checking helpers
        private static bool EncodingUsesRleOrCmp(TgaEncoding type)
        {
            switch (type)
            {
                case TgaEncoding.IndexedRLE:
                case TgaEncoding.RGBRLE:
                case TgaEncoding.GrayScaleCmp:
                case TgaEncoding.IndexedHDRLE:
                case TgaEncoding.IndexedHDRLEQ:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsIndexedEncoding(TgaEncoding encoding)
        {
            switch (encoding)
            {
                case TgaEncoding.Indexed:
                case TgaEncoding.IndexedRLE:
                case TgaEncoding.IndexedHDRLE:
                case TgaEncoding.IndexedHDRLEQ:
                    return true;
                default:
                    return false;
            }
        }

        // palette
        private static Color[] ReadPalette(BinaryReader reader, int length, int bpp, int depth)
        {
            byte[] paletteData = reader.ReadBytes((int)(length * ((float)depth / 8)));
            Color[] palette = DecodeUncompressedPixelDataToColors(paletteData, depth, bpp << (bpp / 4) - 1, bpp << (bpp / 4) - 1, true);
            return palette;
        }

        private static void WritePalette(BinaryWriter writer, Color[] colors, int length, int bpp, int depth)
        {
            WritePixelData(writer, colors, depth, bpp << (bpp / 4) - 1, bpp << (bpp / 4) - 1, true);
        }

        // indices
        private static byte[] DecodeUncompressedIndexDataTo8Bit(byte[] indices, int bpp, int width, int height)
        {
            switch (bpp)
            {
                case 8:
                    {
                        byte[] oldIndices = indices;
                        indices = new byte[oldIndices.Length];

                        for (int y = height - 1; y >= 0; y--)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                indices[x + (height - 1 - y) * width] = oldIndices[x + y * width];
                            }
                        }
                    }
                    break;

                case 4:
                    {
                        byte[] oldIndices = indices;
                        indices = new byte[oldIndices.Length * 2];

                        for (int y = height - 1; y >= 0; y--)
                        {
                            for (int x = 0; x < width; x += 2)
                            {
                                int nIdx = x + y * width;
                                int oIdx = (x + (height - 1 - y) * (width >> 1)) - (x >> 1);

                                indices[nIdx] = (byte)(oldIndices[oIdx] & 0x0F);
                                indices[nIdx + 1] = (byte)((oldIndices[oIdx] & 0xF0) >> 4);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            return indices;
        }

        private static byte[] ReadIndices(BinaryReader reader, TgaEncoding type, int bpp, int width, int height)
        {
            byte[] indices = reader.ReadBytes((int)((width * height) * ((float)bpp / 8)));

            if (EncodingUsesRleOrCmp(type))
            {
                TgaDataDecoder decoder = GetDecoder(type);
                throw new NotImplementedException();
            }
            else
            {
                indices = DecodeUncompressedIndexDataTo8Bit(indices, bpp, width, height);
            }

            return indices;
        }

        private static void WriteIndices(BinaryWriter writer, byte[] indices, int width, int height, TgaEncoding type, int bpp)
        {
            if (EncodingUsesRleOrCmp(type))
            {
                throw new NotImplementedException();
            }
            else
            {
                WriteIndicesData(writer, bpp, width, height, indices);
            }
        }

        private static void WriteIndicesData(BinaryWriter writer, int bpp, int width, int height, byte[] indices)
        {
            switch (bpp)
            {
                case 8:
                    for (int y = height - 1; y >= 0; y--)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            writer.Write(indices[x + y * width]);
                        }
                    }
                    break;

                case 4:
                    for (int y = height - 1; y >= 0; y--)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int idx = x + y * width;
                            writer.Write(indices[idx] | indices[idx + 1] << 4);
                        }
                    }
                    break;
            }
        }

        // pixels
        private static Color[] DecodeUncompressedPixelDataToColors(byte[] pixelData, int bpp, int width, int height, bool palette)
        {
            Color[] pixels;
            int pIdx = 0;
            switch (bpp)
            {
                case 32:
                    {
                        pixels = new Color[width * height];

                        if (palette)
                        {
                            for (int i = 0; i < pixelData.Length; i += 4)
                            {
                                pixels[pIdx++] = Color.FromArgb(BitConverter.ToInt32(pixelData, i));
                            }
                        }
                        else
                        {
                            for (int y = height - 1; y >= 0; y--)
                            {
                                for (int x = 0; x < width; x++, pIdx += 4)
                                {
                                    pixels[x + y * width] = Color.FromArgb(BitConverter.ToInt32(pixelData, pIdx));
                                }
                            }
                        }
                    }
                    break;
                case 24:
                    {
                        pixels = new Color[width*height];

                        if (palette)
                        {
                            for (int i = 0; i < pixelData.Length; i+=3)
                            {
                                pixels[pIdx++] = Color.FromArgb(pixelData[i+2], pixelData[i + 1], pixelData[i]);
                            }
                        }
                        else
                        {
                            for (int y = height - 1; y >= 0; y--)
                            {
                                for (int x = 0; x < width; x++, pIdx += 3)
                                {
                                    pixels[x + y * width] = Color.FromArgb(pixelData[pIdx+2], pixelData[pIdx+1], pixelData[pIdx]);
                                }
                            }
                        }
                    }
                    break;
                case 16:
                    {
                        pixels = new Color[width * height];

                        for (int y = height - 1; y >= 0; y--)
                        {
                            for (int x = 0; x < width; x++, pIdx+=2)
                            {
                                ushort color = BitConverter.ToUInt16(pixelData, pIdx);
                                pixels[x + (height - 1 - y) * width] = Color.FromArgb(
                                BitHelper.IsBitSet(color, 15) ? 0 : byte.MaxValue,  // a
                                BitHelper.GetBits(color, 5, 0) << 3,                // r
                                BitHelper.GetBits(color, 5, 5) << 3,                // g
                                BitHelper.GetBits(color, 5, 10) << 3);              // b
                            }
                        }
                    }
                    break;

                default:
                    throw new System.NotImplementedException();
            }
            return pixels;
        }

        private static Color[] ReadPixels(BinaryReader reader, TgaEncoding type, int bpp, int width, int height)
        {
            byte[] pixelData = reader.ReadBytes((int)((width * height) * ((float)bpp / 8)));

            if (EncodingUsesRleOrCmp(type))
            {
                TgaDataDecoder decoder = GetDecoder(type);
                throw new NotImplementedException();
            }
            else
            {
                return DecodeUncompressedPixelDataToColors(pixelData, bpp, width, height, false);
            }
        }

        private static void WritePixels(BinaryWriter writer, TgaEncoding type, Color[] pixels, int depth, int width, int height, bool palette)
        {
            if (EncodingUsesRleOrCmp(type))
            {
                throw new NotImplementedException();
            }
            else
            {
                WritePixelData(writer, pixels, depth, width, height, palette);
            }
        }

        private static void WritePixelData(BinaryWriter writer, Color[] pixels, int depth, int width, int height, bool palette)
        {
            switch (depth)
            {
                case 32:
                    {
                        if (palette)
                        {
                            writer.Write(pixels);
                        }
                        else
                        {
                            for (int y = height - 1; y >= 0; y--)
                            {
                                for (int x = 0; x < width; x++)
                                {
                                    writer.Write(pixels[x + y * width]);
                                }
                            }
                        }
                    }
                    break;

                case 24:
                    {
                        if (palette)
                        {
                            foreach (Color color in pixels)
                            {
                                writer.Write(color.B);
                                writer.Write(color.G);
                                writer.Write(color.R);
                            }
                        }
                        else
                        {
                            for (int y = height - 1; y >= 0; y--)
                            {
                                for (int x = 0; x < width; x++)
                                {
                                    Color pixel = pixels[x + y * width];
                                    writer.Write(pixel.B);
                                    writer.Write(pixel.G);
                                    writer.Write(pixel.R);
                                }
                            }
                        }
                    }
                    break;

                case 16:
                    {
                        if (palette)
                        {
                            foreach (Color color in pixels)
                            {
                                ushort px = 0;

                                if (color.A >= 0x7F)
                                {
                                    BitHelper.SetBit(ref px, 15);
                                }

                                BitHelper.ClearAndSetBits(ref px, 5, (ushort)(color.R >> 3), 0);
                                BitHelper.ClearAndSetBits(ref px, 5, (ushort)(color.G >> 3), 5);
                                BitHelper.ClearAndSetBits(ref px, 5, (ushort)(color.B >> 3), 10);

                                writer.Write(px);
                            }
                        }
                        else
                        {
                            for (int y = height - 1; y >= 0; y--)
                            {
                                for (int x = 0; x < width; x++)
                                {
                                    ushort px = 0;

                                    if (pixels[x + y * width].A >= 0x7F)
                                    {
                                        BitHelper.SetBit(ref px, 15);
                                    }

                                    BitHelper.ClearAndSetBits(ref px, 5, (ushort)(pixels[x + y * width].R >> 3), 0);
                                    BitHelper.ClearAndSetBits(ref px, 5, (ushort)(pixels[x + y * width].G >> 3), 5);
                                    BitHelper.ClearAndSetBits(ref px, 5, (ushort)(pixels[x + y * width].B >> 3), 10);

                                    writer.Write(px);
                                }
                            }
                        }
                    }
                    break;
            }
        }
    }
}
