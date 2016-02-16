using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AtlusLibSharp.Persona3.FileSystem.CRIWare
{
    using Common.Utilities;

    public class ISODirectoryRecord
    {
        // Struct fields
        private int _LBA;
        private int _size;
        private int _flags;
        private string _name;

        // Properties
        private ISODirectoryRecord _parent;
        private List<ISODirectoryRecord> _subEntries;

        public int LBA
        {
            get { return _LBA; }
        }

        public int Size
        {
            get { return _size; }
        }

        public int Flags
        {
            get { return _flags; }
        }

        public string Name
        {
            get { return _name; }
        }

        public ISODirectoryRecord ParentDirectory
        {
            get { return _parent; }
        }

        public List<ISODirectoryRecord> SubEntries
        {
            get { return _subEntries; }
        }

        internal ISODirectoryRecord(BinaryReader reader, ISODirectoryRecord parent)
        {
            _parent = parent;
            InternalRead(reader);
        }

        private void InternalRead(BinaryReader reader)
        {
            long posStart = reader.GetPosition();
            byte length = reader.ReadByte();
            byte extLength = reader.ReadByte();

            _LBA = reader.ReadInt32();
            reader.Seek(0x04, SeekOrigin.Current); // LBA_BE

            _size = reader.ReadInt32();
            reader.Seek(0x04 + 0x07, SeekOrigin.Current); // size_BE + datetime

            _flags = reader.ReadByte();
            reader.Seek(0x06, SeekOrigin.Current); // unit size + interleave gap + volume seq number

            byte nameLength = reader.ReadByte();
            byte[] nameBytes = reader.ReadBytes(nameLength);

            if (nameBytes.Length == 1)
            {
                if (nameBytes[0] == 0)
                    _name = ".";
                else if (nameBytes[0] == 1)
                    _name = "..";
            }
            else
            {
                _name = Encoding.ASCII.GetString(nameBytes).Split(';')[0];
            }

            bool isDirectory = (_flags & (int)RecordFlags.DirectoryRecord) == (int)RecordFlags.DirectoryRecord;
            bool isNotParentOrGrandparentDirectory = nameLength != 1 || _parent == null;

            if (isDirectory && isNotParentOrGrandparentDirectory)
            {
                reader.Seek(CVMFile.CVM_HEADER_SIZE + ((long)_LBA * CVMFile.ISO_BLOCKSIZE), SeekOrigin.Begin);
                _subEntries = new List<ISODirectoryRecord>();

                // Set the initial sector start position
                long posSubEntriesSectorStart = reader.BaseStream.Position;
                long posSubEntriesDataEnd = posSubEntriesSectorStart + _size;

                while (reader.BaseStream.Position < posSubEntriesDataEnd)
                {
                    ISODirectoryRecord record = new ISODirectoryRecord(reader, this);

                    _subEntries.Add(record);

                    // Skip padding
                    byte test = reader.ReadByte();
                    while (test == 0 && reader.BaseStream.Position < posSubEntriesDataEnd)
                    {
                        test = reader.ReadByte();
                    }

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
