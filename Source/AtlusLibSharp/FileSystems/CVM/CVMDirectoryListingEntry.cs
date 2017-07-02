using System.IO;
using System.Runtime.InteropServices;

namespace AtlusLibSharp.FileSystems.CVM
{
    using System;
    using AtlusLibSharp.Utilities;
    using ISO;

    [StructLayout(LayoutKind.Explicit, Size = SIZE)]
    internal struct DirectoryListingEntryHeader
    {
        public const int SIZE = 48;
        public const int NameLength = 32;

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
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NameLength)]
        public string name;
    }

    public class CvmDirectoryListingEntry
    {
        private DirectoryListingEntryHeader mHeader;
        private CvmDirectoryListing mOriginDirList;
        private CvmDirectoryListing mDirList;

        internal CvmDirectoryListingEntry(BinaryReader reader, CvmDirectoryListing originDirList)
        {
            mOriginDirList = originDirList;
            mHeader = reader.ReadStructure<DirectoryListingEntryHeader>(DirectoryListingEntryHeader.SIZE);
        }

        public int Size
        {
            get { return mHeader.size; }
        }

        public int LBA
        {
            get { return mHeader.LBA; }
        }

        public RecordFlags Flags
        {
            get { return (RecordFlags)mHeader.flags; }
        }

        public string Name
        {
            get { return mHeader.name; }
        }

        public CvmDirectoryListing DirectoryListing
        {
            get { return mDirList; }
            internal set { mDirList = value; }
        }

        public override string ToString()
        {
            return Name;
        }

        internal void InternalWrite(BinaryWriter writer)
        {
            writer.WriteStructure(mHeader);
        }

        internal void Update(IsoDirectoryRecord record)
        {
            if (mHeader.name != record.Name)
            {
                Console.WriteLine("Warning: CVM entry name mismatch! Expected: \"{0}\" Got: \"{1}\"", mHeader.name, record.Name);
            }

            mHeader.size = record.Size;
            mHeader.LBA = record.LBA;

            if (mDirList != null)
            {
                mDirList.Update(record);
            }
        }
    }
}
