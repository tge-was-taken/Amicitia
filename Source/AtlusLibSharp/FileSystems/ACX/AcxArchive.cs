using System.Collections.Generic;
using System.IO;
using AtlusLibSharp.IO;
using AtlusLibSharp.Utilities;

namespace AtlusLibSharp.FileSystems.ACX
{
    public class AcxFile : BinaryBase
    {
        private const int MAGIC = 0;

        public int EntryCount => Entries.Count;

        public List<BinaryFile> Entries { get; private set; }

        public AcxFile()
        {
            Entries = new List<BinaryFile>();
        }

        public AcxFile( string path )
        {
            using ( BinaryReader reader = new BinaryReader( File.OpenRead( path ) ) )
            {
                Read( reader );
            }
        }

        public AcxFile( Stream stream, bool leaveOpen = false )
        {
            using ( BinaryReader reader = new BinaryReader( stream ) )
            {
                Read( reader );
            }
        }

        internal AcxFile( BinaryReader reader )
        {
            Read( reader );
        }

        private int ReadBEInt( BinaryReader reader )
        {
            var bytes = reader.ReadBytes( sizeof( int ) );
            return bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3];
        }

        internal void Read( BinaryReader reader )
        {
            int magic = ReadBEInt( reader );
            if ( magic != MAGIC )
                throw new InvalidDataException();

            int entryCount = ReadBEInt( reader );
            var entryHeaders = new(int Offset, int Size)[entryCount];
            for ( int i = 0; i < entryHeaders.Length; i++ )
            {
                entryHeaders[i].Offset = ReadBEInt( reader );
                entryHeaders[i].Size = ReadBEInt( reader );
            }

            Entries = new List<BinaryFile>( entryCount );
            for ( int i = 0; i < Entries.Capacity; i++ )
            {
                reader.BaseStream.Seek( entryHeaders[i].Offset, SeekOrigin.Begin);
                var bytes = reader.ReadBytes( entryHeaders[i].Size );
                var entry = new BinaryFile( bytes );

                Entries.Add( entry );
            }
        }

        private void WriteBEInt(BinaryWriter writer, int value)
        {
            uint valueU = ( uint )value;
            int swapped = (int)(( valueU & 0xFF ) << 24 | ( valueU & 0xFF00 ) << 16 | ( valueU & 0xFF0000 ) << 8 | ( valueU & 0xFF000000 ));
            writer.Write( swapped );
        }

        internal override void Write(BinaryWriter writer)
        {
            WriteBEInt( writer, MAGIC );
            WriteBEInt( writer, EntryCount );

            var entryOffsets = new int[EntryCount];
            int entryOffset = AlignmentHelper.Align(8 + ( EntryCount * 8 ), 4);
            for (int i = 0; i < Entries.Count; i++ )
            {
                WriteBEInt( writer, entryOffset );
                WriteBEInt( writer, Entries[i].Length );

                entryOffsets[i] = entryOffset;
                entryOffset = AlignmentHelper.Align( entryOffset + ( Entries[i].Length ), 4 );
            }

            for ( int i = 0; i < Entries.Count; i++ )
            {
                writer.BaseStream.Seek( entryOffsets[i], SeekOrigin.Begin );
                Entries[i].Write( writer );
            }
        }
    }
}
