namespace AtlusLibSharp.Generic.Archives
{
    using Persona3.Archives;
    using System.IO;
    using Utilities;

    /// <summary>
    /// Class representing an entry in a GenericPAK instance.
    /// </summary>
    public class GenericPAKEntry : IArchiveEntry
    {
        // Constants
        internal static int NAME_LENGTH = 252;

        // Fields
        private string _name;
        private byte[] _data;

        // Constructors
        public GenericPAKEntry(string filepath)
        {
            _name = Path.GetFileName(filepath);
            _data = File.ReadAllBytes(filepath);
        }

        public GenericPAKEntry(string name, byte[] data)
        {
            _name = name;
            _data = data;
        }

        public GenericPAKEntry(string name, MemoryStream stream)
        {
            _name = name;
            _data = stream.ToArray();
        }

        internal GenericPAKEntry(BinaryReader reader)
        {
            InternalRead(reader);
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

        public int DataLength
        {
            get { return _data.Length; }
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
            }
        }

        // Methods
        internal void InternalWrite(BinaryWriter writer)
        {
            writer.WriteCString(_name, NAME_LENGTH);
            writer.Write(_data.Length);
            writer.Write(_data);
            writer.AlignPosition(64);
        }

        private void InternalRead(BinaryReader reader)
        {
            _name = reader.ReadCString(NAME_LENGTH);
            int dataLength = reader.ReadInt32();

            if (dataLength > reader.BaseStream.Length || dataLength < 0)
            {
                throw new InvalidDataException("GenericPAKEntry: dataLength has an invalid value.");
            }

            _data = reader.ReadBytes(dataLength);
            reader.AlignPosition(64);
        }
    }
}
