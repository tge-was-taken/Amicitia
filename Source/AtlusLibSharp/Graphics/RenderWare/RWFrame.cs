using AtlusLibSharp.IO;

namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;
    using System.Collections.Generic;
    using System.Numerics;
    using Utilities;

    public class RwFrame
    {
        private RwFrame mParent;

        /// <summary>
        /// Gets the local transformation matrix of the scene node.
        /// </summary>
        public Matrix4x4 Transform { get; set; }

        /// <summary>
        /// Gets the user flags of the scene node.
        /// </summary>
        public int UserFlags { get; set; }

        /// <summary>
        /// Gets a list of children of this scene node.
        /// </summary>
        public List<RwFrame> Children { get; }

        /// <summary>
        /// Gets the parent node of this scene node. Returns null if there is no parent assigned.
        /// </summary>
        public RwFrame Parent
        {
            get { return mParent; }
            private set
            {
                if (value == null)
                {
                    return;
                }

                mParent = value;
                mParent.Children.Add(this);
            }
        }

        /// <summary>
        /// Gets the world (absolute) transformation matrix of this scene node. This can be an expensive operation.
        /// </summary>
        public Matrix4x4 WorldTransform
        {
            get
            {
                Matrix4x4 tfm;

                if (mParent != null)
                    tfm = Transform * mParent.WorldTransform;
                else
                    tfm = Transform;

                return tfm;
            }
        }

        /// <summary>
        /// Gets if the scene node has bone metadata.
        /// </summary>
        public bool HasHAnimExtension
        {
            get { return HAnimFrameExtensionNode != null; }
        }

        /// <summary>
        /// Gets the bone metadata of this scene node. Returns null if not present.
        /// </summary>
        public RwHAnimFrameExtensionNode HAnimFrameExtensionNode { get; set; }

        /// <summary>
        /// Construct a new scene node with a given local transform, node parent and user flag.
        /// </summary>
        /// <param name="transform">The local transformation matrix of the node.</param>
        /// <param name="parent">The parent node of the scene node.</param>
        /// <param name="userFlag">The user flag to set.</param>
        public RwFrame(Matrix4x4 transform, RwFrame parent, int userFlag)
        {
            Transform = transform;
            UserFlags = userFlag;
            Parent = parent;
        }

        // read from binary reader
        internal RwFrame(BinaryReader reader, List<RwFrame> frameList)
        {
            Transform = reader.ReadMatrix4x3();
            int parentIndex = reader.ReadInt32();
            UserFlags = reader.ReadInt32();

            Children = new List<RwFrame>();

            if (parentIndex != -1)
                mParent = frameList[parentIndex];
            else
                mParent = null;
        }

        // write with binary writer
        internal void Write(BinaryWriter writer, List<RwFrame> sceneNodeList)
        {
            writer.Write(Transform, true);
            writer.Write(sceneNodeList.IndexOf(Parent));
            writer.Write(UserFlags);
        }
    }
}