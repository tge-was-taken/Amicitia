namespace AtlusLibSharp.Utilities
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Threading.Tasks;

    public static class BitmapHelper
    {
        public static Bitmap Create(Color[] palette, byte[] indices, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = bitmap.LockBits
            (
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                bitmap.PixelFormat
            );                   

            unsafe
            {
                byte* p = (byte*)bitmapData.Scan0;
                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        int offset = (x * 4) + y * bitmapData.Stride;
                        Color color = palette[indices[x + y * width]];
                        p[offset] = color.B;
                        p[offset + 1] = color.G;
                        p[offset + 2] = color.R;
                        p[offset + 3] = color.A;
                    }
                });
            }    

            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        public static Bitmap Create(Color[] colors, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = bitmap.LockBits
            (
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                bitmap.PixelFormat
            );

            unsafe
            {
                byte* p = (byte*)bitmapData.Scan0;
                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        int offset = (x * 4) + y * bitmapData.Stride;
                        Color color = colors[x + y * width];
                        p[offset] = color.B;
                        p[offset + 1] = color.G;
                        p[offset + 2] = color.R;
                        p[offset + 3] = color.A;
                    }
                });
            }

            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        public static byte[] GetIndices(Bitmap bitmap)
        {
            BitmapData rawData = bitmap.LockBits
            (
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, 
                bitmap.PixelFormat
            );

            byte[] indices = new byte[rawData.Height * rawData.Width];

            // Performance critical code, so we're going to be using pointer arithmic
            unsafe
            {
                byte* p = (byte*)rawData.Scan0;
                Parallel.For(0, rawData.Height, y =>
                {
                    for (int x = 0; x < rawData.Width; x++)
                    {
                        int offset = y * rawData.Stride + x;
                        indices[x + y * rawData.Width] = (p[offset]);
                    }
                });
            }

            // Unlock the bitmap so it won't stay locked in memory
            bitmap.UnlockBits(rawData);

            return indices;
        }

        public static Color[] GetColors(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            BitmapData bitmapData = bitmap.LockBits
            (
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                bitmap.PixelFormat
            );

            Color[] colors = new Color[height * width];

            unsafe
            {
                byte* p = (byte*)bitmapData.Scan0;
                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        int offset = (x * 4) + y * bitmapData.Stride;
                        colors[x + y * width] = Color.FromArgb
                        (
                            p[offset + 3], p[offset + 2], p[offset + 1], p[offset]
                        );
                    }
                });
            }
            bitmap.UnlockBits(bitmapData);

            return colors;
        }

        public static Color[] GetPalette(Bitmap bitmap, int paletteColorCount)
        {
            Color[] palette = new Color[paletteColorCount];

            for (int i = 0; i < bitmap.Palette.Entries.Length; i++)
            {
                if (i == paletteColorCount)
                {
                    break;
                }

                palette[i] = bitmap.Palette.Entries[i];
            }

            return palette;
        }
    }
}
