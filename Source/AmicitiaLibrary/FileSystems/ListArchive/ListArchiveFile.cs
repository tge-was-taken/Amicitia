using System.Text;

namespace AmicitiaLibrary.FileSystems.ListArchive
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using IO;
    using PAKToolArchive;

    public sealed class ListArchiveFile : BinaryBase, ISimpleArchiveFile
    {
        // Fields
        private List<ListArchiveEntry> mEntries;
        private bool mBigEndian;

        public bool BigEndian
        {
            get => mBigEndian;
            set => mBigEndian = value;
        }

        // Constructors
        public ListArchiveFile(string path)
        {
            if (!VerifyFileType(path))
            {
                throw new InvalidDataException("Not a valid ListArchiveFile.");
            }

            using ( EndianBinaryReader reader = new EndianBinaryReader( File.OpenRead(path), Endianness.LittleEndian))
            {
                Read(reader);
            }
        }

        public ListArchiveFile(Stream stream, bool leaveOpen = false)
        {
            if (!VerifyFileType(stream))
            {
                throw new InvalidDataException("Not a valid ListArchiveFile.");
            }

            using (EndianBinaryReader reader = new EndianBinaryReader( stream, Encoding.Default, leaveOpen, Endianness.LittleEndian))
            {
                Read(reader);
            }
        }

        public ListArchiveFile(string[] filepaths)
        {
            mEntries = new List<ListArchiveEntry>(filepaths.Length);
            foreach (string path in filepaths)
            {
                mEntries.Add(new ListArchiveEntry(path));
            }
        }

        public ListArchiveFile()
        {
            mEntries = new List<ListArchiveEntry>();
        }

        // Properties
        public int EntryCount
        {
            get { return mEntries.Count; }
        }

        ISimpleArchiveFile ISimpleArchiveFile.Create(IEnumerable<IArchiveEntry> entries)
        {
            var file = new ListArchiveFile();
            foreach (var archiveEntry in entries)
            {
                file.Entries.Add(new ListArchiveEntry(archiveEntry.Name, archiveEntry.Data));
            }

            return file;
        }

        public List<ListArchiveEntry> Entries
        {
            get { return mEntries; }
        }

        IEnumerable<IArchiveEntry> ISimpleArchiveFile.Entries => Entries;

        // static methods
        public static ListArchiveFile Create(string directorypath)
        {
            return new ListArchiveFile(Directory.GetFiles(directorypath));
        }

        public static ListArchiveFile Create(PakToolArchiveFile pak)
        {
            ListArchiveFile arc = new ListArchiveFile();
            foreach (PakToolArchiveEntry entry in pak.Entries)
            {
                arc.Entries.Add(new ListArchiveEntry(entry.Name, entry.Data));
            }
            return arc;
        }

        public static bool VerifyFileType(string path)
        {
            return InternalVerifyFileType(File.OpenRead(path));
        }

        public static bool VerifyFileType(Stream stream)
        {
            return InternalVerifyFileType(stream);
        }

        private static bool InternalVerifyFileType(Stream stream)
        {
            // check stream length
            int entrySize = 36;
            if ( stream.Length <= 4 + entrySize )
                return false;

            byte[] testData = new byte[4 + entrySize];
            stream.Read( testData, 0, 4 + entrySize );
            stream.Position = 0;

            int numOfFiles = BitConverter.ToInt32( testData, 0 );

            // num of files sanity check
            if ( numOfFiles > 1024 || numOfFiles < 1 || ( numOfFiles * entrySize ) > stream.Length )
            {
                numOfFiles = ( int )( ( numOfFiles << 8 ) & 0xFF00FF00 ) | ( ( numOfFiles >> 8 ) & 0xFF00FF );
                numOfFiles = ( numOfFiles << 16 ) | ( ( numOfFiles >> 16 ) & 0xFFFF );

                if ( numOfFiles > 1024 || numOfFiles < 1 || ( numOfFiles * entrySize ) > stream.Length )
                    return false;
            }

            // check if the name field is correct
            bool nameTerminated = false;
            for ( int i = 0; i < entrySize - 4; i++ )
            {
                if ( testData[4 + i] == 0x00 )
                {
                    if ( i == 0 )
                        return false;

                    nameTerminated = true;
                }

                if ( testData[4 + i] != 0x00 && nameTerminated == true )
                    return false;
            }

            // first entry length sanity check
            int length = BitConverter.ToInt32( testData, entrySize );
            if ( length >= stream.Length || length < 0 )
            {
                length = ( int )( ( length << 8 ) & 0xFF00FF00 ) | ( ( length >> 8 ) & 0xFF00FF );
                length = ( length << 16 ) | ( ( length >> 16 ) & 0xFFFF );

                if ( length >= stream.Length || length < 0 )
                    return false;
            }

            return true;
        }

        // instance methods
        internal override void Write(BinaryWriter writer)
        {
            int count = mEntries.Count;
            if ( mBigEndian )
                count = EndiannessHelper.Swap( count );

            writer.Write( count );
            foreach (ListArchiveEntry entry in mEntries)
            {
                if ( mBigEndian )
                    entry.mBigEndian = true;
                else
                    entry.mBigEndian = false;

                entry.InternalWrite(writer);
            }
        }

        private void Read(EndianBinaryReader reader)
        {
            int numEntries = reader.ReadInt32();
            if ( numEntries > 1024 || numEntries < 1 || ( numEntries * 36 ) > reader.BaseStream.Length )
            {
                numEntries = ( int )( ( numEntries << 8 ) & 0xFF00FF00 ) | ( ( numEntries >> 8 ) & 0xFF00FF );
                numEntries = ( numEntries << 16 ) | ( ( numEntries >> 16 ) & 0xFFFF );

                if ( reader.Endianness == Endianness.LittleEndian )
                    reader.Endianness = Endianness.BigEndian;
                else
                    reader.Endianness = Endianness.LittleEndian;
            }

            mEntries = new List<ListArchiveEntry>(numEntries);
            for (int i = 0; i < numEntries; i++)
            {
                mEntries.Add(new ListArchiveEntry(reader));
            } 

            if ( reader.Endianness == Endianness.BigEndian )
                mBigEndian = true;
        }
    }
}
