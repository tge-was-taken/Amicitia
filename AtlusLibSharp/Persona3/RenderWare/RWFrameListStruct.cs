using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWFrameListStruct : RWNode
    {
        private RWFrame[] _frames;
        public int FrameCount { get; private set; }

        public RWFrame[] Frames
        {
            get { return _frames; }
            set
            {
                _frames = value;
                FrameCount = _frames.Length;
            }
        }

        internal RWFrameListStruct(uint size, uint version, RWNode parent, BinaryReader reader)
                : base(RWType.Struct, size, version, parent)
        {
            FrameCount = reader.ReadInt32();
            Frames = new RWFrame[FrameCount];
            for (int i = 0; i < FrameCount; i++)
                Frames[i] = new RWFrame(reader, i, Frames);
        }

        public RWFrameListStruct(RWFrame[] frames)
            : base(RWType.Struct)
        {
            Frames = frames;
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write(FrameCount);
            for (int i = 0; i < FrameCount; i++)
                Frames[i].Write(writer);
        }
    }
}