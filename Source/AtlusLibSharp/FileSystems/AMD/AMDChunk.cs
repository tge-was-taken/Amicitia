namespace AtlusLibSharp.FileSystems.AMD
{
    using System.IO;
    using IO;
    using Utilities;

    public class AMDChunk : BinaryFileBase
    {
        /****************/
        /* Private data */
        /****************/
        // constants
        private const int TAG_LENGTH = 16;

        // members
        private string m_tag;
        private uint m_flags;
        private byte[] m_data;

        /**************/
        /* Properties */
        /**************/
        /// <summary>
        /// Gets or sets the tag string of this <see cref="AMDChunk"/>.
        /// </summary>
        public string Tag
        {
            get { return m_tag; }
            set { m_tag = value; }
        }

        /// <summary>
        /// Gets or sets the flags of this <see cref="AMDChunk"/>.
        /// </summary>
        public int Flags
        {
            get { return (int)m_flags; }
            set { m_flags = (uint)value; }
        }

        /// <summary>
        /// Gets the size of the data in the <see cref="AMDChunk"/>.
        /// </summary>
        public int Size
        {
            get { return m_data.Length; }
        }

        /// <summary>
        /// Gets or sets the data in the <see cref="AMDChunk"/>.
        /// </summary>
        public byte[] Data
        {
            get { return m_data; }
            set { m_data = value; }
        }

        /****************/
        /* Constructors */
        /****************/
        /// <summary>
        /// Creates a new, empty <see cref="AMDChunk"/>.
        /// </summary>
        public AMDChunk()
        {
        }

        /// <summary>
        /// Creates a new <see cref="AMDChunk"/> with a specified tag, flags and data.
        /// </summary>
        public AMDChunk(string tag, int flags, byte[] data)
        {
            m_tag = tag;
            m_flags = (uint)flags;
            m_data = data;
        }

        /// <summary>
        /// Creates a new <see cref="AMDChunk"/> with a specified tag, flags and data stream.
        /// </summary>
        public AMDChunk(string tag, int flags, Stream data)
        {
            m_tag = tag;
            m_flags = (uint)flags;
            data.Read(m_data, 0, (int)data.Length);
        }

        /// <summary>
        /// Creates a new <see cref="AMDChunk"/> by reading it using the given reader.
        /// </summary>
        internal AMDChunk(BinaryReader reader)
        {
            InternalRead(reader);
        }

        /************************/
        /* Read / Write methods */
        /************************/
        // read the data using the binary reader
        private void InternalRead(BinaryReader reader)
        {
            m_tag = reader.ReadCString(TAG_LENGTH);
            m_flags = reader.ReadUInt32();
            int size = reader.ReadInt32();
            m_data = reader.ReadBytes(size);
        }

        internal override void InternalWrite(BinaryWriter writer)
        {
            writer.WriteCString(m_tag, TAG_LENGTH);
            writer.Write(m_flags);
            writer.Write(Size);
            writer.Write(m_data);
        }
    }
}
