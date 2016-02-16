using System.IO;
using System.Runtime.InteropServices;

namespace AtlusLibSharp.Persona3.FileSystem.CRIWare
{
    using System;
    using Common.Utilities;

    [StructLayout(LayoutKind.Explicit, Size = SIZE)]
    internal struct DirectoryListingEntryHeader
    {
        public const int SIZE = 48;
        public const int NAME_LENGTH = 32;

        [FieldOffset(0)]
        public short pad; // always 0

        [FieldOffset(2)]
        public int size;

        [FieldOffset(6)]
        public int unused; // always 0

        [FieldOffset(10)]
        public int LBA;

        [FieldOffset(14)]
        public byte flags;

        [FieldOffset(15)]
        public byte unk0x0E;

        [FieldOffset(16)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NAME_LENGTH)]
        public string name;
    }

    public class CVMDirectoryListingEntry
    {
        private DirectoryListingEntryHeader _header;
        private CVMDirectoryListing _originDirList;
        private CVMDirectoryListing _dirList;

        internal CVMDirectoryListingEntry(BinaryReader reader, CVMDirectoryListing originDirList)
        {
            _originDirList = originDirList;
            _header = reader.ReadStructure<DirectoryListingEntryHeader>(DirectoryListingEntryHeader.SIZE);
        }

        public int Size
        {
            get { return _header.size; }
        }

        public int LBA
        {
            get { return _header.LBA; }
        }

        public RecordFlags Flags
        {
            get { return (RecordFlags)_header.flags; }
        }

        public string Name
        {
            get { return _header.name; }
        }

        public CVMDirectoryListing DirectoryListing
        {
            get { return _dirList; }
            internal set { _dirList = value; }
        }

        public override string ToString()
        {
            return Name;
        }

        internal void InternalWrite(BinaryWriter writer)
        {
            writer.WriteStructure(_header);
        }

        internal void Update(ISODirectoryRecord record)
        {
            if (_header.name != record.Name)
            {
                Console.WriteLine("Warning: CVM entry name mismatch! Expected: \"{0}\" Got: \"{1}\"", _header.name, record.Name);
            }

            _header.size = record.Size;
            _header.LBA = record.LBA;

            if (_dirList != null)
            {
                _dirList.Update(record);
            }
        }
    }

    [Flags]
    public enum RecordFlags : byte
    {
        FileRecord      = 1 << 0,
        DirectoryRecord = 1 << 1
    }
}
