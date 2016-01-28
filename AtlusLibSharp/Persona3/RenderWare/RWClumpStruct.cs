using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWClumpStruct : RWNode
    {
        private int _atomicCount;
        private int _lightCount;
        private int _cameraCount;

        public int AtomicCount
        {
            get { return _atomicCount; }
        }

        public int LightCount
        {
            get { return _lightCount; }
        }

        public int CameraCount
        {
            get { return _cameraCount; }
        }

        internal RWClumpStruct(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            _atomicCount = reader.ReadInt32();
            _lightCount = reader.ReadInt32(); // unused
            _cameraCount = reader.ReadInt32(); // unused
        }

        internal RWClumpStruct(RWClump clump)
            : base(new RWNodeFactory.RWNodeProcHeader { Parent = clump, Type = RWType.Struct, Version = DefaultVersion })
        {
            _atomicCount = clump.AtomicList.Count;
            _lightCount = 0;
            _cameraCount = 0;
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write(_atomicCount);
            writer.Write(_lightCount);
            writer.Write(_cameraCount);
        }
    }
}
