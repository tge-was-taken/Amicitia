namespace GameIO
{
    using System.IO;
    using System.Text;

    public class GameIOWriter : BinaryWriter
    {
        private Endian _endian;

        public Endian Endian
        {
            get { return _endian; }
            set { _endian = value; }
        }

        public GameIOWriter(Stream output, Endian endian)
            : base(output)
        {
            _endian = endian;
        }

        public GameIOWriter(Stream output, Encoding encoding, Endian endian)
            : base(output, encoding)
        {
            _endian = endian;
        }

        public GameIOWriter(Stream output, Encoding encoding, bool leaveOpen, Endian endian)
            : base(output, encoding, leaveOpen)
        {
            _endian = endian;
        }

        public GameIOWriter(string filepath, Encoding encoding, bool leaveOpen, Endian endian)
            : base(File.OpenWrite(filepath), encoding, leaveOpen)
        {
            _endian = endian;
        }
    }
}
