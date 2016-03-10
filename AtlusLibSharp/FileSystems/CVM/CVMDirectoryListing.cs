namespace AtlusLibSharp.FileSystems.CVM
{
    using System.IO;
    using System.Runtime.InteropServices;
    using AtlusLibSharp.Utilities;
    using ISO;

    [StructLayout(LayoutKind.Explicit, Size = SIZE)]
    internal struct CVMDirectoryListingHeader
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

    public class CVMDirectoryListing
    {
        private CVMDirectoryListingHeader _header;
        private CVMDirectoryListingEntry _originEntry; // the entry this listing originates from, null if it's the root directory
        private CVMDirectoryListingEntry[] _subEntries;

        internal CVMDirectoryListing(BinaryReader reader, CVMDirectoryListingEntry originEntry)
        {
            _originEntry = originEntry;
            InternalRead(reader);
        }

        public CVMDirectoryListingEntry OriginEntry
        {
            get { return _originEntry; }
        }

        public CVMDirectoryListingEntry[] SubEntries
        {
            get { return _subEntries; }
        }

        internal void Update(ISODirectoryRecord record)
        {
            _header.directoryRecordLBA = record.LBA;
       
            for (int i = 0; i < _header.entryCount; i++)
            {
                _subEntries[i].Update(record.SubEntries[i]);
            }
        }

        internal void InternalWrite(BinaryWriter writer)
        {
            writer.WriteStructure(_header);

            for (int i = 0; i < _header.entryCount; i++)
            {
                _subEntries[i].InternalWrite(writer);
            }

            writer.AlignPosition(16);

            for (int i = 0; i < _header.entryCount; i++)
            {
                if (i > 1 && _subEntries[i].Flags.HasFlagUnchecked(RecordFlags.DirectoryRecord))
                {
                    _subEntries[i].DirectoryListing.InternalWrite(writer);
                }
            }
        }

        private void InternalRead(BinaryReader reader)
        {
            _header = reader.ReadStructure<CVMDirectoryListingHeader>(CVMDirectoryListingHeader.SIZE);
            _subEntries = new CVMDirectoryListingEntry[_header.entryCount];
          
            for (int i = 0; i < _header.entryCount; i++)
            {
                _subEntries[i] = new CVMDirectoryListingEntry(reader, this);
            }
            
            reader.AlignPosition(16);
           
            for (int i = 0; i < _header.entryCount; i++)
            {
                if (i > 1 && _subEntries[i].Flags.HasFlagUnchecked(RecordFlags.DirectoryRecord))
                {
                    _subEntries[i].DirectoryListing = new CVMDirectoryListing(reader, _subEntries[i]);
                }
            }
        }
    }
}
