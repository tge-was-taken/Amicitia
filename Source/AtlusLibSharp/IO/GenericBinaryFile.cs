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
            _data = new byte[stream.Length];
            stream.Read(_data, 0, (int)stream.Length);
            stream.Dispose();
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
