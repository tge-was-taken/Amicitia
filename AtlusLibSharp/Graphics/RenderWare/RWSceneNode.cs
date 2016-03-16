namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;
    using System.Collections.Generic;
    using OpenTK;
    using Utilities;

    public class RWSceneNode
    {
        private Matrix4 _transform;
        private RWSceneNode _parent;
        private int _userFlags;
        private List<RWSceneNode> _children;
        private RWSceneNodeBoneMetadata _boneMetadata;

        /// <summary>
        /// Gets the local transformation matrix of the scene node.
        /// </summary>
        public Matrix4 Transform
        {
            get { return _transform; }
            set
            {
                _transform = value;
            }
        }

        /// <summary>
        /// Gets the user flags of the scene node.
        /// </summary>
        public int UserFlags
        {
            get { return _userFlags; }
        }

        /// <summary>
        /// Gets a list of children of this scene node.
        /// </summary>
        public List<RWSceneNode> Children
        {
            get { return _children; }
        }

        /// <summary>
        /// Gets the parent node of this scene node. Returns null if there is no parent assigned.
        /// </summary>
        public RWSceneNode Parent
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

        /// <summary>
        /// Gets the world (absolute) transformation matrix of this scene node. This can be an expensive operation.
        /// </summary>
        public Matrix4 WorldTransform
        {
            get
            {
                Matrix4 tfm;

                if (_parent != null)
                    tfm = _transform * _parent.WorldTransform;
                else
                    tfm = _transform;

                return tfm;
            }
        }

        public bool HasBoneMetadata
        {
            get { return _boneMetadata != null; }
        }

        public RWSceneNodeBoneMetadata BoneMetadata
        {
            get { return _boneMetadata; }
            set { _boneMetadata = value; }
        } 

        /// <summary>
        /// Construct a new scene node with a given local transform, node parent and user flag.
        /// </summary>
        /// <param name="transform">The local transformation matrix of the node.</param>
        /// <param name="parent">The parent node of the scene node.</param>
        /// <param name="userFlag">The user flag to set.</param>
        public RWSceneNode(Matrix4 transform, RWSceneNode parent, int userFlag)
        {
            _transform = transform;
            _userFlags = userFlag;
            Parent = parent;
        }

        // read from binary reader
        internal RWSceneNode(BinaryReader reader, List<RWSceneNode> frameList)
        {
            _transform = reader.ReadMatrix4x3().ToMatrix4();
            int parentIndex = reader.ReadInt32();
            _userFlags = reader.ReadInt32();

            _children = new List<RWSceneNode>();

            if (parentIndex != -1)
                _parent = frameList[parentIndex];
            else
                _parent = null;
        }

        // write with binary writer
        internal void InternalWrite(BinaryWriter writer, List<RWSceneNode> sceneNodeList)
        {
            writer.Write(_transform.ToMatrix4x3());
            writer.Write(sceneNodeList.IndexOf(Parent));
            writer.Write(_userFlags);
        }
    }
}