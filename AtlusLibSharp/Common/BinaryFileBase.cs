namespace AtlusLibSharp.Common
{
    using System.IO;
    using System.Text;

    public abstract class BinaryFileBase
    {
        public void Save(string path)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(path)))
            {
                InternalWrite(writer);
            }
        }

        public void Save(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Default, true))
            {
                InternalWrite(writer);
            }
        }

        public virtual byte[] GetBytes()
        {
            MemoryStream mStream = new MemoryStream();
            Save(mStream);
            byte[] bytes = mStream.ToArray();
            mStream.Dispose();
            return bytes;
        }

        internal abstract void InternalWrite(BinaryWriter writer);
    }

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
