using System.Linq;
using System.Text;

namespace AmicitiaLibrary.FileSystems.PAKToolArchive
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AmicitiaLibrary.Utilities;
    using ListArchive;
    using IO;

    /// <summary>
    /// Class containing all functionality required in order to load and create *.PAK archives.
    /// Used in Persona 3, 4 and later Atlus games as a generic file container.
    /// Other used extensions are *.F00, *.F01, *.P00, *.P01, *.BIN and others
    /// </summary>
    public sealed class PakToolArchiveFile : BinaryBase, ISimpleArchiveFile
    {
        /**********************/
        /**** Constructors ****/
        /**********************/

        /// <summary>
        /// Loads an archive from the given path.
        /// </summary>
        /// <param name="path">Path of the file to load.</param>
        public PakToolArchiveFile(string path) 
        {
            using (FileStream stream = File.OpenRead(path))
            {
                /*
                if (!InternalVerifyFileType(stream))
                {
                    throw new InvalidDataException("Not a valid file");
                }
                */

                Entries = new List<PakToolArchiveEntry>();
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    Read(reader);
                }
            }
        }

        /// <summary>
        /// Loads an archive from the stream.
        /// </summary>
        /// <param name="stream">Stream to load the archive from.</param>
        public PakToolArchiveFile(Stream stream, bool leaveOpen = false)
        {
            /*
            if (!InternalVerifyFileType(stream))
            {
                throw new InvalidDataException("Not a valid file");
            }
            */

            Entries = new List<PakToolArchiveEntry>();
            using (BinaryReader reader = new BinaryReader(stream, Encoding.Default, leaveOpen))
            {
                Read(reader);
            }
        }

        /// <summary>
        /// Create a new archive from a list of file paths representing archive entries.
        /// </summary>
        /// <param name="filepaths">List of file paths pointing to files to be loaded as entries.</param>
        public PakToolArchiveFile(IList<string> filepaths)
        {
            Entries = new List<PakToolArchiveEntry>(filepaths.Count);

            for (int i = 0; i < filepaths.Count; i++)
            {
                Entries.Add(new PakToolArchiveEntry(filepaths[i]));
            }
        }

        /// <summary>
        /// Creates a new, empty archive.
        /// </summary>
        public PakToolArchiveFile()
        {
            Entries = new List<PakToolArchiveEntry>();
        }

        /********************/
        /**** Properties ****/
        /********************/

        /// <summary>
        /// Gets the number of entries in the archive.
        /// </summary>
        public int EntryCount
        {
            get { return Entries.Count; }
        }

        ISimpleArchiveFile ISimpleArchiveFile.Create(IEnumerable<IArchiveEntry> entries)
        {
            var file = new PakToolArchiveFile();
            foreach (var entry in entries)
            {
                file.Entries.Add(new PakToolArchiveEntry(entry.Name, entry.Data));
            }

            return file;
        }

        /// <summary>
        /// Gets the list of archive entries.
        /// </summary>
        public List<PakToolArchiveEntry> Entries { get; }

        IEnumerable<IArchiveEntry> ISimpleArchiveFile.Entries => Entries;

        /*****************/
        /**** Methods ****/
        /*****************/

        /// <summary>
        /// Create a new archive file using the files in the specified folder.
        /// </summary>
        /// <param name="directorypath">Path to the directory containing the files to load as entries.</param>
        /// <returns>A newly created archive.</returns>
        public static PakToolArchiveFile Create(string directorypath)
        {
            return new PakToolArchiveFile(Directory.GetFiles(directorypath));
        }

        /// <summary>
        /// Create a new archive file using a <see cref="ListArchiveFile"/>.
        /// </summary>
        /// <param name="arc">The <see cref="ListArchiveFile"/> to load into the <see cref="PakToolArchiveFile"/>.</param>
        /// <returns>A newly created archive.</returns>
        public static PakToolArchiveFile Create(ListArchiveFile arc)
        {
            PakToolArchiveFile pak = new PakToolArchiveFile();

            for (int i = 0; i < arc.EntryCount; i++)
            {
                ListArchiveEntry arcEntry = arc.Entries[i];
                pak.Entries.Add(new PakToolArchiveEntry(arcEntry.Name, arcEntry.Data));
            }

            return pak;
        }

        /// <summary>
        /// Verifies that the file at the path is a valid <see cref="PakToolArchiveFile"/>.
        /// </summary>
        public static bool VerifyFileType(string path)
        {
            return InternalVerifyFileType(File.OpenRead(path));
        }

        /// <summary>
        /// Verifies that the stream is of a valid <see cref="PakToolArchiveFile"/>.
        /// </summary>
        public static bool VerifyFileType(Stream stream)
        {
            return InternalVerifyFileType(stream);
        }

        internal override void Write(BinaryWriter writer)
        {
            foreach (PakToolArchiveEntry entry in Entries)
            {
                entry.InternalWrite(writer);
            }

            // Write terminator entry
            writer.Write(0, PakToolArchiveEntry.MAX_NAME_LENGTH + 4);
        }

        private static bool InternalVerifyFileType(Stream stream)
        {
            // check if the file is too small to be a proper pak file
            if (stream.Length <= PakToolArchiveEntry.MAX_NAME_LENGTH + 4)
            {
                return false;
            }

            // read some test data
            byte[] testData = new byte[PakToolArchiveEntry.MAX_NAME_LENGTH + 4];
            stream.Read(testData, 0, PakToolArchiveEntry.MAX_NAME_LENGTH + 4);
            stream.Position = 0;

            // check if first byte is zero, if so then no name can be stored thus making the file corrupt
            if (testData[0] == 0x00)
                return false;

            bool nameTerminated = false;
            for (int i = 0; i < PakToolArchiveEntry.MAX_NAME_LENGTH; i++)
            {
                if (testData[i] == 0x00)
                    nameTerminated = true;

                // If the name has already been terminated but there's still data in the reserved space,
                // fail the test
                if (nameTerminated == true && testData[i] != 0x00)
                    return false;
            }

            int testLength = BitConverter.ToInt32(testData, PakToolArchiveEntry.MAX_NAME_LENGTH);

            // sanity check, if the length of the first file is >= 100 mb, fail the test
            if (testLength >= (1024 * 1024 * 100) || testLength < 0)
            {
                return false;
            }

            return true;
        }

        private void Read(BinaryReader reader)
        {
            // Read first entry
            PakToolArchiveEntry entry = new PakToolArchiveEntry(reader);

            while (entry.DataLength != 0)
            {
                // Entry wasn't empty, so add a new one to the list
                Entries.Add(entry);

                if (reader.BaseStream.Position == reader.BaseStream.Length)
                {
                    break;
                }

                // Read next entry
                entry = new PakToolArchiveEntry(reader);
            }
        }
    }
}
