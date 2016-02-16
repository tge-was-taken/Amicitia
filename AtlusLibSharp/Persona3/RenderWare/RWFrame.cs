using OpenTK;
using System.IO;
using System.Collections.Generic;

namespace AtlusLibSharp.Persona3.RenderWare
{
    using Common.Utilities;

    public class RWFrame
    {
        // struct fields
        private Matrix4 _localMatrix;
        private int _parentIndex;
        private int _exportFlag;

        // other private members
        private int _index;
        private List<RWFrame> _children;
        private RWFrame _parent;
        private Matrix4 _worldMatrix;

        /// <summary>
        /// Gets the local transformation matrix of this frame.
        /// </summary>
        public Matrix4 LocalMatrix
        {
            get { return _localMatrix; }
        }

        /// <summary>
        /// Gets the parent frame index of this frame.
        /// </summary>
        public int ParentIndex
        {
            get { return _parentIndex; }
        }

        /// <summary>
        /// Gets the export flag (not used by the engine itself) of this frame.
        /// </summary>
        public int ExportFlag
        {
            get { return _exportFlag; }
        }

        /// <summary>
        /// Gets the index in the frame list of this frame.
        /// </summary>
        public int Index
        {
            get { return _index; }
        }

        /// <summary>
        /// Gets a list of children of this frame.
        /// </summary>
        public List<RWFrame> Children
        {
            get { return _children; }
        }

        /// <summary>
        /// Gets the world (absolute) transformation matrix of this frame.
        /// </summary>
        public Matrix4 WorldMatrix
        {
            get { return _worldMatrix; }
        }

        /// <summary>
        /// Gets the parent frame of this frame.
        /// </summary>
        public RWFrame Parent
        {
            get { return _parent; }
            private set
            {
                if (value == null)
                {
                    return;
                }

                _parent = value;
                _parent.Children.Add(this);
            }
        }

        internal RWFrame(BinaryReader reader, int index, List<RWFrame> frameList)
        {
            _localMatrix = reader.ReadMatrix4x3().ToMatrix4();
            _parentIndex = reader.ReadInt32();
            _exportFlag = reader.ReadInt32();

            _index = index;
            _children = new List<RWFrame>();

            if (_parentIndex != -1)
            {
                _parent = frameList[_parentIndex];
                _worldMatrix = _parent._worldMatrix * _localMatrix;
            }
            else
            {
                _parent = null;
                _worldMatrix = _localMatrix;
            }
        }

        public RWFrame(Matrix4 localMatrix, int index, int parentIndex, IList<RWFrame> frames)
        {
            _localMatrix = localMatrix;
            _parentIndex = parentIndex;
            _exportFlag = 0;
            Parent = frames[_parentIndex];

            if (_parent != null)
                _worldMatrix = _localMatrix * _parent._worldMatrix;
            else
                _worldMatrix = _localMatrix;
        }

        internal void InternalWrite(BinaryWriter writer)
        {
            writer.Write(_localMatrix.ToMatrix4x3());
            writer.Write(_parentIndex);
            writer.Write(_exportFlag);
        }
    }
}