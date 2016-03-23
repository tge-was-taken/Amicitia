namespace AtlusLibSharp.FileSystems.ListArchive
{
    using System.IO;
    using AtlusLibSharp.Utilities;

    public class ListArchiveFileEntry 
    {
        // Constants
        internal const int NAME_LENGTH = 32;

        // Fields
        private string _name;
        private byte[] _data;

        // Constructors
        public ListArchiveFileEntry(string filepath)
        {
            _name = Path.GetFileName(filepath);
            _data = File.ReadAllBytes(filepath);
        }

        public ListArchiveFileEntry(string name, Stream stream)
        {
            _name = name;
            _data = new byte[(int)stream.Length];
            stream.Read(_data, 0, (int)stream.Length);
        }

        public ListArchiveFileEntry(string name, byte[] data)
        {
            _name = name;
            _data = data;
        }

        internal ListArchiveFileEntry(BinaryReader reader)
        {
            _name = reader.ReadCString(NAME_LENGTH);
            int dataLength = reader.ReadInt32();
            _data = reader.ReadBytes(dataLength);
        }

        // Properties
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int DataLength
        {
            get { return _data.Length; }
        }

        public byte[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        // Methods
        internal void InternalWrite(BinaryWriter writer)
        {
            writer.WriteCString(_name, NAME_LENGTH);
            writer.Write(_data.Length);
            writer.Write(_data);
        }
    }
}
