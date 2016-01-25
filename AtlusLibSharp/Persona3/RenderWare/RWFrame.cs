using OpenTK;
using System.IO;
using System.Collections.Generic;

namespace AtlusLibSharp.Persona3.RenderWare
{
    using Utilities;

    public class RWFrame
    {
        public int Index { get; private set; }
        public Matrix4 WorldMatrix { get; private set; }
        public Matrix4 LocalMatrix { get; private set; }
        public int ParentIndex { get; private set; }
        public int ExportFlag { get; private set; }
        private RWFrame _parent;

        public RWFrame Parent
        {
            get { return _parent; }
            set
            {
                if (value == null)
                    return;
                if (value.Children == null)
                    value.Children = new List<RWFrame>();
                if (!value.Children.Contains(this))
                    value.Children.Add(this);
                _parent = value;
            }
        }

        public List<RWFrame> Children { get; set; }

        // Constructors
        public RWFrame(Matrix4 localMatrix, int index, int parentIndex, RWFrame[] frames)
        {
            LocalMatrix = localMatrix;
            ParentIndex = parentIndex;
            ExportFlag = 0;
            Parent = frames[parentIndex];

            if (Parent != null)
                WorldMatrix = LocalMatrix * Parent.WorldMatrix;
            else
                WorldMatrix = LocalMatrix;
        }

        internal RWFrame(BinaryReader reader, int index, RWFrame[] frames)
        {
            LocalMatrix = reader.ReadMatrix4x3().ToMatrix4();
            ParentIndex = reader.ReadInt32();
            ExportFlag = reader.ReadInt32();

            Index = index;

            if (ParentIndex != -1)
                Parent = frames[ParentIndex];

            if (Parent != null)
                WorldMatrix = LocalMatrix * Parent.WorldMatrix;
            else
                WorldMatrix = LocalMatrix;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(LocalMatrix);
            writer.Write(ParentIndex);
            writer.Write(ExportFlag);
        }
    }
}