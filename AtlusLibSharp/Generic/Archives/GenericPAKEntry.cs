namespace AtlusLibSharp.Generic.Archives
{
    using System.IO;
    using Utilities;

    /// <summary>
    /// Class representing an entry in a GenericPAK instance.
    /// </summary>
    public class GenericPAKEntry
    {
        // Constants
        internal static int NAME_LENGTH = 252;

        // Fields
        private string _name;
        private int _length;
        private byte[] _data;

        // Constructors
        public GenericPAKEntry(string filePath)
        {
            _name = Path.GetFileName(filePath);
            _data = File.ReadAllBytes(filePath);
            _length = _data.Length;
        }

        public GenericPAKEntry(string name, byte[] data)
        {
            _name = name;
            _data = data;
            _length = data.Length;
        }

        public GenericPAKEntry(string name, MemoryStream stream)
        {
            _name = name;
            _data = stream.ToArray();
            _length = _data.Length;
        }

        internal GenericPAKEntry(BinaryReader reader)
        {
            Read(reader);
        }

        // Properties
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                if (_name.Length > NAME_LENGTH)
                {
                    _name = _name.Remove(NAME_LENGTH - 1);
                }
            }
        }

        public int Length
        {
            get { return _length; }
        }

        public byte[] Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
                _length = _data.Length;
            }
        }

        // Methods
        internal void InternalWrite(BinaryWriter writer)
        {
            writer.WriteCString(_name, NAME_LENGTH);
            writer.Write(_length);
            writer.Write(_data);
            writer.AlignPosition(64);
        }

        private void Read(BinaryReader reader)
        {
            _name = reader.ReadCString(NAME_LENGTH);
            _length = reader.ReadInt32();

            if (_length > reader.BaseStream.Length || _length < 0)
            {
                throw new InvalidDataException("GenericPAKEntry: length has an invalid value.");
            }

            _data = reader.ReadBytes(_length);
            reader.AlignPosition(64);
        }
    }
}
