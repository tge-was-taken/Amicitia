namespace AtlusLibSharp.Persona3.Archives
{
    using System;
    using System.IO;
    using Utilities;

    /// <summary>
    /// Class representing an entry in a BVPArchive instance.
    /// </summary>
    public class BVPArchiveEntry
    {
        // Constants
        internal const int ENTRY_SIZE = 0xC;

        // Fields
        private int _flag;
        private int _offset;
        private int _length;
        private byte[] _data;

        // Constructors
        public BVPArchiveEntry(string filePath, int flag = 0)
        {
            _flag = flag;
            _data = File.ReadAllBytes(filePath);
            _length = _data.Length;
        }

        public BVPArchiveEntry(byte[] data, int flag = 0)
        {
            _flag = flag;
            _data = data;
            _length = data.Length;
        }

        internal BVPArchiveEntry(BinaryReader reader)
        {
            _flag = reader.ReadInt32();
            _offset = reader.ReadInt32();
            _length = reader.ReadInt32();

            if (_length != 0)
            {
                _data = reader.ReadBytesAtOffset(_length, _offset);
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

        public int Length
        {
            get { return _length; }
        }

        public byte[] Data
        {
            get { return _data; }
            set
            {
                _data = value;
                _length = _data.Length;
            }
        }

        internal int Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        // Methods
        internal void WriteEntry(BinaryWriter writer)
        {
            writer.Write(_flag);
            writer.Write(_offset);
            writer.Write(_length);
        }
    }
}
