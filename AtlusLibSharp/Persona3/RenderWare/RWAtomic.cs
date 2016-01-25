using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWAtomic : RWNode
    {
        private RWAtomicStruct _struct;
        private RWExtension _extension;

        public RWAtomicStruct Struct
        {
            get { return _struct; }
            set
            {
                _struct = value;
                _struct.Parent = this;
            }
        }

        public RWExtension Extension
        {
            get { return _extension; }
            set
            {
                _extension = value;
                _extension.Parent = this;
            }
        }

        public RWAtomic(uint size, uint version, RWNode parent, BinaryReader reader)
                : base(RWType.Atomic, size, version, parent)
        {
            Struct = ReadNode(reader, this) as RWAtomicStruct;
            Extension = ReadNode(reader, this) as RWExtension;
        }

        public RWAtomic(RWAtomicStruct data, RWExtension ext)
            : base(RWType.Atomic)
        {
            Struct = data;
            Extension = ext;
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            Struct.Write(writer);
            Extension.Write(writer);
        }
    }
}
