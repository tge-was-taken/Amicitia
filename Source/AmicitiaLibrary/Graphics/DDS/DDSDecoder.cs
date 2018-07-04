using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CSharpImageLibrary;

namespace AmicitiaLibrary.Graphics.DDS
{
    public static class DDSDecoder
    {
        public static Bitmap Decode( byte[] data )
        {
            Bitmap bitmap;

            try
            {
                // load image from dds data
                var image = new ImageEngineImage( data );
                bitmap = ImageEngineImageToBitmap( image );
            }
            catch ( Exception )
            {
                // Bug: ImagineEngine randomly crashes? Seems to happen with P3D/P5D files only.
                // Seems like it doesn't support some configuration
                bitmap = new Bitmap( 32, 32, PixelFormat.Format32bppArgb );
                Trace.WriteLine( "ImageEngine failed to decode DDS texture" );
            }

            return bitmap;
        }

        private static Bitmap ImageEngineImageToBitmap( ImageEngineImage image )
        {
            // save the image to bmp
            var bitmapStream = new MemoryStream();
            image.Save( bitmapStream, new ImageFormats.ImageEngineFormatDetails( ImageEngineFormat.PNG ), MipHandling.KeepTopOnly, 0, 0, true );

            // load the saved bmp into a new bitmap
            return new Bitmap( bitmapStream );
        }
    }
}
