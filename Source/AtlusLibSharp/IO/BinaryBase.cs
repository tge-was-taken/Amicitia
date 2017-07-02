namespace AtlusLibSharp.IO
{
    using System.IO;
    using System.Text;

    public abstract class BinaryBase
    {
        public BinaryBase( ) { }

        /*
        public BinaryBase( string path )
        {
            using ( var fileStream = File.OpenRead( path ) )
            using ( var reader = new BinaryReader( fileStream ) )
            {
                Read( reader );
            }
        }

        public BinaryBase( Stream stream, bool leaveOpen = false )
        {
            using ( var reader = new BinaryReader( stream ) )
                Read( reader );
        }

        internal BinaryBase( BinaryReader reader ) => Read( reader );
        */

        public void Save(string path)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(path)))
            {
                Write(writer);
            }
        }

        public void Save(Stream stream, bool leaveOpen = true)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Default, leaveOpen ) )
            {
                Write(writer);
            }
        }

        public virtual byte[] GetBytes()
        {
            byte[] bytes;

            using (MemoryStream stream = new MemoryStream())
            {
                Save(stream);
                bytes = stream.ToArray();
            }

            return bytes;
        }

        public MemoryStream GetMemoryStream()
        {
            MemoryStream stream = new MemoryStream();
            Save(stream);
            stream.Position = 0;

            return stream;
        }

        internal abstract void Write( BinaryWriter writer );

        //internal abstract void Read( BinaryReader reader );
    }
}
