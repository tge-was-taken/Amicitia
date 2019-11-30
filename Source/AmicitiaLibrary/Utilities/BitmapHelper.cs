using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace AmicitiaLibrary.Utilities
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Threading.Tasks;
    using nQuant;

    /// <summary>
    /// Contains helper methods for creating and converting bitmaps to and from indexed bitmap data.
    /// </summary>
    internal static class BitmapHelper
    {
        /// <summary>
        /// Create a new <see cref="Bitmap"/> instance using a color palette, per-pixel palette color indices and the image width and height.
        /// </summary>
        /// <param name="palette">The color palette used by the image.</param>
        /// <param name="indices">The per-pixel palette color indices used by the image.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <returns>A new <see cref="Bitmap"/> instance created using the data provided.</returns>
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

        /// <summary>
        /// Create a new <see cref="Bitmap"/> instance using an array of <see cref="Color"/> pixels and the image width and height.
        /// </summary>
        /// <param name="colors"><see cref="Color"/> array containing the color of each pixel in the image.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <returns>A new <see cref="Bitmap"/> instance created using the data provided.</returns>
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

        /// <summary>
        /// Read the per-pixel palette color indices of an indexed bitmap and return them.
        /// </summary>
        /// <param name="bitmap">The indexed bitmap to read the per-pixel palette color indices of.</param>
        /// <returns>Array of <see cref="System.Byte"/> containing the per-pixel palette color indices.</returns>
        public static byte[] GetIndices(Bitmap bitmap)
        {
            BitmapData rawData = bitmap.LockBits
            (
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat
            );

            byte[] indices = new byte[rawData.Height * rawData.Width];

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

        /// <summary>
        /// Read the pixel colors of a bitmap and return them.
        /// </summary>
        /// <param name="bitmap">The bitmap to read the pixel colors of.</param>
        /// <returns>Array of <see cref="Color"/> containing the color of each pixel in the <see cref="Bitmap"/>.</returns>
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

        /// <summary>
        /// Retrieve the color palette of an indexed bitmap and returns a specified max limit of colors to return. 
        /// The limit does not guarantee the amount of colors returned if the palette contains less colors than the specified limit.
        /// </summary>
        /// <param name="bitmap">The bitmap to read the palette palette of.</param>
        /// <param name="paletteColorCount">The max limit of palette colors to return.</param>
        /// <returns>Array of <see cref="Color"/> containing the palette colors of the <see cref="Bitmap"/>.</returns>
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

        /// <summary>
        /// Encodes a bitmap into an indexed bitmap with a per-pixel palette color index using a specified number of colors in the palette.
        /// </summary>
        /// <param name="bitmap">The bitmap to encode.</param>
        /// <param name="paletteColorCount">The number of colors to be present in the palette.</param>
        /// <param name="indices">The per-pixel palette color indices.</param>
        /// <param name="palette">The <see cref="Color"/> array containing the palette colors of the indexed bitmap.</param>
        public static void QuantizeBitmap(Bitmap bitmap, int paletteColorCount, out byte[] indices, out Color[] palette)
        {
            int bitDepth = Image.GetPixelFormatSize(bitmap.PixelFormat);
            if (bitDepth != 32)
            {
                bitmap = ConvertTo32Bpp(bitmap);
            }

            WuQuantizer quantizer = new WuQuantizer();
            Bitmap quantBitmap = (Bitmap)quantizer.QuantizeImage(bitmap, paletteColorCount, 0, 1);
            palette = GetPalette(quantBitmap, paletteColorCount);
            indices = GetIndices(quantBitmap);
        }

        //public static int GetSimilarColorCount(Bitmap bitmap, double threshold = ColorHelper.SIMILARITY_THRESHOLD_STRICT)
        //{
        //    return GetSimilarColorCount(GetColors(bitmap), threshold);
        //}

        //public static int GetSimilarColorCount(Color[] colors, double threshold = ColorHelper.SIMILARITY_THRESHOLD_STRICT )
        //{
        //    List<Color> uniqueColors = new List<Color>();

        //    foreach (var color in colors)
        //    {
        //        if (!uniqueColors.Any(x => x.IsSimilar(color, threshold)))
        //            uniqueColors.Add(color);
        //    }

        //    return uniqueColors.Count;
        //}

        private static Bitmap ConvertTo32Bpp(Image img)
        {
            var bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
            using (var gr = Graphics.FromImage(bmp))
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            return bmp;
        }

        // https://stackoverflow.com/questions/3064854/determine-if-alpha-channel-is-used-in-an-image/39013496#39013496
        public static Boolean HasTransparency( Bitmap bitmap )
        {
            // not an alpha-capable color format.
            if ( ( bitmap.Flags & ( Int32 )ImageFlags.HasAlpha ) == 0 )
                return false;
            // Indexed formats. Special case because one index on their palette is configured as THE transparent color.
            if ( bitmap.PixelFormat == PixelFormat.Format8bppIndexed || bitmap.PixelFormat == PixelFormat.Format4bppIndexed )
            {
                ColorPalette pal = bitmap.Palette;
                // Find the transparent index on the palette.
                Int32 transCol = -1;
                for ( int i = 0; i < pal.Entries.Length; i++ )
                {
                    Color col = pal.Entries[i];
                    if ( col.A != 255 )
                    {
                        // Color palettes should only have one index acting as transparency. Not sure if there's a better way of getting it...
                        transCol = i;
                        break;
                    }
                }
                // none of the entries in the palette have transparency information.
                if ( transCol == -1 )
                    return false;
                // Check pixels for existence of the transparent index.
                Int32 colDepth = Image.GetPixelFormatSize( bitmap.PixelFormat );
                BitmapData data = bitmap.LockBits( new Rectangle( 0, 0, bitmap.Width, bitmap.Height ), ImageLockMode.ReadOnly, bitmap.PixelFormat );
                Int32 stride = data.Stride;
                Byte[] bytes = new Byte[bitmap.Height * stride];
                Marshal.Copy( data.Scan0, bytes, 0, bytes.Length );
                bitmap.UnlockBits( data );
                if ( colDepth == 8 )
                {
                    // Last line index.
                    Int32 lineMax = bitmap.Width - 1;
                    for ( Int32 i = 0; i < bytes.Length; i++ )
                    {
                        // Last position to process.
                        Int32 linepos = i % stride;
                        // Passed last image byte of the line. Abort and go on with loop.
                        if ( linepos > lineMax )
                            continue;
                        Byte b = bytes[i];
                        if ( b == transCol )
                            return true;
                    }
                }
                else if ( colDepth == 4 )
                {
                    // line size in bytes. 1-indexed for the moment.
                    Int32 lineMax = bitmap.Width / 2;
                    // Check if end of line ends on half a byte.
                    Boolean halfByte = bitmap.Width % 2 != 0;
                    // If it ends on half a byte, one more needs to be processed.
                    // We subtract in the other case instead, to make it 0-indexed right away.
                    if ( !halfByte )
                        lineMax--;
                    for ( Int32 i = 0; i < bytes.Length; i++ )
                    {
                        // Last position to process.
                        Int32 linepos = i % stride;
                        // Passed last image byte of the line. Abort and go on with loop.
                        if ( linepos > lineMax )
                            continue;
                        Byte b = bytes[i];
                        if ( ( b & 0x0F ) == transCol )
                            return true;
                        if ( halfByte && linepos == lineMax ) // reached last byte of the line. If only half a byte to check on that, abort and go on with loop.
                            continue;
                        if ( ( ( b & 0xF0 ) >> 4 ) == transCol )
                            return true;
                    }
                }
                return false;
            }
            if ( bitmap.PixelFormat == PixelFormat.Format32bppArgb || bitmap.PixelFormat == PixelFormat.Format32bppPArgb )
            {
                BitmapData data = bitmap.LockBits( new Rectangle( 0, 0, bitmap.Width, bitmap.Height ), ImageLockMode.ReadOnly, bitmap.PixelFormat );
                Byte[] bytes = new Byte[bitmap.Height * data.Stride];
                Marshal.Copy( data.Scan0, bytes, 0, bytes.Length );
                bitmap.UnlockBits( data );
                for ( Int32 p = 3; p < bytes.Length; p += 4 )
                {
                    if ( bytes[p] != 255 )
                        return true;
                }
                return false;
            }
            // Final "screw it all" method. This is pretty slow, but it won't ever be used, unless you
            // encounter some really esoteric types not handled above, like 16bppArgb1555 and 64bppArgb.
            for ( Int32 i = 0; i < bitmap.Width; i++ )
            {
                for ( Int32 j = 0; j < bitmap.Height; j++ )
                {
                    if ( bitmap.GetPixel( i, j ).A != 255 )
                        return true;
                }
            }
            return false;
        }
    }
}
