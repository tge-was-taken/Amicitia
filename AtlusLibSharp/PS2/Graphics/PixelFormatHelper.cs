namespace AtlusLibSharp.PS2.Graphics
{
    using System.IO;
    using System.Drawing;

    public static class PixelFormatHelper
    {
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
                        ConvertAlphaToPC(colorArray[i].A),
                        colorArray[i].R,
                        colorArray[i].G,
                        colorArray[i].B);
                }
            }
        }

        public static void WritePSMCT32(BinaryWriter writer, int width, int height, Color[] colorArray)
        {
            foreach (Color color in colorArray)
            {
                uint colorData = (uint)(color.R | (color.G << 8) | (color.B << 16) | (ConvertAlphaToPS2(color.A) << 24));
                writer.Write(colorData);
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
                    (byte)((color >> 8)  & byte.MaxValue),
                    (byte)((color >> 16) & byte.MaxValue));
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

        public static void WritePSMCT16S(BinaryWriter writer, int width, int height, Color[] colorArray)
        {
            foreach (Color color in colorArray)
            {
                short colorData = (short)((color.R & 0x1F) | ((color.G & 0x1F) << 8) | ((color.B & 0x1F) << 16));
                writer.Write(colorData);
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

        public static void WritePSMT8(BinaryWriter writer, int width, int height, byte[] indicesArray)
        {
            for (int i = 0; i < indicesArray.Length; i++)
            {
                writer.Write(indicesArray[i]);
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

        public static void WritePSMT4(BinaryWriter writer, int width, int height, byte[] indicesArray)
        {
            for (int i = 0; i < indicesArray.Length; i += 2)
            {
                writer.Write((byte)((indicesArray[i] & 0x0F) | ((indicesArray[i + 1] & 0x0F) << 4)));
            }
        }

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

        public static byte ConvertAlphaToPC(byte original)
        {
            return (byte)(((float)original / 0x80) * 0xFF); // scale the alpha value between 0x00 to 0xFF
        }

        public static byte ConvertAlphaToPS2(byte original)
        {
            return (byte)(((float)original / 0xFF) * 0x80); //scale the alpha value between 0x00 to 0x80
        }
    }
}
