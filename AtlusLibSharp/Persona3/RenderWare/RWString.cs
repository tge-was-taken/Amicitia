using System.IO;
using System.Text;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWString : RWNode
    {
        public string Value { get; set; }

        internal RWString(uint size, uint version, RWNode parent, BinaryReader reader)
                : base(RWType.String, size, version, parent)
        {
            Value = "";
            int idx = 0;
            while (idx != Size)
            {
                idx++;
                Value += (char)reader.ReadByte();
            }
            Value = Value.Replace("\0", "");
        }

        public RWString(string value)
            : base(RWType.String)
        {
            Value = value;
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write(Encoding.ASCII.GetBytes(Value));
            writer.Write((byte)0);
            int align = (4 - ((Encoding.ASCII.GetByteCount(Value) + 1) % 4));
            if (align == 4)
                align = 0;
            writer.Write(new byte[align]);
        }
    }
}