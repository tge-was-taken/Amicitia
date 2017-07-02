using System.Text;

namespace AtlusLibSharp.FileSystems.ListArchive
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using IO;
    using PAKToolArchive;

    public sealed class ListArchiveFile : BinaryBase, ISimpleArchiveFile
    {
        // Fields
        private List<ListArchiveEntry> mEntries;

        // Constructors
        public ListArchiveFile(string path)
        {
            if (!VerifyFileType(path))
            {
                throw new InvalidDataException("Not a valid ListArchiveFile.");
            }

            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                Read(reader);
            }
        }

        public ListArchiveFile(Stream stream, bool leaveOpen = false)
        {
            if (!VerifyFileType(stream))
            {
                throw new InvalidDataException("Not a valid ListArchiveFile.");
            }

            using (BinaryReader reader = new BinaryReader(stream, Encoding.Default, leaveOpen))
            {
                Read(reader);
            }
        }

        public ListArchiveFile(string[] filepaths)
        {
            mEntries = new List<ListArchiveEntry>(filepaths.Length);
            foreach (string path in filepaths)
            {
                mEntries.Add(new ListArchiveEntry(path));
            }
        }

        public ListArchiveFile()
        {
            mEntries = new List<ListArchiveEntry>();
        }

        // Properties
        public int EntryCount
        {
            get { return mEntries.Count; }
        }

        ISimpleArchiveFile ISimpleArchiveFile.Create(IEnumerable<IArchiveEntry> entries)
        {
            var file = new ListArchiveFile();
            foreach (var archiveEntry in entries)
            {
                file.Entries.Add(new ListArchiveEntry(archiveEntry.Name, archiveEntry.Data));
            }

            return file;
        }

        public List<ListArchiveEntry> Entries
        {
            get { return mEntries; }
        }

        IEnumerable<IArchiveEntry> ISimpleArchiveFile.Entries => Entries;

        // static methods
        public static ListArchiveFile Create(string directorypath)
        {
            return new ListArchiveFile(Directory.GetFiles(directorypath));
        }

        public static ListArchiveFile Create(PakToolArchiveFile pak)
        {
            ListArchiveFile arc = new ListArchiveFile();
            foreach (PakToolArchiveEntry entry in pak.Entries)
            {
                arc.Entries.Add(new ListArchiveEntry(entry.Name, entry.Data));
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
            if (stream.Length <= 4 + ListArchiveEntry.NAME_LENGTH + 4)
                return false;

            byte[] testData = new byte[4 + ListArchiveEntry.NAME_LENGTH + 4];
            stream.Read(testData, 0, 4 + ListArchiveEntry.NAME_LENGTH + 4);
            stream.Position = 0;

            int numOfFiles = BitConverter.ToInt32(testData, 0);

            // num of files sanity check
            if (numOfFiles > 1024 || numOfFiles < 1)
                return false;

            // check if the name field is correct
            bool nameTerminated = false;
            for (int i = 0; i < ListArchiveEntry.NAME_LENGTH; i++)
            {
                if (testData[4 + i] == 0x00)
                    nameTerminated = true;

                if (testData[4 + i] != 0x00 && nameTerminated == true)
                    return false;
            }

            // first entry length sanity check
            int length = BitConverter.ToInt32(testData, 4 + ListArchiveEntry.NAME_LENGTH);
            if (length >= (1024 * 1024 * 100) || length < 0)
            {
                return false;
            }

            return true;
        }

        // instance methods
        internal override void Write(BinaryWriter writer)
        {
            writer.Write(mEntries.Count);
            foreach (ListArchiveEntry entry in mEntries)
            {
                entry.InternalWrite(writer);
            }
        }

        private void Read(BinaryReader reader)
        {
            int numEntries = reader.ReadInt32();
            mEntries = new List<ListArchiveEntry>(numEntries);
            for (int i = 0; i < numEntries; i++)
            {
                mEntries.Add(new ListArchiveEntry(reader));
            }
        }
    }
}
