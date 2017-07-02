using AtlusLibSharp.Utilities;

namespace AtlusLibSharp.IO
{
    using System.IO;

    public class BinaryFile : BinaryBase
    {
        public string Path { get; }

        public byte[] Bytes { get; }

        public int Length => Bytes.Length;

        public BinaryFile(string path)
        {
            Path = path;
            Bytes = File.ReadAllBytes(path);
        }

        public BinaryFile(byte[] data)
        {
            Path = string.Empty;
            Bytes = data;
        }

        public BinaryFile(Stream stream, bool leaveOpen = false)
        {
            Path = string.Empty;
            Bytes = stream.ReadAllBytes();
            if (!leaveOpen)
                stream.Dispose();
        }

        public override byte[] GetBytes()
        {
            return Bytes;
        }

        internal override void Write(BinaryWriter writer)
        {
            writer.Write(Bytes);
        }
    }
}
