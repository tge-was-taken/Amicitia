namespace AtlusLibSharp.FileSystems.AMD
{
    using IO;
    using System.Collections.Generic;
    using System.IO;
    using Utilities;

    public class AMDFile : BinaryFileBase
    {
        /****************/
        /* Private data */
        /****************/
        // constants
        private const int MAGIC_LENGTH = 4;
        private const string MAGIC = "CHNK";

        // members
        private List<AMDChunk> m_chunkList;

        /**************/
        /* Properties */
        /**************/
        /// <summary>
        /// Gets the number of chunks contained in the <see cref="AMDFile"/>.
        /// </summary>
        public int ChunkCount
        {
            get { return m_chunkList.Count; }
        }

        /// <summary>
        /// Gets the list of chunks contained in the <see cref="AMDFile"/>.
        /// </summary>
        public List<AMDChunk> Chunks
        {
            get { return m_chunkList; }
        }

        /****************/
        /* Constructors */
        /****************/
        /// <summary>
        /// Creates a new, empty <see cref="AMDFile"/>.
        /// </summary>
        public AMDFile()
        {
            m_chunkList = new List<AMDChunk>();
        }

        /// <summary>
        /// Creates a new <see cref="AMDFile"/> by reading the AMD file at the given path.
        /// </summary>
        /// <param name="path">The path pointing to the the file to load.</param>
        public AMDFile(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                InternalRead(reader);
            }
        }

        /// <summary>
        /// Creates a new <see cref="AMDFile"/> by reading the AMD file from the given stream.
        /// </summary>
        /// <param name="path">The path pointing to the the file to load.</param>
        public AMDFile(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                InternalRead(reader);
            }
        }

        /// <summary>
        /// Creates a new <see cref="AMDFile"/> by reading it from the <see cref="Stream"/> using the <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader"></param>
        internal AMDFile(BinaryReader reader)
        {
            InternalRead(reader);
        }

        /************************/
        /* Read / Write methods */
        /************************/
        // read the amd file data using the binary reader
        private void InternalRead(BinaryReader reader)
        {
            // read magic & verify
            string magic = reader.ReadCString(MAGIC_LENGTH);

            if (magic != MAGIC)
            {
                throw new InvalidDataException(string.Format("Expected magic string {0}. Got {1}.", MAGIC, magic));
            }

            // read chunks
            int numChunks = reader.ReadInt32();
            m_chunkList = new List<AMDChunk>(numChunks);

            for (int i = 0; i < numChunks; i++)
            {
                m_chunkList.Add(new AMDChunk(reader));
            }
        }

        // write the data using the binary writer
        internal override void InternalWrite(BinaryWriter writer)
        {
            // write magic
            writer.WriteCString(MAGIC, MAGIC_LENGTH);

            // write chunk list
            writer.Write(ChunkCount);
            foreach (AMDChunk chunk in Chunks)
            {
                chunk.InternalWrite(writer);
            }
        }
    }
}
