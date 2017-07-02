namespace AtlusLibSharp.FileSystems.ISO
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using AtlusLibSharp.Utilities;
    using CVM;

    public class IsoDirectoryRecord
    {
        // StructNode fields
        private int mLba;
        private int mSize;
        private int mFlags;
        private string mName;

        // Properties
        private IsoDirectoryRecord mParent;
        private List<IsoDirectoryRecord> mSubEntries;

        public int LBA
        {
            get { return mLba; }
        }

        public int Size
        {
            get { return mSize; }
        }

        public int Flags
        {
            get { return mFlags; }
        }

        public string Name
        {
            get { return mName; }
        }

        public IsoDirectoryRecord ParentDirectory
        {
            get { return mParent; }
        }

        public List<IsoDirectoryRecord> SubEntries
        {
            get { return mSubEntries; }
        }

        internal IsoDirectoryRecord(BinaryReader reader, IsoDirectoryRecord parent)
        {
            mParent = parent;
            InternalRead(reader);
        }

        private void InternalRead(BinaryReader reader)
        {
            long posStart = reader.GetPosition();
            byte length = reader.ReadByte();
            byte extLength = reader.ReadByte();

            mLba = reader.ReadInt32();
            reader.Seek(0x04, SeekOrigin.Current); // LBA_BE

            mSize = reader.ReadInt32();
            reader.Seek(0x04 + 0x07, SeekOrigin.Current); // size_BE + datetime

            mFlags = reader.ReadByte();
            reader.Seek(0x06, SeekOrigin.Current); // unit size + interleave gap + volume seq number

            byte nameLength = reader.ReadByte();
            byte[] nameBytes = reader.ReadBytes(nameLength);

            if (nameBytes.Length == 1)
            {
                if (nameBytes[0] == 0)
                    mName = ".";
                else if (nameBytes[0] == 1)
                    mName = "..";
            }
            else
            {
                mName = Encoding.ASCII.GetString(nameBytes).Split(';')[0];
            }

            bool isDirectory = (mFlags & (int)RecordFlags.DirectoryRecord) == (int)RecordFlags.DirectoryRecord;
            bool isNotParentOrGrandparentDirectory = nameLength != 1 || mParent == null;

            if (isDirectory && isNotParentOrGrandparentDirectory)
            {
                reader.Seek(CvmFile.CVM_HEADER_SIZE + ((long)mLba * CvmFile.ISO_BLOCKSIZE), SeekOrigin.Begin);
                mSubEntries = new List<IsoDirectoryRecord>();

                // Set the initial sector start position
                long posSubEntriesSectorStart = reader.BaseStream.Position;
                long posSubEntriesDataEnd = posSubEntriesSectorStart + mSize;

                while (reader.BaseStream.Position < posSubEntriesDataEnd)
                {
                    IsoDirectoryRecord record = new IsoDirectoryRecord(reader, this);

                    mSubEntries.Add(record);

                    // Skip padding
                    while (reader.ReadByte() == 0 && reader.BaseStream.Position < posSubEntriesDataEnd);

                    // Break out of the loop if we've read to or past the end position
                    if (reader.BaseStream.Position >= posSubEntriesDataEnd)
                    {
                        break;
                    }

                    // We've found a non-zero byte, seek back
                    reader.BaseStream.Position -= 1;
                }
            }

            reader.Seek(posStart + length + extLength, SeekOrigin.Begin);
        }
    }
}
