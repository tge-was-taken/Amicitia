using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using AmicitiaLibrary.Utilities;
using CSharpImageLibrary;

namespace AmicitiaLibrary.Graphics.DDS
{
    public static class DDSEncoder
    {
        public static byte[] Encode( Bitmap bitmap )
        {
            var image     = GetImageEngineImageFromBitmap( bitmap );
            var ddsFormat = DetermineBestDDSFormat( bitmap );
            return image.Save( new ImageFormats.ImageEngineFormatDetails( ddsFormat ), MipHandling.GenerateNew, 0, 0, false );
        }

        private static ImageEngineImage GetImageEngineImageFromBitmap( Bitmap bitmap )
        {
            // save bitmap to stream
            var bitmapStream = new MemoryStream();
            bitmap.Save( bitmapStream, ImageFormat.Png );

            // create bitmap image
            return new ImageEngineImage( bitmapStream );
        }

        private static ImageEngineFormat DetermineBestDDSFormat( Bitmap bitmap )
        {
            var ddsFormat = ImageEngineFormat.DDS_DXT1;
            if ( BitmapHelper.HasTransparency( bitmap ) )
            {
                ddsFormat = ImageEngineFormat.DDS_DXT3;
            }

            return ddsFormat;
        }
    }
}
