namespace AtlusLibSharp.Generic.Archives
{
    using Persona3.Archives;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Utilities;

    /// <summary>
    /// Class containing all functionality required in order to load and create *.BIN archives.
    /// Used in Persona 3, 4 and later Atlus games as a generic file container.
    /// Other used extensions are *.F00, *.F01, *.P00, *.P01, *.PAK
    /// </summary>
    public class GenericPAK : BinaryFileBase, IArchive
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
            if (!InternalVerifyFileType(stream))
            {
                throw new InvalidDataException("Not a valid GenericPAK");
            }

            _entries = new List<GenericPAKEntry>();
            using (BinaryReader reader = new BinaryReader(stream))
            {
                InternalRead(reader, nameLength);
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

        // static methods
        public static bool VerifyFileType(string path)
        {
            return InternalVerifyFileType(File.OpenRead(path));
        }

        public static bool VerifyFileType(Stream stream)
        {
            return InternalVerifyFileType(stream);
        }

        public static GenericPAK Create(string directorypath)
        {
            return new GenericPAK(Directory.GetFiles(directorypath));
        }

        public static GenericPAK From(GenericVitaArchive arc)
        {
            GenericPAK pak = new GenericPAK();
            for (int i = 0; i < arc.EntryCount; i++)
            {
                GenericVitaArchiveEntry arcEntry = arc.Entries[i];
                pak.Entries.Add(new GenericPAKEntry(arcEntry.Name, arcEntry.Data));
            }
            return pak;
        }

        private static bool InternalVerifyFileType(Stream stream)
        {
            // check if the file is too small to be a proper pak file
            if (stream.Length <= GenericPAKEntry.NAME_LENGTH + 4)
            {
                return false;
            }

            // read some test data
            byte[] testData = new byte[GenericPAKEntry.NAME_LENGTH + 4];
            stream.Read(testData, 0, GenericPAKEntry.NAME_LENGTH + 4);
            stream.Position = 0;

            // check if first byte is zero, if so then no name can be stored thus making the file corrupt
            if (testData[0] == 0x00)
                return false;

            bool nameTerminated = false;
            for (int i = 0; i < GenericPAKEntry.NAME_LENGTH; i++)
            {
                if (testData[i] == 0x00)
                    nameTerminated = true;

                // If the name has already been terminated but there's still data in the reserved space,
                // fail the test
                if (nameTerminated == true && testData[i] != 0x00)
                    return false;
            }

            int testLength = BitConverter.ToInt32(testData, GenericPAKEntry.NAME_LENGTH - 1);

            // sanity check, if the length of the first file is >= 100 mb, fail the test
            if (testLength >= (1024 * 1024 * 100) || testLength < 0)
            {
                return false;
            }

            return true;
        }

        // instance methods
        internal override void InternalWrite(BinaryWriter writer)
        {
            foreach (GenericPAKEntry entry in _entries)
            {
                entry.InternalWrite(writer);
            }

            // Write terminator entry
            writer.Write(0, GenericPAKEntry.NAME_LENGTH + 4);
        }

        private void InternalRead(BinaryReader reader, int nameLength)
        {
            // Read first entry
            GenericPAKEntry entry = new GenericPAKEntry(reader);

            while (entry.DataLength != 0)
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

        IArchiveEntry IArchive.GetEntry(int index)
        {
            return _entries[index];
        }
    }
}
