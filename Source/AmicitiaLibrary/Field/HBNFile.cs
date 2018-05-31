using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AmicitiaLibrary.IO;

namespace AmicitiaLibrary.Field
{
    public class HbnFile : BinaryBase
    {
        public const int MAGIC = 0x45678901;

        public short VersionMajor { get; set; }

        public short VersionMinor { get; set; }

        public int Entry1Count => Entries1.Count;

        public int Entry1Size => 0x20;

        public int Entry2Count => Entries2.Count;

        public int Entry2Size => 0x14;

        public int Entry3Count => Entries3.Count;

        public int Entry3Size => 0x14;

        public int Entry4Count => Entries4.Count;

        public int Entry4Size => 0x14;

        public int Entry5Count => Entries5.Count;

        public int Entry5Size => 0x14;

        public int Entry6Count => Entries6.Count;

        public int Entry6Size => 0x20;

        public int Entry7Count => Entries7.Count;

        public int Entry7Size => 0x14;

        public int Entry8Count => Entries8.Count;

        public int Entry8Size => 0x14;

        public List<BinaryFile> Entries1 { get; private set; }

        public List<BinaryFile> Entries2 { get; private set; }

        public List<BinaryFile> Entries3 { get; private set; }

        public List<BinaryFile> Entries4 { get; private set; }

        public List<BinaryFile> Entries5 { get; private set; }

        public List<BinaryFile> Entries6 { get; private set; }

        public List<BinaryFile> Entries7 { get; private set; }

        public List<BinaryFile> Entries8 { get; private set; }

        public HbnFile( short versionMajor, short versionMinor )
        {
            VersionMajor = versionMajor;
            VersionMinor = versionMinor;
            Entries1 = new List<BinaryFile>();
            Entries2 = new List<BinaryFile>();
            Entries3 = new List<BinaryFile>();
            Entries4 = new List<BinaryFile>();
            Entries5 = new List<BinaryFile>();
            Entries6 = new List<BinaryFile>();
            Entries7 = new List<BinaryFile>();
            Entries8 = new List<BinaryFile>();
        }

        public HbnFile( string filepath ) : this(File.OpenRead(filepath), false) { }

        public HbnFile( Stream stream, bool leaveOpen = false )
        {
            using ( var reader = new BinaryReader( stream, Encoding.Default, leaveOpen ) )
                Read( reader );
        }

        internal void Read( BinaryReader reader )
        {
            int magic = reader.ReadInt32();
            if ( magic != MAGIC )
                throw new InvalidDataException();

            VersionMajor = reader.ReadInt16();
            VersionMinor = reader.ReadInt16();

            ReadEntryHeader( reader, Entry1Size, out int entry1Count );
            ReadEntryHeader( reader, Entry2Size, out int entry2Count );
            ReadEntryHeader( reader, Entry3Size, out int entry3Count );
            ReadEntryHeader( reader, Entry4Size, out int entry4Count );
            ReadEntryHeader( reader, Entry5Size, out int entry5Count );
            ReadEntryHeader( reader, Entry6Size, out int entry6Count );
            ReadEntryHeader( reader, Entry7Size, out int entry7Count );
            ReadEntryHeader( reader, Entry8Size, out int entry8Count );

            Entries1 = ReadEntries( entry1Count, Entry1Size, reader );
            Entries2 = ReadEntries( entry2Count, Entry2Size, reader );
            Entries3 = ReadEntries( entry3Count, Entry3Size, reader );
            Entries4 = ReadEntries( entry4Count, Entry4Size, reader );
            Entries5 = ReadEntries( entry5Count, Entry5Size, reader );
            Entries6 = ReadEntries( entry6Count, Entry6Size, reader );
            Entries7 = ReadEntries( entry7Count, Entry7Size, reader );
            Entries8 = ReadEntries( entry8Count, Entry8Size, reader );
        }

        private static void ReadEntryHeader( BinaryReader reader, int size, out int count)
        {
            count = reader.ReadInt32();
            int actualSize = reader.ReadInt32();
            if ( actualSize != size )
                throw new Exception();
        }

        private static List<BinaryFile> ReadEntries(int count, int size, BinaryReader reader)
        {
            var entries = new List<BinaryFile>( count );
            for ( int i = 0; i < entries.Capacity; i++ )
                entries.Add( new BinaryFile( reader.ReadBytes( size ) ) );

            return entries;
        }

        internal override void Write( BinaryWriter writer )
        {
            writer.Write( MAGIC );
            writer.Write( VersionMajor );
            writer.Write( VersionMinor );
            writer.Write( Entry1Count );
            writer.Write( Entry1Size );
            writer.Write( Entry2Count );
            writer.Write( Entry2Size );
            writer.Write( Entry3Count );
            writer.Write( Entry3Size );
            writer.Write( Entry4Count );
            writer.Write( Entry4Size );
            writer.Write( Entry5Count );
            writer.Write( Entry5Size );
            writer.Write( Entry6Count );
            writer.Write( Entry6Size );
            writer.Write( Entry7Count );
            writer.Write( Entry7Size );
            writer.Write( Entry8Count );
            writer.Write( Entry8Size );
            WriteEntries( Entries1, writer );
            WriteEntries( Entries2, writer );
            WriteEntries( Entries3, writer );
            WriteEntries( Entries4, writer );
            WriteEntries( Entries5, writer );
            WriteEntries( Entries6, writer );
            WriteEntries( Entries7, writer );
            WriteEntries( Entries8, writer );
        }

        private static void WriteEntries( List<BinaryFile> entries, BinaryWriter writer )
        {
            foreach ( var entry in entries )
            {
                entry.Write( writer );
            }   
        }
    }
}
