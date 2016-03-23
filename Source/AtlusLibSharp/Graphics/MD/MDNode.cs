namespace AtlusLibSharp.SMT3.Modeling
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Common.Utilities;
    using OpenTK;

    public class MDNode
    {
        internal const int SIZE = 0x50;

        // Fields
        private IntPtr _ptr;
        private uint _flag1;
        private uint _flag2;
        private int _index;
        private int _parentIndex;
        private Vector3 _rotation;
        private Vector3 _position;
        private Vector3 _scale;
        private uint _meshBoundingBoxOffset;
        private uint _meshOffset;
        private uint _unk1;
        private uint _unk2;

        private MDNode _parent;
        private List<MDNode> _children = new List<MDNode>();
        private MDBoundingBox _bbox;
        private MDMesh _mesh;
        private string _name;
        private Matrix4 _localMatrix;
        private Matrix4 _worldMatrix;

        // Constructors
        internal MDNode(MDChunk model, BinaryReader reader)
        {
            _ptr = (IntPtr)reader.BaseStream.Position;
            Read(model, reader);
        }

        // Properties

        /// <summary>
        /// Get the index of this node.
        /// </summary>
        public int Index
        {
            get { return _index; }
        }

        /// <summary>
        /// Get the parent of this node.
        /// </summary>
        public MDNode Parent
        {
            get
            {
                return _parent;
            }

            set
            {
                _parent = value;
                if (_parent != null)
                {
                    _parent.Children.Add(this);
                }
            }
        }

        /// <summary>
        /// Get a list with children of this node.
        /// </summary>
        public List<MDNode> Children
        {
            get { return _children; }
            set { _children = value; }
        }

        /// <summary>
        /// Get the local rotation of this node in radians.
        /// </summary>
        public Vector3 Rotation
        {
            get { return _rotation; }
        }

        /// <summary>
        /// Get the local position of this node.
        /// </summary>
        public Vector3 Position
        {
            get { return _position; }
        }

        /// <summary>
        /// Get the local scale of this node.
        /// </summary>
        public Vector3 Scale
        {
            get { return _scale; }
        }

        /// <summary>
        /// Get the AABB bounding box of this node.
        /// </summary>
        public MDBoundingBox BoundingBox
        {
            get { return _bbox; }
        }

        /// <summary>
        /// Get the mesh assigned to this node.
        /// </summary>
        public MDMesh Mesh
        {
            get { return _mesh; }
        }

        /// <summary>
        /// Get the name of this node if present, otherwise returns null.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Get the local matrix of this node.
        /// </summary>
        public Matrix4 LocalMatrix
        {
            get { return _localMatrix; }
        }

        /// <summary>
        /// Get the world matrix of this node.
        /// </summary>
        public Matrix4 WorldMatrix
        {
            get { return _worldMatrix; }
        }

        // Methods
        public override string ToString()
        {
            if (_name != null)
            {
                return string.Format("Node flag1:{0} flag2:{1} name:{2}", _flag1, _flag2, _name);
            }
            else
            {
                return string.Format("Node flag1:{0} flag2:{1} index:{2}", _flag1, _flag2, _index);
            }
        }

        // Internal Methods
        internal void ReadMeshes(MDChunk model, BinaryReader reader)
        {
            if (_meshOffset != 0)
            {
                reader.BaseStream.Seek((uint)model.offset + MDChunk.DATA_START_ADDRESS + _meshOffset, SeekOrigin.Begin);
                _mesh = new MDMesh(model, this, reader);
            }
        }

        // Private Methods
        private void Read(MDChunk model, BinaryReader reader)
        {
            _flag1 = reader.ReadUInt32();
            _flag2 = reader.ReadUInt32();
            _index = reader.ReadInt32();
            _parentIndex = reader.ReadInt32();
            _rotation = reader.ReadVector3();
            reader.BaseStream.Position += 4;
            _position = reader.ReadVector3();
            reader.BaseStream.Position += 4;
            _scale = reader.ReadVector3();
            reader.BaseStream.Position += 4;
            _meshBoundingBoxOffset = reader.ReadUInt32();
            _meshOffset = reader.ReadUInt32();
            _unk1 = reader.ReadUInt32();
            _unk2 = reader.ReadUInt32();

            _localMatrix = Matrix4.CreateRotationX(_rotation.X) * Matrix4.CreateRotationY(_rotation.Y) * Matrix4.CreateRotationZ(_rotation.Z);
            _localMatrix *= Matrix4.CreateScale(_scale);
            _localMatrix *= Matrix4.CreateTranslation(_position);
            _worldMatrix = _localMatrix;

            if (_parentIndex != -1)
            {
                Parent = model.Nodes[_parentIndex];
                _worldMatrix *= Parent.WorldMatrix;
            }

            if (model.NodeNameSection != null)
            {
                _name = Array.Find(model.NodeNameSection.Names, n => n.ID == _index).Name;
            }

            if (_meshBoundingBoxOffset != 0)
            {
                reader.BaseStream.Seek((uint)model.offset + MDChunk.DATA_START_ADDRESS + _meshBoundingBoxOffset, SeekOrigin.Begin);
                _bbox = new MDBoundingBox();
                _bbox.Min = reader.ReadVector3();
                _bbox.Max = reader.ReadVector3();
            }
        }
    }
}
