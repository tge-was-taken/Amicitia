namespace AtlusLibSharp.FileSystems.CVM
{
    using System.IO;
    using System.Runtime.InteropServices;
    using AtlusLibSharp.Utilities;
    using ISO;
    using System;

    [StructLayout(LayoutKind.Explicit, Size = SIZE)]
    internal struct CvmDirectoryListingHeader
    {
        public const int SIZE = 22;

        [FieldOffset(0)]
        public int entryCount;

        [FieldOffset(4)]
        public int entryCountAux; // same as entryCount

        [FieldOffset(8)]
        public int directoryRecordLBA; // absolute pos in cvm = 0x1800 + (directoryRecordLBA * 0x800)

        [FieldOffset(12)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8 + 1)]
        public string tag;

        [FieldOffset(20)]
        public short unused; // always 0
    }

    public class CvmDirectoryListing
    {
        private CvmDirectoryListingHeader mHeader;
        private CvmDirectoryListingEntry mOriginEntry; // the entry this listing originates from, null if it's the root directory
        private CvmDirectoryListingEntry[] mSubEntries;

        internal CvmDirectoryListing(BinaryReader reader, CvmDirectoryListingEntry originEntry)
        {
            mOriginEntry = originEntry;
            InternalRead(reader);
        }

        public CvmDirectoryListingEntry OriginEntry
        {
            get { return mOriginEntry; }
        }

        public CvmDirectoryListingEntry[] SubEntries
        {
            get { return mSubEntries; }
        }

        internal void Update(IsoDirectoryRecord record)
        {
            mHeader.directoryRecordLBA = record.LBA;
       
            for (int i = 0; i < mHeader.entryCount; i++)
            {
                // slow but lenient
                Array.Find(mSubEntries, elem => elem.Name == record.SubEntries[i].Name).Update(record.SubEntries[i]);
            }
        }

        internal void InternalWrite(BinaryWriter writer)
        {
            writer.WriteStructure(mHeader);

            for (int i = 0; i < mHeader.entryCount; i++)
            {
                mSubEntries[i].InternalWrite(writer);
            }

            writer.AlignPosition(16);

            for (int i = 0; i < mHeader.entryCount; i++)
            {
                if (i > 1 && mSubEntries[i].Flags.HasFlagUnchecked(RecordFlags.DirectoryRecord))
                {
                    mSubEntries[i].DirectoryListing.InternalWrite(writer);
                }
            }
        }

        private void InternalRead(BinaryReader reader)
        {
            mHeader = reader.ReadStructure<CvmDirectoryListingHeader>(CvmDirectoryListingHeader.SIZE);
            mSubEntries = new CvmDirectoryListingEntry[mHeader.entryCount];
          
            for (int i = 0; i < mHeader.entryCount; i++)
            {
                mSubEntries[i] = new CvmDirectoryListingEntry(reader, this);
            }
            
            reader.AlignPosition(16);
           
            for (int i = 0; i < mHeader.entryCount; i++)
            {
                if (i > 1 && mSubEntries[i].Flags.HasFlagUnchecked(RecordFlags.DirectoryRecord))
                {
                    mSubEntries[i].DirectoryListing = new CvmDirectoryListing(reader, mSubEntries[i]);
                }
            }
        }
    }
}
