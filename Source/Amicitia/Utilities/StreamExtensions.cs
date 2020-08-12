using System.IO;

namespace Amicitia.Utilities
{
    internal static class StreamExtensions
    {
        public static MemoryStream ToMemoryStream(this Stream stream, bool leaveOpen)
        {
            var mstream = new MemoryStream();
            stream.Position = 0;
            stream.CopyTo( mstream );
            mstream.Position = 0;

            if ( !leaveOpen )
                stream.Dispose();

            return mstream;
        }

        public static void SaveToFile(this Stream stream, bool leaveOpen, string filePath)
        {
            using (var fstream = File.Create( filePath ))
            {
                stream.Position = 0;
                stream.CopyTo( fstream );
            }

            if ( !leaveOpen )
                stream.Dispose();
        }
    }
}
