using OpenTK;
using System.IO;
using System.Collections.Generic;

namespace AtlusLibSharp.Persona3.RenderWare
{
    using Utilities;

    public class RWFrame
    {
        // Fields
        private int _index;
        private Matrix4 _worldMatrix;
        private Matrix4 _localMatrix;
        private int _parentIndex;
        private int _exportFlag;
        private RWFrame _parent;

        // Properties
        public int Index
        {
            get { return _index; }
        }

        public Matrix4 WorldMatrix
        {
            get { return _worldMatrix; }
        }

        public Matrix4 LocalMatrix
        {
            get { return _localMatrix; }
        }

        public int ParentIndex
        {
            get { return _parentIndex; }
        }

        public int ExportFlag
        {
            get { return _exportFlag; }
        }

        public RWFrame Parent
        {
            get { return _parent; }
            private set
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
        internal RWFrame(BinaryReader reader, int index, RWFrameListStruct rwStruct)
        {
            _index = index;
            _localMatrix = reader.ReadMatrix4x3().ToMatrix4();
            _parentIndex = reader.ReadInt32();
            _exportFlag = reader.ReadInt32();

            if (_parentIndex != -1)
                Parent = rwStruct.Frames[_parentIndex];

            if (_parent != null)
                _worldMatrix = _localMatrix * _parent._worldMatrix;
            else
                _worldMatrix = _localMatrix;
        }

        public RWFrame(Matrix4 localMatrix, int index, int parentIndex, RWFrameListStruct rwStruct)
        {
            _localMatrix = localMatrix;
            _parentIndex = parentIndex;
            _exportFlag = 0;
            Parent = rwStruct.Frames[_parentIndex];

            if (_parent != null)
                _worldMatrix = _localMatrix * _parent._worldMatrix;
            else
                _worldMatrix = _localMatrix;
        }

        // Methods
        internal void InternalWrite(BinaryWriter writer)
        {
            writer.Write(_localMatrix);
            writer.Write(_parentIndex);
            writer.Write(_exportFlag);
        }
    }
}