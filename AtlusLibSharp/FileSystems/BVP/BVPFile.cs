namespace AtlusLibSharp.FileSystems.BVP
{
    using System.IO;
    using System.Collections.Generic;
    using AtlusLibSharp.Utilities;
    using IO;

    /// <summary>
    /// Class containing all functionality required in order to load and create *.BVP archives.
    /// Used in Persona 3 and 4 for storing battle dialog.
    /// </summary>
    public sealed class BVPFile : BinaryFileBase
    {
        /**********************/
        /**** Constructors ****/
        /**********************/

        /// <summary>
        /// Loads a battle voice package (*.bvp) file from the given path.
        /// </summary>
        /// <param name="path">Path of the *.bvp file to load.</param>
        public BVPFile(string path)
        {
            Entries = new List<BVPEntry>();
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                InternalRead(reader);
            }
        }

        /// <summary>
        /// Loads a battle voice package from the given stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="leaveStreamOpen"></param>
        public BVPFile(Stream stream, bool leaveStreamOpen)
        {
            Entries = new List<BVPEntry>();

            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveStreamOpen))
            {
                InternalRead(reader);
            }
        }

        /// <summary>
        /// Initializes a new, empty battle voice package.
        /// </summary>
        public BVPFile()
        {
            Entries = new List<BVPEntry>();
        }

        /********************/
        /**** Properties ****/
        /********************/

        /// <summary>
        /// Gets the number of entries.
        /// </summary>
        public int EntryCount
        {
            get { return Entries.Count; }
        }

        /// <summary>
        /// Gets the list of entries in the battle voice package.
        /// </summary>
        public List<BVPEntry> Entries { get; }

        /*****************/
        /**** Methods ****/
        /*****************/

        /// <summary>
        /// Creates a new battle voice package from the files located at the given path.
        /// </summary>
        /// <param name="directorypath">Path where the files to pack into the file are located.</param>
        /// <returns>A newly created <see cref="BVPFile"/>.</returns>
        public static BVPFile Create(string directorypath)
        {
            BVPFile bvp = new BVPFile();
            string[] filepaths = Directory.GetFiles(directorypath);

            foreach (string item in filepaths)
                bvp.Entries.Add(new BVPEntry(item));

            return bvp;
        }

        /// <summary>
        /// Extracts the entries to a folder at the specified path.
        /// </summary>
        /// <param name="path">Folder path to extract the entries to.</param>
        public void Extract(string path)
        {
            Directory.CreateDirectory(path);

            for (int i = 0; i < EntryCount; i++)
            {
                string fileName = "Entry" + i.ToString("D3") + ".BMD";
                File.WriteAllBytes(Path.Combine(path, fileName), Entries[i].Data);
            }
        }

        internal override void InternalWrite(BinaryWriter writer)
        {
            // Save the position of the file in the stream
            long posFileStart = writer.GetPosition();

            // Precalculate the first offset
            long posPrevDataEnd = (Entries.Count + 1) * BVPEntry.SIZE;

            // Write the entry table
            for (int i = 0; i < Entries.Count; i++)
            {
                // The offset is the position where the last data write ended, aligned to the 16 byte boundary
                Entries[i].FileDataOffset = (int)AlignmentHelper.Align(posPrevDataEnd, 16);

                // Write table entry
                Entries[i].WriteEntryHeader(writer);

                // Write data at offset
                writer.Write(Entries[i].Data, posFileStart + Entries[i].FileDataOffset);

                // Update the previous data end position
                posPrevDataEnd = Entries[i].FileDataOffset + Entries[i].DataLength;
            }

            // Write empty terminator entry
            writer.Write(0, BVPEntry.SIZE);

            // Seek to the last data write position, and align the file to 64 bytes
            writer.BaseStream.Seek(posPrevDataEnd, SeekOrigin.Begin);
            writer.AlignPosition(64);
        }

        private void InternalRead(BinaryReader reader)
        {
            // Read first entry
            BVPEntry entry = new BVPEntry(reader);

            // Loop while we haven't read an empty entry
            while (entry.DataLength != 0)
            {
                // Entry wasn't empty, so add it to the list
                Entries.Add(entry);

                // Read next entry
                entry = new BVPEntry(reader);
            }
        }
    }
}
