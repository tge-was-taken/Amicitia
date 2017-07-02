namespace AtlusLibSharp.Scripting
{
    using System.IO;

    internal struct TypeTableEntry
    {
        internal const int TypetableEntryLength = 16;

        public int Type;
        public int ElementLength;
        public int ElementCount;
        public int DataOffset;

        internal TypeTableEntry(BinaryReader reader)
        {
            Type = reader.ReadInt32();
            ElementLength = reader.ReadInt32();
            ElementCount = reader.ReadInt32();
            DataOffset = reader.ReadInt32();
        }

        internal void InternalWrite(BinaryWriter writer)
        {
            writer.Write(Type);
            writer.Write(ElementLength);
            writer.Write(ElementCount);
            writer.Write(DataOffset);
        }
    }
}
