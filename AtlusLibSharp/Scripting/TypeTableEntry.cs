namespace AtlusLibSharp.Scripting
{
    using System.IO;

    internal struct TypeTableEntry
    {
        internal const int TYPETABLE_ENTRY_LENGTH = 16;

        public int type;
        public int elementLength;
        public int elementCount;
        public int dataOffset;

        internal TypeTableEntry(BinaryReader reader)
        {
            type = reader.ReadInt32();
            elementLength = reader.ReadInt32();
            elementCount = reader.ReadInt32();
            dataOffset = reader.ReadInt32();
        }

        internal void InternalWrite(BinaryWriter writer)
        {
            writer.Write(type);
            writer.Write(elementLength);
            writer.Write(elementCount);
            writer.Write(dataOffset);
        }
    }
}
