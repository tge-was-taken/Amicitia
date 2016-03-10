using AtlusLibSharp.Utilities;
using System.IO;

namespace AtlusLibSharp.Scripting
{
    public class BFCodeLabel
    {
        internal const int SIZE = 0x20;

        private string _name;
        private uint _offset;
        private ushort _id;
        private int _opcodeIndex;

        internal BFCodeLabel() { }

        internal BFCodeLabel(BinaryReader reader)
        {
            _name = reader.ReadCString(24);
            _offset = reader.ReadUInt32();
            int unused = reader.ReadInt32();
            _opcodeIndex = (int)_offset;
        }

        internal void InternalWrite(BinaryWriter writer)
        {
            writer.WriteCString(_name, 24);
            writer.Write(_offset);
            writer.Write(0);
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int ID
        {
            get { return _id; }
            internal set { _id = (ushort)value; }
        }

        public int OpcodeIndex
        {
            get { return _opcodeIndex; }
            internal set { _opcodeIndex = value; }
        }

        internal uint Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }
    }
}
