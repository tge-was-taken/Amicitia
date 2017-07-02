using AtlusLibSharp.Utilities;

namespace AtlusLibSharp.PS2.Graphics
{
    using System.IO;
    using System.Drawing;
    using System;

    internal static class PS2PixelFormatHelper
    {
        private const string EXCEPTION_INVALID_PXFORMAT = "Invalid pixel format specified.";

        // read/write delegates

        public delegate void ReadPixelColorDelegate(BinaryReader reader, int width, int height, out Color[] colorArray);

        public delegate void ReadPixelIndicesDelegate(BinaryReader reader, int width, int height, out byte[] indicesArray);

        public delegate void WritePixelColorDelegate(BinaryWriter writer, int width, int height, Color[] colorArray);

        public delegate void WritePixelIndicesDelegate(BinaryWriter writer, int width, int height, byte[] colorArray);

        // alpha scalers

        public static byte ScaleHalfRangeAlphaToFullRange(byte original)
        {
            return (byte)((original / 128.0f) * 255);
        }

        public static byte ScaleFullRangeAlphaToHalfRange(byte original)
        {
            return (byte)((original / 255.0f) * 128);
        }

        // helper methods to get info about the pixel format

        public static int GetPaletteDimension(PS2PixelFormat imageFmt)
        {
            int paletteWH = 16;

            if (imageFmt == PS2PixelFormat.PSMT4 ||
                imageFmt == PS2PixelFormat.PSMT4HH ||
                imageFmt == PS2PixelFormat.PSMT4HL)
                paletteWH = 4;

            return paletteWH;
        }

        public static int GetPixelFormatDepth(PS2PixelFormat fmt)
        {
            switch (fmt)
            {
                case PS2PixelFormat.PSMTC32:
                case PS2PixelFormat.PSMZ32:
                case PS2PixelFormat.PSMZ24:
                    return 32;
                case PS2PixelFormat.PSMTC24:
                    return 24;
                case PS2PixelFormat.PSMTC16:
                case PS2PixelFormat.PSMTC16S:
                case PS2PixelFormat.PSMZ16:
                case PS2PixelFormat.PSMZ16S:
                    return 16;
                case PS2PixelFormat.PSMT8:
                case PS2PixelFormat.PSMT8H:
                    return 8;
                case PS2PixelFormat.PSMT4:
                case PS2PixelFormat.PSMT4HL:
                case PS2PixelFormat.PSMT4HH:
                    return 4;

                default:
                    throw new ArgumentException(EXCEPTION_INVALID_PXFORMAT, "fmt");
            }
        }

        public static int GetIndexedColorCount(PS2PixelFormat fmt)
        {
            switch (fmt)
            {
                case PS2PixelFormat.PSMT8:
                case PS2PixelFormat.PSMT8H:
                    return 256;
                case PS2PixelFormat.PSMT4:
                case PS2PixelFormat.PSMT4HL:
                case PS2PixelFormat.PSMT4HH:
                    return 16;
                default:
                    throw new ArgumentException(EXCEPTION_INVALID_PXFORMAT, "fmt");
            }
        }

        public static int GetTexelDataSize(PS2PixelFormat fmt, int width, int height)
        {
            switch (fmt)
            {
                case PS2PixelFormat.PSMTC32:
                case PS2PixelFormat.PSMTC24:
                case PS2PixelFormat.PSMZ32:
                case PS2PixelFormat.PSMZ24:
                    return (width * height) * 4;

                case PS2PixelFormat.PSMTC16:
                case PS2PixelFormat.PSMTC16S:
                case PS2PixelFormat.PSMZ16:
                case PS2PixelFormat.PSMZ16S:
                    return (width * height) * 2;

                case PS2PixelFormat.PSMT8:
                case PS2PixelFormat.PSMT8H:
                    return (width * height) * 1;

                case PS2PixelFormat.PSMT4:
                case PS2PixelFormat.PSMT4HL:
                case PS2PixelFormat.PSMT4HH:
                    return (width * height) / 2; // 4 bit index only takes up half a texel

                default:
                    throw new ArgumentException(EXCEPTION_INVALID_PXFORMAT, "fmt");
            }
        }

        public static bool IsIndexedPixelFormat(PS2PixelFormat fmt)
        {
            switch (fmt)
            {
                case PS2PixelFormat.PSMT8:
                case PS2PixelFormat.PSMT4:
                case PS2PixelFormat.PSMT8H:
                case PS2PixelFormat.PSMT4HL:
                case PS2PixelFormat.PSMT4HH:
                    return true;
            }
            return false;
        }

        public static PS2PixelFormat GetBestPixelFormat(Bitmap bitmap)
        {
            /*
            int similarColorCount = BitmapHelper.GetSimilarColorCount(bitmap);

            if ( similarColorCount > 50 )
                return PS2PixelFormat.PSMT8;
            else
                return PS2PixelFormat.PSMT4;
            */

            return PS2PixelFormat.PSMT8;
        }

        // post/pre processing methods

        public static void TilePalette(Color[] colorArray, out Color[] newColorArray)
        {
            newColorArray = new Color[colorArray.Length];
            int newIndex = 0;
            int oldIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int x = 0; x < 8; x++)
                {
                    newColorArray[newIndex++] = colorArray[oldIndex++];
                }
                oldIndex += 8;
                for (int x = 0; x < 8; x++)
                {
                    newColorArray[newIndex++] = colorArray[oldIndex++];
                }
                oldIndex -= 16;
                for (int x = 0; x < 8; x++)
                {
                    newColorArray[newIndex++] = colorArray[oldIndex++];
                }
                oldIndex += 8;
                for (int x = 0; x < 8; x++)
                {
                    newColorArray[newIndex++] = colorArray[oldIndex++];
                }
            }
        }

        public static void UnSwizzle8(int width, int height, byte[] paletteIndices, out byte[] newPaletteIndices)
        {
            newPaletteIndices = new byte[paletteIndices.Length];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int block_location = (y & (~0xF)) * width + (x & (~0xF)) * 2;
                    int swap_selector = (((y + 2) >> 2) & 0x1) * 4;
                    int position_Y = (((y & (~3)) >> 1) + (y & 1)) & 0x7;
                    int column_location = position_Y * width * 2 + ((x + swap_selector) & 0x7) * 4;
                    int byte_number = ((y >> 1) & 1) + ((x >> 2) & 2); // 0,1,2,3
                    newPaletteIndices[y * width + x] = paletteIndices[block_location + column_location + byte_number];
                }
            }
        }

        public static void Swizzle8(int width, int height, byte[] paletteIndices, out byte[] newPaletteIndices)
        {
            newPaletteIndices = new byte[paletteIndices.Length];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte uPen = paletteIndices[(y * width + x)];

                    int block_location = (y & (~0xF)) * width + (x & (~0xF)) * 2;
                    int swap_selector = (((y + 2) >> 2) & 0x1) * 4;
                    int position_Y = (((y & (~3)) >> 1) + (y & 1)) & 0x7;
                    int column_location = position_Y * width * 2 + ((x + swap_selector) & 0x7) * 4;

                    int byte_number = ((y >> 1) & 1) + ((x >> 2) & 2); // 0,1,2,3

                    newPaletteIndices[block_location + column_location + byte_number] = uPen;
                }
            }
        }

        // read/write delegate factory methods

        public static ReadPixelColorDelegate GetReadPixelColorDelegate(PS2PixelFormat fmt)
        {
            switch (fmt)
            {
                case PS2PixelFormat.PSMTC32:
                case PS2PixelFormat.PSMZ32:
                    return ReadPSMCT32;

                case PS2PixelFormat.PSMZ24:
                case PS2PixelFormat.PSMTC24:
                    return ReadPSMCT24;

                case PS2PixelFormat.PSMTC16:
                case PS2PixelFormat.PSMZ16:
                    return ReadPSMCT16;

                case PS2PixelFormat.PSMZ16S:
                case PS2PixelFormat.PSMTC16S:
                    return ReadPSMCT16S;

                default:
                    throw new ArgumentException(EXCEPTION_INVALID_PXFORMAT, "fmt");
            }
        }

        public static ReadPixelIndicesDelegate GetReadPixelIndicesDelegate(PS2PixelFormat fmt)
        {
            switch (fmt)
            {
                case PS2PixelFormat.PSMT8:
                case PS2PixelFormat.PSMT8H:
                    return ReadPSMT8;

                case PS2PixelFormat.PSMT4:
                case PS2PixelFormat.PSMT4HL:
                case PS2PixelFormat.PSMT4HH:
                    return ReadPSMT4;

                default:
                    throw new ArgumentException(EXCEPTION_INVALID_PXFORMAT, "fmt");
            }
        }

        public static WritePixelColorDelegate GetWritePixelColorDelegate(PS2PixelFormat fmt)
        {
            switch (fmt)
            {
                case PS2PixelFormat.PSMTC32:
                case PS2PixelFormat.PSMZ32:
                    return WritePSMCT32;

                case PS2PixelFormat.PSMZ24:
                case PS2PixelFormat.PSMTC24:
                    return WritePSMCT24;

                case PS2PixelFormat.PSMTC16:
                case PS2PixelFormat.PSMZ16:
                    return WritePSMCT16;

                case PS2PixelFormat.PSMZ16S:
                case PS2PixelFormat.PSMTC16S:
                    return WritePSMCT16S;

                default:
                    throw new ArgumentException(EXCEPTION_INVALID_PXFORMAT, "fmt");
            }
        }

        public static WritePixelIndicesDelegate GetWritePixelIndicesDelegate(PS2PixelFormat fmt)
        {
            switch (fmt)
            {
                case PS2PixelFormat.PSMT8:
                case PS2PixelFormat.PSMT8H:
                    return WritePSMT8;

                case PS2PixelFormat.PSMT4:
                case PS2PixelFormat.PSMT4HL:
                case PS2PixelFormat.PSMT4HH:
                    return WritePSMT4;

                default:
                    throw new ArgumentException(EXCEPTION_INVALID_PXFORMAT, "fmt");
            }
        }

        // read methods

        public static void ReadPSMCT32(BinaryReader reader, int width, int height, out Color[] colorArray)
        {
            colorArray = new Color[height * width];
            int maxAlpha = -1;

            for (int i = 0; i < colorArray.Length; i++)
            {
                uint color = reader.ReadUInt32();
                colorArray[i] = Color.FromArgb(
                    (byte)(((color >> 24) & byte.MaxValue)),
                    (byte)(color & byte.MaxValue),
                    (byte)((color >> 8)   & byte.MaxValue),
                    (byte)((color >> 16)  & byte.MaxValue));

                if (colorArray[i].A > maxAlpha)
                {
                    maxAlpha = colorArray[i].A;
                }
            }

            // Check if the alpha value wasn't already in the 0x00 - 0xFF range
            if (!(maxAlpha > 0x80))
            {
                for (int i = 0; i < colorArray.Length; i++)
                {
                    colorArray[i] = Color.FromArgb(
                        ScaleHalfRangeAlphaToFullRange(colorArray[i].A),
                        colorArray[i].R,
                        colorArray[i].G,
                        colorArray[i].B);
                }
            }
        }

        public static void ReadPSMCT24(BinaryReader reader, int width, int height, out Color[] colorArray)
        {
            colorArray = new Color[height * width];
            for (int i = 0; i < colorArray.Length; i++)
            {
                uint color = reader.ReadUInt32();
                colorArray[i] = Color.FromArgb(
                    byte.MaxValue,
                    (byte)(color & byte.MaxValue),
                    (byte)((color >> 8) & byte.MaxValue),
                    (byte)((color >> 16) & byte.MaxValue));
            }
        }

        public static void ReadPSMCT16(BinaryReader reader, int width, int height, out Color[] colorArray)
        {
            colorArray = new Color[width * height];
            for (int i = 0; i < colorArray.Length; i++)
            {
                ushort color = reader.ReadUInt16();
                colorArray[i] = Color.FromArgb(
                byte.MaxValue,
                (byte)((color & 0x001F) << 3),
                (byte)(((color & 0x03E0) >> 5) << 3),
                (byte)(((color & 0x7C00) >> 10) << 3));
            }
        }

        public static void ReadPSMCT16S(BinaryReader reader, int width, int height, out Color[] colorArray)
        {
            colorArray = new Color[width * height];
            for (int i = 0; i < colorArray.Length; i++)
            {
                short color = reader.ReadInt16();
                colorArray[i] = Color.FromArgb(
                byte.MaxValue,
                (byte)((color & 0x001F) << 3),
                (byte)(((color & 0x03E0) >> 5) << 3),
                (byte)(((color & 0x7C00) >> 10) << 3));
            }
        }

        public static void ReadPSMT8(BinaryReader reader, int width, int height, out byte[] indicesArray)
        {
            indicesArray = new byte[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    indicesArray[x + y * width] = reader.ReadByte();
                }
            }
        }

        public static void ReadPSMT4(BinaryReader reader, int width, int height, out byte[] indicesArray)
        {
            indicesArray = new byte[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x += 2)
                {
                    byte indices = reader.ReadByte();
                    indicesArray[x + y * width] = (byte)(indices & 0x0F);
                    indicesArray[(x + 1) + y * width] = (byte)((indices & 0xF0) >> 4);
                }
            }
        }

        // write methods

        public static void WritePSMCT32(BinaryWriter writer, int width, int height, Color[] colorArray)
        {
            foreach (Color color in colorArray)
            {
                uint colorData = (uint)(color.R | (color.G << 8) | (color.B << 16) | (ScaleFullRangeAlphaToHalfRange(color.A) << 24));
                writer.Write(colorData);
            }
        }

        public static void WritePSMCT24(BinaryWriter writer, int width, int height, Color[] colorArray)
        {
            foreach (Color color in colorArray)
            {
                uint colorData = (uint)(color.R | (color.G << 8) | (color.B << 16));
                writer.Write(colorData);
            }
        }

        public static void WritePSMCT16(BinaryWriter writer, int width, int height, Color[] colorArray)
        {
            foreach (Color color in colorArray)
            {
                int r = color.R >> 3;
                int g = color.G >> 3;
                int b = color.B >> 3;
                int a = color.A >> 7;
                ushort colorData = (ushort)((a << 15) | (b << 10) | (g << 5) | (r));
                writer.Write(colorData);
            }
        }

        public static void WritePSMCT16S(BinaryWriter writer, int width, int height, Color[] colorArray)
        {
            foreach (Color color in colorArray)
            {
                short colorData = (short)((color.R & 0x1F) | ((color.G & 0x1F) << 8) | ((color.B & 0x1F) << 16));
                writer.Write(colorData);
            }
        }

        public static void WritePSMT8(BinaryWriter writer, int width, int height, byte[] indicesArray)
        {
            for (int i = 0; i < indicesArray.Length; i++)
            {
                writer.Write(indicesArray[i]);
            }
        }

        public static void WritePSMT4(BinaryWriter writer, int width, int height, byte[] indicesArray)
        {
            for (int i = 0; i < indicesArray.Length; i += 2)
            {
                writer.Write((byte)((indicesArray[i] & 0x0F) | ((indicesArray[i + 1] & 0x0F) << 4)));
            }
        }

        // generic read/write methods

        public static void ReadPixelData<T>(PS2PixelFormat fmt, BinaryReader reader, int width, int height, out T[] array)
        {
            if (IsIndexedPixelFormat(fmt))
            {
                ReadPixelIndicesDelegate readPixelIndices = GetReadPixelIndicesDelegate(fmt);
                byte[] pixelIndices;
                readPixelIndices(reader, width, height, out pixelIndices);
                array = pixelIndices as T[];
            }
            else
            {
                ReadPixelColorDelegate readPixels = GetReadPixelColorDelegate(fmt);
                Color[] pixels;
                readPixels(reader, width, height, out pixels);
                array = pixels as T[];
            }
        }

        public static void WritePixelData<T>(PS2PixelFormat fmt, BinaryWriter writer, int width, int height, T[] array)
        {
            if (IsIndexedPixelFormat(fmt))
            {
                WritePixelIndicesDelegate writePixelIndices = GetWritePixelIndicesDelegate(fmt);
                writePixelIndices(writer, width, height, array as byte[]);
            }
            else
            {
                WritePixelColorDelegate writePixels = GetWritePixelColorDelegate(fmt);
                writePixels(writer, width, height, array as Color[]);
            }
        }
    }
}
