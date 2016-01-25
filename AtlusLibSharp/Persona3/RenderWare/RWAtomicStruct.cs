using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWAtomicStruct : RWNode
    {
        // Fields
        public int FrameIndex { get; private set; }
        public int GeometryIndex { get; private set; }
        public int Flag1 { get; private set; }
        public int Flag2 { get; private set; }

        // Constructors
        public RWAtomicStruct(int frameIndex, int geometryIndex, int flag1, int flag2)
            : base(RWType.Struct)
        {
            FrameIndex = frameIndex;
            GeometryIndex = geometryIndex;
            Flag1 = flag1;
            Flag2 = flag2;
        }

        internal RWAtomicStruct(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            FrameIndex = reader.ReadInt32();
            GeometryIndex = reader.ReadInt32();
            Flag1 = reader.ReadInt32();
            Flag2 = reader.ReadInt32();
        }

        // Methods
        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write(FrameIndex);
            writer.Write(GeometryIndex);
            writer.Write(Flag1);
            writer.Write(Flag2);
        }
    }
}