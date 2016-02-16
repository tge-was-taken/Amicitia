namespace AtlusLibSharp.Common.FileSystem.Archives
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Common.Utilities;

    /// <summary>
    /// Class containing all functionality required in order to load and create *.BIN archives.
    /// Used in Persona 3, 4 and later Atlus games as a generic file container.
    /// Other used extensions are *.F00, *.F01, *.P00, *.P01, *.PAK
    /// </summary>
    public class PAKToolFile : BinaryFileBase, IArchive
    {
        // Fields
        private List<PAKToolFileEntry> _entries;

        // Constructors
        public PAKToolFile(string path) 
            : this(File.OpenRead(path))
        {
        }

        public PAKToolFile(Stream stream)
        {
            if (!InternalVerifyFileType(stream))
            {
                throw new InvalidDataException("Not a valid PAKToolFile");
            }

            _entries = new List<PAKToolFileEntry>();
            using (BinaryReader reader = new BinaryReader(stream))
            {
                InternalRead(reader);
            }
        }

        public PAKToolFile(string[] filepaths)
        {
            _entries = new List<PAKToolFileEntry>(filepaths.Length);

            for (int i = 0; i < filepaths.Length; i++)
            {
                _entries.Add(new PAKToolFileEntry(filepaths[i]));
            }
        }

        public PAKToolFile()
        {
            _entries = new List<PAKToolFileEntry>();
        }

        // Properties
        public int EntryCount
        {
            get { return _entries.Count; }
        }

        public List<PAKToolFileEntry> Entries
        {
            get { return _entries; }
            set { _entries = value; }
        }

        // static methods
        public static PAKToolFile Create(string directorypath)
        {
            return new PAKToolFile(Directory.GetFiles(directorypath));
        }

        public static PAKToolFile Create(ListArchiveFile arc)
        {
            PAKToolFile pak = new PAKToolFile();
            for (int i = 0; i < arc.EntryCount; i++)
            {
                ListArchiveFileEntry arcEntry = arc.Entries[i];
                pak.Entries.Add(new PAKToolFileEntry(arcEntry.Name, arcEntry.Data));
            }
            return pak;
        }

        public static bool VerifyFileType(string path)
        {
            return InternalVerifyFileType(File.OpenRead(path));
        }

        public static bool VerifyFileType(Stream stream)
        {
            return InternalVerifyFileType(stream);
        }

        private static bool InternalVerifyFileType(Stream stream)
        {
            // check if the file is too small to be a proper pak file
            if (stream.Length <= PAKToolFileEntry.NAME_LENGTH + 4)
            {
                return false;
            }

            // read some test data
            byte[] testData = new byte[PAKToolFileEntry.NAME_LENGTH + 4];
            stream.Read(testData, 0, PAKToolFileEntry.NAME_LENGTH + 4);
            stream.Position = 0;

            // check if first byte is zero, if so then no name can be stored thus making the file corrupt
            if (testData[0] == 0x00)
                return false;

            bool nameTerminated = false;
            for (int i = 0; i < PAKToolFileEntry.NAME_LENGTH; i++)
            {
                if (testData[i] == 0x00)
                    nameTerminated = true;

                // If the name has already been terminated but there's still data in the reserved space,
                // fail the test
                if (nameTerminated == true && testData[i] != 0x00)
                    return false;
            }

            int testLength = BitConverter.ToInt32(testData, PAKToolFileEntry.NAME_LENGTH);

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
            foreach (PAKToolFileEntry entry in _entries)
            {
                entry.InternalWrite(writer);
            }

            // Write terminator entry
            writer.Write(0, PAKToolFileEntry.NAME_LENGTH + 4);
        }

        private void InternalRead(BinaryReader reader)
        {
            // Read first entry
            PAKToolFileEntry entry = new PAKToolFileEntry(reader);

            while (entry.DataLength != 0)
            {
                // Entry wasn't empty, so add a new one to the list
                _entries.Add(entry);

                if (reader.BaseStream.Position == reader.BaseStream.Length)
                {
                    break;
                }

                // Read next entry
                entry = new PAKToolFileEntry(reader);
            }
        }

        IArchiveEntry IArchive.GetEntry(int index)
        {
            return _entries[index];
        }
    }
}
