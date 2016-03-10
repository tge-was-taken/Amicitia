namespace AtlusLibSharp.IO
{
    using System.IO;
    using System.Text;

    public abstract class BinaryFileBase : IWriteable
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

        void IWriteable.InternalWrite(BinaryWriter writer)
        {
            InternalWrite(writer);
        }
    }
}
