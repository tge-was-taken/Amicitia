using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWClumpStruct : RWNode
    {
        public int AtomicCount { get; private set; }
        public int LightCount { get; private set; } // unused
        public int CameraCount { get; private set; } // unused

        internal RWClumpStruct(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
                : base(header)
        {
            AtomicCount = reader.ReadInt32();
            LightCount = reader.ReadInt32(); // unused
            CameraCount = reader.ReadInt32(); // unused
        }

        internal RWClumpStruct(RWClump clump)
            : base(new RWNodeFactory.RWNodeProcHeader { Parent = clump, Type = RWType.Struct, Version = DefaultVersion })
        {
            AtomicCount = clump.AtomicList.Count;
            LightCount = 0;
            CameraCount = 0;
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write(AtomicCount);
            writer.Write(LightCount);
            writer.Write(CameraCount);
        }
    }
}
