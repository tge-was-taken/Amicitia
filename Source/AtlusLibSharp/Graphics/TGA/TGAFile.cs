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
    public class TGAFile : BinaryFileBase, ITextureFile
    {
        /****************************/
        /* TGA header struct fields */
        /****************************/
        private byte _idLength;
        private byte _colorMapType;
        private TGAEncoding _dataType;
        private short _paletteStartIndex;
        private short _paletteLength;
        private byte _paletteDepth;
        private short _xOrigin;
        private short _yOrigin;
        private short _width;
        private short _height;
        private byte _bpp;
        private byte _imageDescriptor;

        // palette and palette indices are used for indexed formats, otherwise they have value null
        private Color[] _palette;
        private byte[] _paletteIndices;

        // pixels is used for non-indexed formats, otherwise null
        private Color[] _pixels;

        // last created bitmap backing store
        private Bitmap _bitmap;

        // pixel de/encoder delegate
        private delegate byte[] TGADataDecoder(byte[] encoded);
        private delegate byte[] TGADataEncoder(byte[] unencoded);

        /// <summary>
        /// Gets the pixel data encoding type of the image data.
        /// </summary>
        public TGAEncoding Encoding
        {
            get { return _dataType; }
        }

        /// <summary>
        /// Gets a <see cref="System.Boolean"/> value indicating if this TGA uses a color palette together with per-pixel palette indices.
        /// </summary>
        public bool IsIndexed
        {
            get { return _colorMapType == 1; }
        }

        /// <summary>
        /// Gets the color depth of the palette. Returns 0 if the image is indexed.
        /// </summary>
        public int PaletteDepth
        {
            get { return _paletteDepth; }
        }

        /// <summary>
        /// Gets the width of the image.
        /// </summary>
        public int Width
        {
            get { return _width; }
        }

        /// <summary>
        /// Gets the height of the image.
        /// </summary>
        public int Height
        {
            get { return _height; }
        }

        /// <summary>
        /// Gets the number of bits per pixel of the image.
        /// </summary>
        public int BitsPerPixel
        {
            get { return _bpp; }
        }

        /// <summary>
        /// Gets the color palette of the image. Returns null if the image is not indexed.
        /// </summary>
        public Color[] Palette
        {
            get { return _palette; }
        }

        /// <summary>
        /// Gets the per-pixel palette color indices of the image. Returns null if the image is not indexed.
        /// </summary>
        public byte[] PixelIndices
        {
            get { return _paletteIndices; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TGAFile"/> class from the specified file.
        /// </summary>
        /// <param name="filename">The tga file name and path.</param>
        public TGAFile(string filename)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filename)))
                InternalRead(reader);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TGAFile"/> class from the specified stream.
        /// </summary>
        /// <param name="stream">The data stream used to load the image.</param>
        /// <param name="leaveStreamOpen">Specifies if the stream shouldn't be disposed after use.</param>
        public TGAFile(Stream stream, bool leaveStreamOpen = false)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveStreamOpen))
                InternalRead(reader);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TGAFile"/> class from the specified bitmap file path, and encoding parameters to encode the bitmap with.
        /// </summary>
        /// <param name="filename">The bitmap file path used for encoding.</param>
        /// <param name="encodingType">Specifies the pixel data encoding to use.</param>
        /// <param name="bitsPerPixel">Specifies the amount of bits per pixel. 
        /// 4 and 8 are reserved for indexed encodings, while 16, 24 and 32 are used for non-indexed images.</param>
        /// <param name="paletteDepth">Specifies the amount of bits per color in the palette. Ignored if the encoding type is of an indexed encoding.</param>
        public TGAFile(string filename, TGAEncoding encodingType = TGAEncoding.Indexed, int bitsPerPixel = 8, int paletteDepth = 32)
            : this(new Bitmap(filename), encodingType, bitsPerPixel, paletteDepth)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TGAFile"/> class from the specified <see cref="Bitmap"/> class, and encoding parameters to encode the bitmap with.
        /// </summary>
        /// <param name="bitmap">The bitmap instance used for encoding.</param>
        /// <param name="encodingType">Specifies the pixel data encoding to use.</param>
        /// <param name="bitsPerPixel">Specifies the amount of bits per pixel. 
        /// 4 and 8 are reserved for indexed encodings, while 16, 24 and 32 are used for non-indexed images.</param>
        /// <param name="paletteDepth">Specifies the amount of bits per color in the palette. Ignored if the encoding type is of an indexed encoding.</param>
        public TGAFile(Bitmap bitmap, TGAEncoding encodingType = TGAEncoding.Indexed, int bitsPerPixel = 8, int paletteDepth = 32)
        {
            // set header
            SetHeaderInfo(bitmap.Width, bitmap.Height, bitsPerPixel, paletteDepth, encodingType);

            // done if the encoding type is set to none
            if (encodingType == TGAEncoding.None)
                return;

            // set bitmap data
            if (IsIndexed)
            {
                BitmapHelper.QuantizeBitmap(bitmap, 1 << bitsPerPixel, out _paletteIndices, out _palette);
            }
            else
            {
                _pixels = BitmapHelper.GetColors(bitmap);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TGAFile"/> class from specified dimensions, color palette, pixel indices and encoding parameters that suit the provided image data.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="palette">The color palette of the image</param>
        /// <param name="pixelIndices">The per-pixel palette color indices of the image.</param>
        /// <param name="encodingType">Specifies the pixel data encoding to use.</param>
        /// <param name="bitsPerPixel">Specifies the amount of bits per pixel. 
        /// 4 and 8 are reserved for indexed encodings, while 16, 24 and 32 are used for non-indexed images.</param>
        /// <param name="paletteDepth">Specifies the amount of bits per color in the palette. Ignored if the encoding type is of an indexed encoding.</param>
        public TGAFile(int width, int height, Color[] palette, byte[] pixelIndices, 
            TGAEncoding encodingType = TGAEncoding.Indexed, int bitsPerPixel = 8, int paletteDepth = 32)
        {
            // set header
            SetHeaderInfo(width, height, bitsPerPixel, paletteDepth, encodingType);

            // done if the encoding type is set to none
            if (encodingType == TGAEncoding.None)
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
                    BitmapHelper.QuantizeBitmap(BitmapHelper.Create(palette, pixelIndices, width, height), 1 << bitsPerPixel, out _paletteIndices, out _palette);
                }
                else
                {
                    // set palette and pixel data
                    _palette = palette;
                    _paletteIndices = pixelIndices;
                }
            }
            else
            {
                _pixels = BitmapHelper.GetColors(BitmapHelper.Create(palette, pixelIndices, width, height));
            }
        }

        /// <summary>
        /// <para>Construct a bitmap from the data in this <see cref="TGAFile"/> instance.</para>
        /// <para>Subsequent calls to this method will return the same bitmap instance without constructing a new one.</para>
        /// </summary>
        public Bitmap GetBitmap()
        {
            // Check if the bitmap hasn't been created already
            if (_bitmap == null || (_bitmap.Width != _width && _bitmap.Height != _height))
            {
                CreateBitmap();
            }         

            return _bitmap;
        }


        public Color[] GetPixels()
        {
            if (IsIndexed && _pixels == null)
            {
                _pixels = new Color[_width * _height];
                for (int y = 0; y < _height; y++)
                    for (int x = 0; x < _width; x++)
                        _pixels[x + y * _width] = _palette[_paletteIndices[x + y * _width]];
            }

            return _pixels;
        }

        internal void InternalRead(BinaryReader reader)
        {
            _idLength = reader.ReadByte();
            _colorMapType = reader.ReadByte();
            _dataType = (TGAEncoding)reader.ReadByte();
            _paletteStartIndex = reader.ReadInt16();
            _paletteLength = reader.ReadInt16();
            _paletteDepth = reader.ReadByte();
            _xOrigin = reader.ReadInt16();
            _yOrigin = reader.ReadInt16();
            _width = reader.ReadInt16();
            _height = reader.ReadInt16();
            _bpp = reader.ReadByte();
            _imageDescriptor = reader.ReadByte();

            // format check
            if (EncodingUsesRLEOrCmp(_dataType) || _dataType == TGAEncoding.Grayscale)
            {
                throw new NotImplementedException($"Data type not supported: {_dataType}");
            }
            else if (_dataType == TGAEncoding.None)
            {
                return;
            }

            // skip user data
            reader.Seek(_idLength, SeekOrigin.Current);

            if (IsIndexed)
            {
                // read palette and indices
                _palette = ReadPalette(reader, _paletteLength, _bpp, _paletteDepth);
                _paletteIndices = ReadIndices(reader, _dataType, _bpp, _width, _height);
            }
            else
            {
                // read pixels
                _pixels = ReadPixels(reader, _dataType, _bpp, _width, _height);
            }
        }

        internal override void InternalWrite(BinaryWriter writer)
        {
            writer.Write(_idLength);
            writer.Write(_colorMapType);
            writer.Write((byte)_dataType);
            writer.Write(_paletteStartIndex);
            writer.Write(_paletteLength);
            writer.Write(_paletteDepth);
            writer.Write(_xOrigin);
            writer.Write(_yOrigin);
            writer.Write(_width);
            writer.Write(_height);
            writer.Write(_bpp);
            writer.Write(_imageDescriptor);

            if (_dataType == TGAEncoding.None)
                return;

            if (IsIndexed)
            {
                // write palette and indices
                WritePalette(writer, _palette, _paletteLength, _bpp, _paletteDepth);
                WriteIndices(writer, _paletteIndices, _width, _height, _dataType, _bpp);
            }
            else
            {
                // write pixels
                WritePixels(writer, _dataType, _pixels, _bpp, _width, _height, false);
            }
        }

        // de/encoder delegate factory methdos
        private static TGADataDecoder GetDecoder(TGAEncoding type)
        {
            switch (type)
            {
                case TGAEncoding.IndexedRLE:
                case TGAEncoding.RGBRLE:
                case TGAEncoding.GrayScaleCmp:
                case TGAEncoding.IndexedHDRLE:
                case TGAEncoding.IndexedHDRLEQ:
                    throw new NotImplementedException($"Decoder for data type: {type} is not implemented.");
                default:
                    throw new ArgumentException($"Data type: {type} is not encoded.");
            }
        }

        private static TGADataEncoder GetEncoder(TGAEncoding type)
        {
            switch (type)
            {
                case TGAEncoding.IndexedRLE:
                case TGAEncoding.RGBRLE:
                case TGAEncoding.GrayScaleCmp:
                case TGAEncoding.IndexedHDRLE:
                case TGAEncoding.IndexedHDRLEQ:
                    throw new NotImplementedException($"Encoder for data type: {type} is not implemented.");
                default:
                    throw new ArgumentException($"Data type: {type} is not encoded.");
            }
        }

        // set header info on creation
        private void SetHeaderInfo(int w, int h, int bpp, int palDepth, TGAEncoding enc)
        {
            // check if the encoding uses rle or compression and throw an exception if it does
            if (EncodingUsesRLEOrCmp(enc))
            { 
                throw new NotImplementedException("Encodings using RLE encoding or compression are not supported.");
            }
            else if (enc == TGAEncoding.Grayscale)
            {
                throw new NotImplementedException("Grayscale encoding is not supported.");
            }

            bool isIndexed = IsIndexedEncoding(enc);

            _idLength = 0;
            _colorMapType = isIndexed ? (byte)1 : (byte)0;
            _dataType = enc;

            if (isIndexed)
            {
                _paletteStartIndex = 0;
                _paletteLength = (short)(1 << bpp);
                _paletteDepth = (byte)palDepth;
            }

            _xOrigin = 0;
            _yOrigin = 0;
            _width = (short)w;
            _height = (short)h;
            _bpp = (byte)bpp;
            _imageDescriptor = 0;
        }

        private void CreateBitmap()
        {
            if (IsIndexed)
            {
                _bitmap = BitmapHelper.Create(_palette, _paletteIndices, _width, _height);
            }
            else
            {
                _bitmap = BitmapHelper.Create(_pixels, _width, _height);
            }
        }

        // encoding type checking helpers
        private static bool EncodingUsesRLEOrCmp(TGAEncoding type)
        {
            switch (type)
            {
                case TGAEncoding.IndexedRLE:
                case TGAEncoding.RGBRLE:
                case TGAEncoding.GrayScaleCmp:
                case TGAEncoding.IndexedHDRLE:
                case TGAEncoding.IndexedHDRLEQ:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsIndexedEncoding(TGAEncoding encoding)
        {
            switch (encoding)
            {
                case TGAEncoding.Indexed:
                case TGAEncoding.IndexedRLE:
                case TGAEncoding.IndexedHDRLE:
                case TGAEncoding.IndexedHDRLEQ:
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

        private static byte[] ReadIndices(BinaryReader reader, TGAEncoding type, int bpp, int width, int height)
        {
            byte[] indices = reader.ReadBytes((int)((width * height) * ((float)bpp / 8)));

            if (EncodingUsesRLEOrCmp(type))
            {
                TGADataDecoder decoder = GetDecoder(type);
                throw new NotImplementedException();
            }
            else
            {
                indices = DecodeUncompressedIndexDataTo8Bit(indices, bpp, width, height);
            }

            return indices;
        }

        private static void WriteIndices(BinaryWriter writer, byte[] indices, int width, int height, TGAEncoding type, int bpp)
        {
            if (EncodingUsesRLEOrCmp(type))
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

        private static Color[] ReadPixels(BinaryReader reader, TGAEncoding type, int bpp, int width, int height)
        {
            byte[] pixelData = reader.ReadBytes((int)((width * height) * ((float)bpp / 8)));

            if (EncodingUsesRLEOrCmp(type))
            {
                TGADataDecoder decoder = GetDecoder(type);
                throw new NotImplementedException();
            }
            else
            {
                return DecodeUncompressedPixelDataToColors(pixelData, bpp, width, height, false);
            }
        }

        private static void WritePixels(BinaryWriter writer, TGAEncoding type, Color[] pixels, int depth, int width, int height, bool palette)
        {
            if (EncodingUsesRLEOrCmp(type))
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
