namespace AtlusLibSharp.FileSystems.ListArchive
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using IO;
    using PAKToolArchive;

    public sealed class ListArchiveFile : BinaryFileBase
    {
        // Fields
        private List<ListArchiveFileEntry> _entries;

        // Constructors
        public ListArchiveFile(string path)
        {
            if (!VerifyFileType(path))
            {
                throw new InvalidDataException("Not a valid ListArchiveFile.");
            }

            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                InternalRead(reader);
            }
        }

        public ListArchiveFile(Stream stream)
        {
            if (!VerifyFileType(stream))
            {
                throw new InvalidDataException("Not a valid ListArchiveFile.");
            }

            using (BinaryReader reader = new BinaryReader(stream))
            {
                InternalRead(reader);
            }
        }

        public ListArchiveFile(string[] filepaths)
        {
            _entries = new List<ListArchiveFileEntry>(filepaths.Length);
            foreach (string path in filepaths)
            {
                _entries.Add(new ListArchiveFileEntry(path));
            }
        }

        public ListArchiveFile()
        {
            _entries = new List<ListArchiveFileEntry>();
        }

        // Properties
        public int EntryCount
        {
            get { return _entries.Count; }
        }

        public List<ListArchiveFileEntry> Entries
        {
            get { return _entries; }
        }

        // static methods
        public static ListArchiveFile Create(string directorypath)
        {
            return new ListArchiveFile(Directory.GetFiles(directorypath));
        }

        public static ListArchiveFile Create(PAKToolArchiveFile pak)
        {
            ListArchiveFile arc = new ListArchiveFile();
            foreach (PAKToolArchiveEntry entry in pak.Entries)
            {
                arc.Entries.Add(new ListArchiveFileEntry(entry.Name, entry.Data));
            }
            return arc;
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
            // check stream length
            if (stream.Length <= 4 + ListArchiveFileEntry.NAME_LENGTH + 4)
                return false;

            byte[] testData = new byte[4 + ListArchiveFileEntry.NAME_LENGTH + 4];
            stream.Read(testData, 0, 4 + ListArchiveFileEntry.NAME_LENGTH + 4);
            stream.Position = 0;

            int numOfFiles = BitConverter.ToInt32(testData, 0);

            // num of files sanity check
            if (numOfFiles > 1024 || numOfFiles < 1)
                return false;

            // check if the name field is correct
            bool nameTerminated = false;
            for (int i = 0; i < ListArchiveFileEntry.NAME_LENGTH; i++)
            {
                if (testData[4 + i] == 0x00)
                    nameTerminated = true;

                if (testData[4 + i] != 0x00 && nameTerminated == true)
                    return false;
            }

            // first entry length sanity check
            int length = BitConverter.ToInt32(testData, 4 + ListArchiveFileEntry.NAME_LENGTH);
            if (length >= (1024 * 1024 * 100) || length < 0)
            {
                return false;
            }

            return true;
        }

        // instance methods
        internal override void InternalWrite(BinaryWriter writer)
        {
            writer.Write(_entries.Count);
            foreach (ListArchiveFileEntry entry in _entries)
            {
                entry.InternalWrite(writer);
            }
        }

        private void InternalRead(BinaryReader reader)
        {
            int numEntries = reader.ReadInt32();
            _entries = new List<ListArchiveFileEntry>(numEntries);
            for (int i = 0; i < numEntries; i++)
            {
                _entries.Add(new ListArchiveFileEntry(reader));
            }
        }
    }
}
