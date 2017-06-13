using AtlusLibSharp.Utilities;

namespace AtlusLibSharp.IO
{
    using System.IO;

    public class GenericBinaryFile : BinaryFileBase
    {
        private string _path;
        private byte[] _data;

        public GenericBinaryFile(string path)
        {
            _path = path;
            _data = File.ReadAllBytes(path);
        }

        public GenericBinaryFile(byte[] data)
        {
            _path = string.Empty;
            _data = data;
        }

        public GenericBinaryFile(Stream stream)
        {
            _path = string.Empty;
            _data = stream.ReadAllBytes();
        }

        public override byte[] GetBytes()
        {
            return _data;
        }

        internal override void InternalWrite(BinaryWriter writer)
        {
            writer.Write(_data);
        }
    }
}
