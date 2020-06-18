namespace AmicitiaLibrary.FileSystems.AMD
{
    using IO;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Utilities;

    public class AmdFile : BinaryBase
    {
        /****************/
        /* Private data */
        /****************/
        // constants
        private const int MAGIC_LENGTH = 4;
        private const string MAGIC = "CHNK";

        // members
        private List<AmdChunk> mChunkList;

        /**************/
        /* Properties */
        /**************/
        /// <summary>
        /// Gets the number of chunks contained in the <see cref="AmdFile"/>.
        /// </summary>
        public int ChunkCount
        {
            get { return mChunkList.Count; }
        }

        /// <summary>
        /// Gets the list of chunks contained in the <see cref="AmdFile"/>.
        /// </summary>
        public List<AmdChunk> Chunks
        {
            get { return mChunkList; }
        }

        /****************/
        /* Constructors */
        /****************/
        /// <summary>
        /// Creates a new, empty <see cref="AmdFile"/>.
        /// </summary>
        public AmdFile()
        {
            mChunkList = new List<AmdChunk>();
        }

        /// <summary>
        /// Creates a new <see cref="AmdFile"/> by reading the AMD file at the given path.
        /// </summary>
        /// <param name="path">The path pointing to the the file to load.</param>
        public AmdFile(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                Read(reader);
            }
        }

        /// <summary>
        /// Creates a new <see cref="AmdFile"/> by reading the AMD file from the given stream.
        /// </summary>
        /// <param name="path">The path pointing to the the file to load.</param>
        public AmdFile(Stream stream, bool leaveOpen = false)
        {
            using (BinaryReader reader = new BinaryReader( stream, Encoding.Default, leaveOpen ) )
            {
                Read(reader);
            }
        }

        /// <summary>
        /// Creates a new <see cref="AmdFile"/> by reading it from the <see cref="Stream"/> using the <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader"></param>
        internal AmdFile(BinaryReader reader)
        {
            Read(reader);
        }

        /************************/
        /* Read / Write methods */
        /************************/
        // read the amd file data using the binary reader
        private void Read(BinaryReader reader)
        {
            // read magic & verify
            string magic = reader.ReadCString(MAGIC_LENGTH);

            if (magic != MAGIC)
            {
                throw new InvalidDataException($"Expected magic string {MAGIC}. Got {magic}.");
            }

            // read chunks
            int numChunks = reader.ReadInt32();
            mChunkList = new List<AmdChunk>(numChunks);

            for (int i = 0; i < numChunks; i++)
            {
                mChunkList.Add(new AmdChunk(reader));
            }
        }

        // write the data using the binary writer
        internal override void Write(BinaryWriter writer)
        {
            // write magic
            writer.WriteCString(MAGIC, MAGIC_LENGTH);

            // write chunk list
            writer.Write(ChunkCount);
            foreach (AmdChunk chunk in Chunks)
            {
                chunk.Write(writer);
            }
        }
    }
}
