using System;
using System.Collections;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal class RwFrameListStructNode : RwNode
    {
        // Fields
        public List<RwFrame> FrameList { get; }

        // Constructors
        public RwFrameListStructNode(RwNode parent) : base(RwNodeId.RwStructNode, parent)
        {
            FrameList = new List<RwFrame>();
        }

        public RwFrameListStructNode(IEnumerable<RwFrame> frames)
            : base(RwNodeId.RwStructNode)
        {
            FrameList = frames.ToList();
        }

        internal RwFrameListStructNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
                : base(header)
        {
            int frameCount = reader.ReadInt32();
            FrameList = new List<RwFrame>(frameCount);

            for (int i = 0; i < frameCount; i++)
            {
                FrameList.Add(new RwFrame(reader, FrameList));
            }
        }

        // Methods
        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write(FrameList.Count);
            foreach (RwFrame frame in FrameList)
                frame.Write(writer, FrameList);
        }
    }
}