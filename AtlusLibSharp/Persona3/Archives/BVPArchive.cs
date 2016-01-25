namespace AtlusLibSharp.Persona3.Archives
{
    using System.IO;
    using System.Collections.Generic;
    using Utilities;

    /// <summary>
    /// Class containing all functionality required in order to load and create *.BVP archives.
    /// Used in Persona 3 and 4 for storing battle dialog.
    /// </summary>
    public class BVPArchive : BinaryFileBase
    {
        // Fields
        private List<BVPArchiveEntry> _entryTable;

        /// <summary>
        /// Creates a new BVPFile from the given path.
        /// </summary>
        /// <param name="folderPackingMode">If true, pack a folder into a .bvp pointed to by the path or else load a .bvp file from the path.</param>
        public BVPArchive(string path, bool folderPackingMode = false)
        {
            _entryTable = new List<BVPArchiveEntry>();

            if (folderPackingMode)
            {
                string[] filePaths = Directory.GetFiles(path);

                for (int i = 0; i < filePaths.Length; i++)
                {
                    _entryTable.Add(new BVPArchiveEntry(filePaths[i]));
                }
            }
            else
            {
                using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                {
                    Read(reader);
                }
            }
        }

        public BVPArchive(Stream stream)
        {
            _entryTable = new List<BVPArchiveEntry>();
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Read(reader);
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
            set { _entryTable = value; }
        }

        // Methods
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
            long posPrevDataEnd = GetEntryTableSize();

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
                posPrevDataEnd = _entryTable[i].Offset + _entryTable[i].Length;
            }

            // Write empty terminator entry
            writer.Write(0, BVPArchiveEntry.ENTRY_SIZE);

            // Seek to the last data write position, and align the file to 64 bytes
            writer.BaseStream.Seek(posPrevDataEnd, SeekOrigin.Begin);
            writer.AlignPosition(64);
        }

        private void Read(BinaryReader reader)
        {
            // Read first entry
            BVPArchiveEntry entry = new BVPArchiveEntry(reader);

            // Loop while we haven't read an empty entry
            while (entry.Length != 0)
            {
                // Entry wasn't empty, so add it to the list
                _entryTable.Add(entry);

                // Read next entry
                entry = new BVPArchiveEntry(reader);
            }
        }

        private int GetEntryTableSize()
        {
            return (_entryTable.Count + 1) * BVPArchiveEntry.ENTRY_SIZE;
        }
    }
}
