namespace AtlusLibSharp.Persona3.Archives
{
    using System;
    using System.IO;
    using Utilities;

    /// <summary>
    /// Class representing an entry in a BVPArchive instance.
    /// </summary>
    public class BVPArchiveEntry : IArchiveEntry
    {
        // Constants
        internal const int ENTRY_SIZE = 0xC;

        // Fields
        private int _flag;
        private int _offset;
        private byte[] _data;

        // Constructors
        public BVPArchiveEntry(string filePath, int flag = 0)
        {
            _flag = flag;
            _data = File.ReadAllBytes(filePath);
        }

        public BVPArchiveEntry(byte[] data, int flag = 0)
        {
            _flag = flag;
            _data = data;
        }

        internal BVPArchiveEntry(BinaryReader reader)
        {
            _flag = reader.ReadInt32();
            _offset = reader.ReadInt32();
            int length = reader.ReadInt32();

            if (length > reader.BaseStream.Length)
            {
                _flag = 0;
                _offset = 0;
                _data = new byte[0];
                return;
            }

            if (length != 0)
            {
                _data = reader.ReadBytesAtOffset(length, _offset);
            }
            else
            {
                _data = new byte[0];
            }
        }

        // Properties
        public int Flag
        {
            get { return _flag; }
            set { _flag = value; }
        }

        public int DataLength
        {
            get { return _data.Length; }
        }

        public byte[] Data
        {
            get { return _data; }
            set
            {
                _data = value;
            }
        }

        internal int Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        string IArchiveEntry.Name
        {
            get { return "Entry"; }
        }

        // Methods
        internal void WriteEntry(BinaryWriter writer)
        {
            writer.Write(_flag);
            writer.Write(_offset);
            writer.Write(_data.Length);
        }
    }
}
