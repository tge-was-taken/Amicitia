namespace AmicitiaLibrary.FileSystems.AMD
{
    using System.IO;
    using IO;
    using Utilities;

    public class AmdChunk : BinaryBase
    {
        /****************/
        /* Private data */
        /****************/
        // constants
        private const int TAG_LENGTH = 16;

        // members
        private string mTag;
        private uint mFlags;
        private byte[] mData;

        /**************/
        /* Properties */
        /**************/
        /// <summary>
        /// Gets or sets the tag string of this <see cref="AmdChunk"/>.
        /// </summary>
        public string Tag
        {
            get { return mTag; }
            set { mTag = value; }
        }

        /// <summary>
        /// Gets or sets the flags of this <see cref="AmdChunk"/>.
        /// </summary>
        public int Flags
        {
            get { return (int)mFlags; }
            set { mFlags = (uint)value; }
        }

        /// <summary>
        /// Gets the size of the data in the <see cref="AmdChunk"/>.
        /// </summary>
        public int Size
        {
            get { return mData.Length; }
        }

        /// <summary>
        /// Gets or sets the data in the <see cref="AmdChunk"/>.
        /// </summary>
        public byte[] Data
        {
            get { return mData; }
            set { mData = value; }
        }

        /****************/
        /* Constructors */
        /****************/
        /// <summary>
        /// Creates a new, empty <see cref="AmdChunk"/>.
        /// </summary>
        public AmdChunk()
        {
        }

        /// <summary>
        /// Creates a new <see cref="AmdChunk"/> with a specified tag, flags and data.
        /// </summary>
        public AmdChunk(string tag, int flags, byte[] data)
        {
            mTag = tag;
            mFlags = (uint)flags;
            mData = data;
        }

        /// <summary>
        /// Creates a new <see cref="AmdChunk"/> with a specified tag, flags and data stream.
        /// </summary>
        public AmdChunk(string tag, int flags, Stream data)
        {
            mTag = tag;
            mFlags = (uint)flags;
            data.Read(mData, 0, (int)data.Length);
        }

        /// <summary>
        /// Creates a new <see cref="AmdChunk"/> by reading it using the given reader.
        /// </summary>
        internal AmdChunk(BinaryReader reader)
        {
            InternalRead(reader);
        }

        /************************/
        /* Read / Write methods */
        /************************/
        // read the data using the binary reader
        private void InternalRead(BinaryReader reader)
        {
            mTag = reader.ReadCString(TAG_LENGTH);
            mFlags = reader.ReadUInt32();
            int size = reader.ReadInt32();
            mData = reader.ReadBytes(size);
        }

        internal override void Write(BinaryWriter writer)
        {
            writer.WriteCString(mTag, TAG_LENGTH);
            writer.Write(mFlags);
            writer.Write(Size);
            writer.Write(mData);
        }
    }
}
