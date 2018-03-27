using System.Collections.Generic;
using System.IO;
using AtlusLibSharp.IO;
using AtlusLibSharp.Utilities;

namespace AtlusLibSharp.FileSystems.ACX
{
    public class AcxFile : BinaryBase
    {
        private const int MAGIC = 0;
        private const int HEADER_SIZE = 8;
        private const int ENTRYHEADER_SIZE = 8;

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
            using ( BinaryReader reader = new BinaryReader( stream, System.Text.Encoding.Default, leaveOpen ) )
            {
                Read( reader );
            }
        }

        internal AcxFile( BinaryReader reader )
        {
            Read( reader );
        }

        private int Swap(int value)
        {
            value = ( int )( ( value << 8 ) & 0xFF00FF00 ) | ( ( value >> 8 ) & 0xFF00FF );
            return ( value << 16 ) | ( ( value >> 16 ) & 0xFFFF );
        }

        private int ReadBEInt( BinaryReader reader )
        {
            return Swap( reader.ReadInt32() );
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
            writer.Write( Swap( value ) );
        }

        internal override void Write(BinaryWriter writer)
        {
            WriteBEInt( writer, MAGIC );
            WriteBEInt( writer, EntryCount );

            var entryOffsets = new int[EntryCount];
            int entryOffset = AlignmentHelper.Align( HEADER_SIZE + ( EntryCount * ENTRYHEADER_SIZE ), 4);
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
