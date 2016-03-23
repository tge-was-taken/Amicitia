namespace AtlusLibSharp.Tables
{
    using System.IO;
    using AtlusLibSharp.Utilities;
    using IO;

    public abstract class TableBase : BinaryFileBase
    {
        protected MemoryStream ReadSection(BinaryReader reader)
        {
            // read of section
            int length = reader.ReadInt32();
            byte[] buffer = new byte[length];

            // read the data
            reader.Read(buffer, 0, length);

            // align the position to a multiple of 16 bytes
            reader.AlignPosition(16);

            // return a new memorystream wrapping over the byte buffer
            return new MemoryStream(buffer);
        }

        internal void WriteSection(BinaryWriter writer, params IWriteable[] array)
        {
            using (BinaryWriter sectionWriter = new BinaryWriter(new MemoryStream()))
            {
                // write the objects to the temporary memory stream
                foreach (IWriteable item in array)
                {
                    item.InternalWrite(writer);
                }

                // write the section the file by copying the temp stream to the writer basestream
                WriteSection(writer, sectionWriter.BaseStream);
            }
        }

        protected void WriteSection(BinaryWriter writer, Stream stream)
        {
            // write length
            writer.Write((uint)stream.Length);

            // copy the stream to the underlying stream of the writer
            stream.CopyTo(writer.BaseStream);

            // write padding
            writer.AlignPosition(16);
        }
    }
}
