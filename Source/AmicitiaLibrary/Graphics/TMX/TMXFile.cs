using System.Text;

namespace AmicitiaLibrary.Graphics.TMX
{
    using System;
    using System.IO;
    using System.Drawing;

    using PS2.Graphics;
    using IO;
    using Utilities;

    public class TmxFile : BinaryBase, ITextureFile
    {
        private const byte HEADER_SIZE = 0x10;
        private const short FLAG = 0x0002;
        private const string TAG = "TMX0";
        private const byte COMMENT_MAX_LENGTH = 28;

        private ushort mMipKl;
        private byte mWrapModes;
        private string mUserComment;
        private Bitmap mBitmap;

        /**********************/
        /**** Constructors ****/
        /**********************/

        public TmxFile(Bitmap bitmap, PS2PixelFormat pixelFormat = PS2PixelFormat.PSMT8, string comment = "")
        {
            Width = (ushort)bitmap.Width;
            Height = (ushort)bitmap.Height;
            PixelFormat = pixelFormat;
            mWrapModes = byte.MaxValue;
            UserComment = comment;

            switch (pixelFormat)
            {
                case PS2PixelFormat.PSMTC32:
                case PS2PixelFormat.PSMTC24:
                case PS2PixelFormat.PSMTC16:
                case PS2PixelFormat.PSMTC16S: // Non-indexed
                    PaletteFormat = 0;
                    Pixels = BitmapHelper.GetColors(bitmap);
                    mBitmap = bitmap;
                    break;
                case PS2PixelFormat.PSMT8:
                case PS2PixelFormat.PSMT8H:
                    SetupIndexedBitmap(bitmap, 256);
                    break;
                case PS2PixelFormat.PSMT4:
                case PS2PixelFormat.PSMT4HL:
                case PS2PixelFormat.PSMT4HH:
                    SetupIndexedBitmap(bitmap, 16);
                    break;
                default:
                    throw new ArgumentException("This pixel format is not supported for encoding.");
            }
        }

        public TmxFile(Stream stream, PS2PixelFormat pixelFormat = PS2PixelFormat.PSMT8, string comment = "")
            : this(new Bitmap(stream), pixelFormat, comment)
        {

        }

        public TmxFile(string path, PS2PixelFormat pixelFormat = PS2PixelFormat.PSMT8, string comment = "")
            : this(new Bitmap(path), pixelFormat, comment)
        {

        }

        public TmxFile(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                Read(reader);
        }

        public TmxFile(Stream stream, bool leaveOpen = false)
        {
            using (BinaryReader reader = new BinaryReader(stream, Encoding.Default, leaveOpen))
                Read(reader);
        }

        public TmxFile(byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
                Read(reader);
        }

        internal TmxFile(BinaryReader reader)
        {
            Read(reader);
        }

        /********************/
        /**** Properties ****/
        /********************/

        public byte PaletteCount { get; private set; }

        public PS2PixelFormat PaletteFormat { get; private set; }

        public ushort Width { get; private set; }

        public ushort Height { get; private set; }

        public PS2PixelFormat PixelFormat { get; private set; }

        public byte MipMapCount { get; private set; }

        public float MipMapKValue
        {
            get
            {
                if (mMipKl != ushort.MaxValue)
                {
                    return ((float)(mMipKl & 0x0FFF)) / (1 << 4);
                }
                else
                {
                    return -0.0625f;
                }
            }
        }

        public byte MipMapLValue
        {
            get
            {
                if (mMipKl != ushort.MaxValue)
                {
                    return (byte)(mMipKl & 0xF000);
                }
                else
                {
                    return 3;
                }
            }
        }

        public TmxWrapMode HorizontalWrappingMode
        {
            get
            {
                if (mWrapModes != byte.MaxValue)
                {
                    return (TmxWrapMode)((mWrapModes & 0xC) >> 2);
                }
                else
                {
                    return TmxWrapMode.Repeat;
                }
            }
            set
            {
                if (mWrapModes != byte.MaxValue)
                {
                    mWrapModes = (byte)(mWrapModes & ~0xC);
                    mWrapModes |= (byte)(((byte)value & 0x3) << 2);
                }
            }
        }

        public TmxWrapMode VerticalWrappingMode
        {
            get
            {
                if (mWrapModes != byte.MaxValue)
                {
                    return (TmxWrapMode)(mWrapModes & 0x3);
                }
                else
                {
                    return TmxWrapMode.Repeat;
                }
            }
            set
            {
                if (mWrapModes != byte.MaxValue)
                {
                    mWrapModes = (byte)(mWrapModes & ~0x3);
                    mWrapModes |= (byte)((byte)value & 0x3);
                }
            }
        }

        public int UserTextureId { get; set; }

        public int UserClutId { get; set; }

        public string UserComment
        {
            get
            {
                return mUserComment;
            }
            set
            {
                mUserComment = value;

                if (mUserComment.Length > COMMENT_MAX_LENGTH)
                {
                    mUserComment = mUserComment.Remove(COMMENT_MAX_LENGTH-1);
                }
            }
        }

        public int PaletteColorCount { get; private set; }

        public Color[][] Palettes { get; private set; }

        public byte[] PixelIndices { get; private set; }

        public byte[][] MipMapPixelIndices { get; private set; }

        public Color[] Pixels { get; private set; }

        public Color[][] MipMapPixels { get; private set; }

        public bool UsesPalette
        {
            get { return PaletteCount > 0; }
        }

        /*****************/
        /**** Methods ****/
        /*****************/

        public static TmxFile Load(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                return new TmxFile(reader);
        }

        public static TmxFile Load(Stream stream, bool leaveStreamOpen = false)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveStreamOpen))
                return new TmxFile(reader);
        }

        public Color[] GetPixels()
        {
            if (UsesPalette && Pixels == null)
            {
                Pixels = new Color[Width * Height];
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        Pixels[x + y * Width] = Palettes[0][PixelIndices[x + y * Width]];
            }

            return Pixels;
        }

        public Bitmap GetBitmap()
        {
            return GetBitmap(0, -1);
        }

        public Bitmap GetBitmap(int paletteIndex = 0, int mipMapIndex = -1)
        {
            if (mipMapIndex == -1)
            {
                // Check if the bitmap hasn't been created already
                if (mBitmap == null || paletteIndex != 0 || (mBitmap.Width != Width && mBitmap.Height != Height))
                {
                    CreateBitmap(paletteIndex, mipMapIndex);
                }
            }
            else
            {
                CreateBitmap(paletteIndex, mipMapIndex);
            }

            return mBitmap;
        }

        internal override void Write(BinaryWriter writer)
        {
            int posFileStart = (int)writer.BaseStream.Position;

            // Seek past chunk header
            writer.BaseStream.Seek(HEADER_SIZE, SeekOrigin.Current);

            writer.Write(PaletteCount);
            writer.Write((byte)PaletteFormat);
            writer.Write(Width);
            writer.Write(Height);
            writer.Write((byte)PixelFormat);
            writer.Write(MipMapCount);
            writer.Write(mMipKl);
            writer.Write((byte)0);
            writer.Write(mWrapModes);
            writer.Write(UserTextureId);
            writer.Write(UserClutId);
            writer.WriteCString(mUserComment, COMMENT_MAX_LENGTH);

            // Check if there's any palettes and write them
            if (UsesPalette)
            {
                WritePalette(writer);
            }

            // Write the pixels for the image and mipmaps
            WritePixels(writer);

            // Calculate the length
            int posFileEnd = (int)writer.BaseStream.Position;
            int length = posFileEnd - posFileStart;

            // Seek back to the chunk header and write it
            writer.BaseStream.Seek(posFileStart, SeekOrigin.Begin);
            writer.Write(FLAG);
            writer.Write((short)0); // userID
            writer.Write(length);
            writer.WriteCString(TAG, 4);

            // Seek back to the end of the data
            writer.BaseStream.Seek(posFileEnd, SeekOrigin.Begin);
        }

        private void Read(BinaryReader reader)
        {
            long posFileStart = reader.GetPosition();
            short flag = reader.ReadInt16();
            short userId = reader.ReadInt16();
            int length = reader.ReadInt32();
            string tag = reader.ReadCString(4);
            reader.AlignPosition(16);

            if (tag != TAG)
            {
                throw new InvalidDataException();
            }

            PaletteCount = reader.ReadByte();
            PaletteFormat = (PS2PixelFormat)reader.ReadByte();
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            PixelFormat = (PS2PixelFormat)reader.ReadByte();
            MipMapCount = reader.ReadByte();
            mMipKl = reader.ReadUInt16();
            byte reserved = reader.ReadByte();
            mWrapModes = reader.ReadByte();
            UserTextureId = reader.ReadInt32();
            UserClutId = reader.ReadInt32();
            UserComment = reader.ReadCString(28);

            // Check if there's any palettes and read them
            if (UsesPalette)
            {
                ReadPalette(reader);
            }

            // Read the pixels for the image and mipmaps
            ReadPixels(reader);
        }

        private void ReadPalette(BinaryReader reader)
        {
            int paletteWH;

            if (PixelFormat == PS2PixelFormat.PSMT4 || PixelFormat == PS2PixelFormat.PSMT4HL || PixelFormat == PS2PixelFormat.PSMT4HH)
            {
                PaletteColorCount = 16;
                paletteWH = 4;
            }
            else
            {
                PaletteColorCount = 256;
                paletteWH = 16;
            }

            Palettes = new Color[PaletteCount][];

            for (int i = 0; i < PaletteCount; i++)
            {
                PS2PixelFormatHelper.ReadPixelData(PaletteFormat, reader, paletteWH, paletteWH, out Palettes[i]);

                if (PaletteColorCount == 256)
                {
                    PS2PixelFormatHelper.TilePalette(Palettes[i], out Palettes[i]);
                }
            }
        }

        private void ReadPixels(BinaryReader reader)
        {
            if (UsesPalette)
            {
                byte[] temp;
                PS2PixelFormatHelper.ReadPixelData(PixelFormat, reader, Width, Height, out temp);
                PixelIndices = temp;

                MipMapPixelIndices = new byte[MipMapCount][];
                for (int i = 0; i < MipMapCount; i++)
                {
                    int div = 2 * (2 * i);
                    PS2PixelFormatHelper.ReadPixelData(PixelFormat, reader, Width / div, Height / div, out MipMapPixelIndices[i]);
                }
            }
            else
            {
                Color[] temp;
                PS2PixelFormatHelper.ReadPixelData(PixelFormat, reader, Width, Height, out temp);
                Pixels = temp;

                MipMapPixels = new Color[MipMapCount][];
                for (int i = 0; i < MipMapCount; i++)
                {
                    int div = 2 * (2 * i);
                    PS2PixelFormatHelper.ReadPixelData(PixelFormat, reader, Width / div, Height / div, out MipMapPixels[i]);
                }
            }
        }
     
        private void WritePalette(BinaryWriter writer)
        {
            Color[][] outPaletteEntries = null;

            int paletteWH = PS2PixelFormatHelper.GetPaletteDimension(PaletteFormat);

            if (PaletteColorCount == 256)
            {
                outPaletteEntries = new Color[PaletteCount][];

                // Tile the palette
                for (int i = 0; i < PaletteCount; i++)
                {
                    PS2PixelFormatHelper.TilePalette(Palettes[i], out outPaletteEntries[i]);
                }
            }
            else
            {
                outPaletteEntries = Palettes;
            }

            for (int i = 0; i < PaletteCount; i++)
            {
                PS2PixelFormatHelper.WritePixelData(PaletteFormat, writer, paletteWH, paletteWH, outPaletteEntries[i]);
            }
        }

        private void WritePixels(BinaryWriter writer)
        {
            if (UsesPalette)
            {
                PS2PixelFormatHelper.WritePixelData(PixelFormat, writer, Width, Height, PixelIndices);
                for (int i = 0; i < MipMapCount; i++)
                {
                    int div = 2 * (2 * i);
                    PS2PixelFormatHelper.WritePixelData(PixelFormat, writer, Width / div, Height / div, MipMapPixelIndices[i]);
                }
            }
            else
            {
                PS2PixelFormatHelper.WritePixelData(PixelFormat, writer, Width, Height, Pixels);
                for (int i = 0; i < MipMapCount; i++)
                {
                    int div = 2 * (2 * i);
                    PS2PixelFormatHelper.WritePixelData(PixelFormat, writer, Width / div, Height / div, MipMapPixels[i]);
                }
            }
        }

        private int GetMipDimension(int dim, int mipIdx)
        {
            int div = 2 * (2 * mipIdx);
            return dim / div;
        }

        private void CreateBitmap(int palIdx, int mipIdx)
        {
            if (UsesPalette)
            {
                Color[][] palettes;

                if (PaletteFormat == PS2PixelFormat.PSMTC32)
                    palettes = ScalePSMCT32PaletteToFullAlphaRange(Palettes);
                else
                    palettes = Palettes;

                if (mipIdx == -1)
                {
                    mBitmap = BitmapHelper.Create(palettes[palIdx], PixelIndices, Width, Height);
                }
                else
                {
                    mBitmap = BitmapHelper.Create(palettes[palIdx], MipMapPixelIndices[mipIdx],
                        GetMipDimension(Width, mipIdx), GetMipDimension(Height, mipIdx));
                }
            }
            else
            {
                if (mipIdx == -1)
                {
                    Color[] pixels = PixelFormat == PS2PixelFormat.PSMTC32 ? ScalePSMCT32PixelsToFullAlphaRange(Pixels) : Pixels;
                    mBitmap = BitmapHelper.Create(pixels, Width, Height);
                }
                else
                {
                    Color[] pixels = PixelFormat == PS2PixelFormat.PSMTC32 ? ScalePSMCT32PixelsToFullAlphaRange(MipMapPixels[mipIdx]) : MipMapPixels[mipIdx];
                    mBitmap = BitmapHelper.Create(pixels, GetMipDimension(Width, mipIdx), GetMipDimension(Height, mipIdx));
                }
            }
        }

        private void SetupIndexedBitmap(Bitmap bitmap, int paletteColorCount)
        {
            PaletteCount = 1;
            PaletteColorCount = paletteColorCount;
            Palettes = new Color[1][];

            byte[] temp;
            BitmapHelper.QuantizeBitmap(bitmap, paletteColorCount, out temp, out Palettes[0]);
            PixelIndices = temp;
        }

        private static Color[][] ScalePSMCT32PaletteToFullAlphaRange(Color[][] palettes)
        {
            int palettesCount = palettes.Length;
            Color[][] scaledPalettes = new Color[palettesCount][];

            for (int p = 0; p < palettesCount; p++)
            {
                scaledPalettes[p] = ScalePSMCT32PixelsToFullAlphaRange(palettes[p]);
            }

            return scaledPalettes;
        }

        private static Color[] ScalePSMCT32PixelsToFullAlphaRange(Color[] colors)
        {
            int colorCount = colors.Length;
            Color[] scaledColors = new Color[colorCount];

            for (int c = 0; c < colorCount; c++)
            {
                scaledColors[c] = Color.FromArgb(
                    PS2PixelFormatHelper.ScaleHalfRangeAlphaToFullRange(colors[c].A),
                    colors[c].R,
                    colors[c].G,
                    colors[c].B);
            }

            return scaledColors;
        }
    }
}
