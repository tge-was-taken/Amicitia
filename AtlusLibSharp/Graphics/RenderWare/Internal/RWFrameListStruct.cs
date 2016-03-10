using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AtlusLibSharp.Graphics.RenderWare
{
    internal class RWFrameListStruct : RWNode
    {
        // Fields
        private List<RWFrame> _frames;

        // Properties
        public int FrameCount
        {
            get { return _frames.Count; }
        }

        public List<RWFrame> Frames
        {
            get { return _frames; }
            private set
            {
                _frames = value;
            }
        }

        // Constructors
        public RWFrameListStruct(IList<RWFrame> frames)
            : base(RWType.Struct)
        {
            Frames = frames.ToList();
        }

        internal RWFrameListStruct(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
                : base(header)
        {
            int frameCount = reader.ReadInt32();
            _frames = new List<RWFrame>(frameCount);

            for (int i = 0; i < frameCount; i++)
            {
                _frames.Add(new RWFrame(reader, i, _frames));
            }
        }

        // Methods
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(FrameCount);
            for (int i = 0; i < FrameCount; i++)
                _frames[i].InternalWrite(writer);
        }
    }
}