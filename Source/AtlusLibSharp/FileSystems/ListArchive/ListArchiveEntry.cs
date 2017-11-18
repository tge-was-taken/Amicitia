namespace AtlusLibSharp.FileSystems.ListArchive
{
    using System.IO;
    using AtlusLibSharp.IO;
    using AtlusLibSharp.Utilities;

    public class ListArchiveEntry : IArchiveEntry
    {
        // Constants
        internal const int NAME_LENGTH = 32;

        // Fields
        private string mName;
        private byte[] mData;
        internal bool mBigEndian;

        // Constructors
        public ListArchiveEntry(string filepath)
        {
            mName = Path.GetFileName(filepath);
            mData = File.ReadAllBytes(filepath);
        }

        public ListArchiveEntry(string name, Stream stream)
        {
            mName = name;
            mData = stream.ReadAllBytes();
        }

        public ListArchiveEntry(string name, byte[] data)
        {
            mName = name;
            mData = data;
        }

        internal ListArchiveEntry(EndianBinaryReader reader)
        {
            mName = reader.ReadCString(NAME_LENGTH);
            int dataLength = reader.ReadInt32();
            mData = reader.ReadBytes(dataLength);

            if ( reader.Endianness == Endianness.BigEndian )
                mBigEndian = true;
        }

        // Properties
        public string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        IArchiveEntry IArchiveEntry.Create(byte[] data, string name)
        {
            return new ListArchiveEntry(name, data);
        }

        public int DataLength
        {
            get { return mData.Length; }
        }

        public byte[] Data
        {
            get { return mData; }
            set { mData = value; }
        }

        // Methods
        internal void InternalWrite(BinaryWriter writer)
        {
            writer.WriteCString(mName, NAME_LENGTH);

            int length = mData.Length;
            if ( mBigEndian )
                length = EndiannessHelper.Swap( length );

            writer.Write( length );
            writer.Write(mData);
        }
    }
}
