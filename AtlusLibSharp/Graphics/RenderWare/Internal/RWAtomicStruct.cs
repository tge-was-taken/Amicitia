using System.IO;

namespace AtlusLibSharp.Graphics.RenderWare
{
    internal class RWDrawCallStruct : RWNode
    {
        private int _frameIndex;
        private int _geometryIndex;
        private int _flag1;
        private int _flag2;
        
        public int FrameIndex
        {
            get { return _frameIndex; }
            set { _frameIndex = value; }
        }

        public int GeometryIndex
        {
            get { return _geometryIndex; }
            set { _geometryIndex = value; }
        }

        public int Flag1
        {
            get { return _flag1; }
            set { _flag1 = value; }
        }

        public int Flag2
        {
            get { return _flag2; }
            set { _flag2 = value; }
        }

        public RWDrawCallStruct(int frameIndex, int geometryIndex, int flag1, int flag2, RWNode parent = null)
            : base(RWNodeType.Struct, parent)
        {
            _frameIndex = frameIndex;
            _geometryIndex = geometryIndex;
            _flag1 = flag1;
            _flag2 = flag2;
        }

        internal RWDrawCallStruct(RWNodeFactory.RWNodeInfo header, BinaryReader reader)
            : base(header)
        {
            _frameIndex = reader.ReadInt32();
            _geometryIndex = reader.ReadInt32();
            _flag1 = reader.ReadInt32();
            _flag2 = reader.ReadInt32();
        }

        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(_frameIndex);
            writer.Write(_geometryIndex);
            writer.Write(_flag1);
            writer.Write(_flag2);
        }
    }
}