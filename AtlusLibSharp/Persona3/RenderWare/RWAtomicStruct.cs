using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWAtomicStruct : RWNode
    {
        // Fields
        private int _frameIndex;
        private int _geometryIndex;
        private int _flag1;
        private int _flag2;
        
        // Properties
        public int FrameIndex
        {
            get { return _frameIndex; }
        }

        public int GeometryIndex
        {
            get { return _geometryIndex; }
        }

        public int Flag1
        {
            get { return _flag1; }
        }

        public int Flag2
        {
            get { return _flag2; }
        }

        // Constructors
        public RWAtomicStruct(int frameIndex, int geometryIndex, int flag1, int flag2)
            : base(RWType.Struct)
        {
            _frameIndex = frameIndex;
            _geometryIndex = geometryIndex;
            _flag1 = flag1;
            _flag2 = flag2;
        }

        internal RWAtomicStruct(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            _frameIndex = reader.ReadInt32();
            _geometryIndex = reader.ReadInt32();
            _flag1 = reader.ReadInt32();
            _flag2 = reader.ReadInt32();
        }

        // Methods
        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write(_frameIndex);
            writer.Write(_geometryIndex);
            writer.Write(_flag1);
            writer.Write(_flag2);
        }
    }
}