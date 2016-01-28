namespace AtlusLibSharp.Persona3.Archives
{
    using System.IO;
    using System.Collections.Generic;
    using Utilities;
    using System;

    /// <summary>
    /// Class containing all functionality required in order to load and create *.BVP archives.
    /// Used in Persona 3 and 4 for storing battle dialog.
    /// </summary>
    public class BVPArchive : BinaryFileBase, IArchive
    {
        // Fields
        private List<BVPArchiveEntry> _entryTable;

        // Constructors
        public BVPArchive(string path)
        {
            _entryTable = new List<BVPArchiveEntry>();
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                InternalRead(reader);
            }
        }

        public BVPArchive(Stream stream)
        {
            _entryTable = new List<BVPArchiveEntry>();
            using (BinaryReader reader = new BinaryReader(stream))
            {
                InternalRead(reader);
            }
        }

        public BVPArchive()
        {
            _entryTable = new List<BVPArchiveEntry>();
        }

        // Properties
        public int EntryCount
        {
            get { return _entryTable.Count; }
        }

        public List<BVPArchiveEntry> Entries
        {
            get { return _entryTable; }
        }

        // Static methods
        public static BVPArchive Create(string directorypath)
        {
            BVPArchive bvp = new BVPArchive();
            string[] filepaths = Directory.GetFiles(directorypath);
            foreach (string item in filepaths)
            {
                bvp.Entries.Add(new BVPArchiveEntry(item));
            }
            return bvp;
        }

        // Instance Methods
        public void Extract(string path)
        {
            Directory.CreateDirectory(path);

            for (int i = 0; i < EntryCount; i++)
            {
                string fileName = "Entry" + i.ToString("D3") + ".BMD";
                File.WriteAllBytes(Path.Combine(path, fileName), _entryTable[i].Data);
            }
        }

        internal override void InternalWrite(BinaryWriter writer)
        {
            // Save the position of the file in the stream
            long posFileStart = writer.GetPosition();

            // Precalculate the first offset
            long posPrevDataEnd = (_entryTable.Count + 1) * BVPArchiveEntry.ENTRY_SIZE;

            // Write the entry table
            for (int i = 0; i < _entryTable.Count; i++)
            {
                // The offset is the position where the last data write ended, aligned to the 16 byte boundary
                _entryTable[i].Offset = (int)AlignmentHelper.Align(posPrevDataEnd, 16);

                // Write table entry
                _entryTable[i].WriteEntry(writer);

                // Write data at offset
                writer.Write(_entryTable[i].Data, posFileStart + _entryTable[i].Offset);

                // Update the previous data end position
                posPrevDataEnd = _entryTable[i].Offset + _entryTable[i].DataLength;
            }

            // Write empty terminator entry
            writer.Write(0, BVPArchiveEntry.ENTRY_SIZE);

            // Seek to the last data write position, and align the file to 64 bytes
            writer.BaseStream.Seek(posPrevDataEnd, SeekOrigin.Begin);
            writer.AlignPosition(64);
        }

        private void InternalRead(BinaryReader reader)
        {
            // Read first entry
            BVPArchiveEntry entry = new BVPArchiveEntry(reader);

            // Loop while we haven't read an empty entry
            while (entry.DataLength != 0)
            {
                // Entry wasn't empty, so add it to the list
                _entryTable.Add(entry);

                // Read next entry
                entry = new BVPArchiveEntry(reader);
            }
        }

        IArchiveEntry IArchive.GetEntry(int index)
        {
            return _entryTable[index];
        }
    }
}
