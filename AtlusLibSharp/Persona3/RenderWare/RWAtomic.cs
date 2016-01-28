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
            private set
            {
                _struct = value;
                _struct.Parent = this;
            }
        }

        public RWExtension Extension
        {
            get { return _extension; }
            private set
            {
                _extension = value;
                _extension.Parent = this;
            }
        }

        internal RWAtomic(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
                : base(header)
        {
            _struct = RWNodeFactory.GetNode<RWAtomicStruct>(this, reader);
            _extension = RWNodeFactory.GetNode<RWExtension>(this, reader);
        }

        public RWAtomic(RWAtomicStruct atomicStruct, RWExtension extension)
            : base(RWType.Atomic)
        {
            Struct = atomicStruct;
            Extension = extension;
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            _struct.InternalWrite(writer);
            _extension.InternalWrite(writer);
        }
    }
}
