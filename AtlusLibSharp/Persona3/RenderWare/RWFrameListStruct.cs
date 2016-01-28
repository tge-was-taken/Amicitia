using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWFrameListStruct : RWNode
    {
        // Fields
        private RWFrame[] _frames;
        private int _frameCount;

        // Properties
        public int FrameCount
        {
            get { return _frameCount; }
        }

        public RWFrame[] Frames
        {
            get { return _frames; }
            private set
            {
                _frames = value;
                _frameCount = _frames.Length;
            }
        }

        // Constructors
        public RWFrameListStruct(RWFrame[] frames)
            : base(RWType.Struct)
        {
            Frames = frames;
        }

        internal RWFrameListStruct(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
                : base(header)
        {
            _frameCount = reader.ReadInt32();
            _frames = new RWFrame[_frameCount];
            for (int i = 0; i < FrameCount; i++)
                _frames[i] = new RWFrame(reader, i, this);
        }

        // Methods
        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write(_frameCount);
            for (int i = 0; i < _frameCount; i++)
                _frames[i].InternalWrite(writer);
        }
    }
}