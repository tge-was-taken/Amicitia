using AtlusLibSharp.Utilities;
using System.IO;

namespace AtlusLibSharp.Scripting
{
    public class BfCodeLabel
    {
        internal const int Size = 0x20;

        private string mName;
        private uint mOffset;
        private ushort mId;
        private int mOpcodeIndex;

        internal BfCodeLabel() { }

        internal BfCodeLabel(BinaryReader reader)
        {
            mName = reader.ReadCString(24);
            mOffset = reader.ReadUInt32();
            int unused = reader.ReadInt32();
            mOpcodeIndex = (int)mOffset;
        }

        internal void InternalWrite(BinaryWriter writer)
        {
            writer.WriteCString(mName, 24);
            writer.Write(mOffset);
            writer.Write(0);
        }

        public string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        public int Id
        {
            get { return mId; }
            internal set { mId = (ushort)value; }
        }

        public int OpcodeIndex
        {
            get { return mOpcodeIndex; }
            internal set { mOpcodeIndex = value; }
        }

        internal uint Offset
        {
            get { return mOffset; }
            set { mOffset = value; }
        }
    }
}
