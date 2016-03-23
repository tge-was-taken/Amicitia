namespace AtlusLibSharp.Tables
{
    using AtlusLibSharp.Utilities;
    using System.IO;

    public abstract class MessageTableBase : TableBase
    {
        public abstract void SaveText(string textFilePath);

        protected void ReadStringsFromSection(BinaryReader reader, ref string[] array, int stringLength)
        {
            using (BinaryReader sectionReader = new BinaryReader(ReadSection(reader)))
            {
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = sectionReader.ReadCString(stringLength);
                }
            }
        }

        protected void WriteStringSection(BinaryWriter writer, string[] array, int stringLength)
        {
            using (BinaryWriter sectionWriter = new BinaryWriter(new MemoryStream()))
            {
                // write the strings to the memory stream
                foreach (string item in array)
                {
                    sectionWriter.WriteCString(item, stringLength);
                }

                // write the section to the file using the memory stream we just wrote to
                WriteSection(writer, sectionWriter.BaseStream);
            }
        }

        protected void WriteStringSectionToText(StreamWriter writer, string[] array, string name, int nameLength)
        {
            writer.WriteLine("SECTION:{0}", name);
            writer.WriteLine("// Note: Max name length is {0} characters", nameLength);
            foreach (string item in array)
                writer.WriteLine(item);
            writer.WriteLine();
        }
    }
}
