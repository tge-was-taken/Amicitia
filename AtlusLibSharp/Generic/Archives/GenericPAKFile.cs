namespace AtlusLibSharp.Generic.Archives
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Utilities;

    /// <summary>
    /// Class containing all functionality required in order to load and create *.BIN archives.
    /// Used in Persona 3, 4 and later Atlus games as a generic file container.
    /// Other used extensions are *.F00, *.F01, *.P00, *.P01, *.PAK
    /// </summary>
    public class GenericPAK : BinaryFileBase
    {
        // Fields
        private List<GenericPAKEntry> _entries;

        // Constructors
        public GenericPAK(string path) 
            : this(File.OpenRead(path), 252)
        {
        }

        public GenericPAK(string path, int nameLength) 
            : this(File.OpenRead(path), nameLength)
        {
        }

        public GenericPAK(Stream stream, int nameLength)
        {
            _entries = new List<GenericPAKEntry>();
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Read(reader, nameLength);
            }
        }

        public GenericPAK(string[] filepaths)
        {
            _entries = new List<GenericPAKEntry>(filepaths.Length);

            for (int i = 0; i < filepaths.Length; i++)
            {
                _entries.Add(new GenericPAKEntry(filepaths[i]));
            }
        }

        public GenericPAK()
        {
            _entries = new List<GenericPAKEntry>();
        }

        // Properties
        public int EntryCount
        {
            get { return _entries.Count; }
        }

        public List<GenericPAKEntry> Entries
        {
            get { return _entries; }
            set { _entries = value; }
        }

        // Methods
        internal override void InternalWrite(BinaryWriter writer)
        {
            foreach (GenericPAKEntry entry in _entries)
            {
                entry.InternalWrite(writer);
            }

            // Write terminator entry
            writer.Write(0, GenericPAKEntry.NAME_LENGTH + 4);
        }

        private void Read(BinaryReader reader, int nameLength = 252)
        {
            GenericPAKEntry.NAME_LENGTH = nameLength;

            // Read first entry
            GenericPAKEntry entry = new GenericPAKEntry(reader);

            while (entry.Length != 0)
            {
                // Entry wasn't empty, so add a new one to the list
                _entries.Add(entry);

                if (reader.BaseStream.Position == reader.BaseStream.Length)
                {
                    break;
                }

                // Read next entry
                entry = new GenericPAKEntry(reader);
            }
        }
    }
}
