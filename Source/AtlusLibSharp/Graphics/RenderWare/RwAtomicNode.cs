namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Encapsulates a RenderWare draw call and all of its corresponding data structures.
    /// </summary>
    public class RwAtomicNode : RwNode
    {
        private RwAtomicStructNode mStructNode;
        private RwExtensionNode mExtensionNode;

        /// <summary>
        /// Gets or sets the index of the <see cref="RwFrame"/> to assign to this draw call. 
        /// </summary>
        public int FrameIndex
        {
            get { return mStructNode.FrameIndex; }
            set { mStructNode.FrameIndex = value; }
        }

        /// <summary>
        /// Gets or sets the index of the <see cref="RwGeometryNode"/> to assign to this draw call.
        /// </summary>
        public int GeometryIndex
        {
            get { return mStructNode.GeometryIndex; }
            set { mStructNode.GeometryIndex = value; }
        }

        /// <summary>
        /// Gets or sets the Flag1 value for this draw call.
        /// </summary>
        public int Flag1
        {
            get { return mStructNode.Flag1; }
            set { mStructNode.Flag1 = value; }
        }

        /// <summary>
        /// Gets or sets the Flag2 value for this draw call.
        /// </summary>
        public int Flag2
        {
            get { return mStructNode.Flag2; }
            set { mStructNode.Flag2 = value; }
        }

        /// <summary>
        /// Gets the extension nodes for this draw call.
        /// </summary>
        public List<RwNode> Extensions
        {
            get { return mExtensionNode.Children; }
        }

        /// <summary>
        /// Initialize a new empty <see cref="RwAtomicNode"/> instance.
        /// </summary>
        public RwAtomicNode()
            : base(RwNodeId.RwAtomicNode)
        {
            mStructNode = new RwAtomicStructNode(0, 0, 0, 0, this);
            mExtensionNode = new RwExtensionNode(this);
        }

        public RwAtomicNode(RwNode parent, int frameIndex, int geometryIndex, int flag1 = 0, int flag2 = 0)
            : base(RwNodeId.RwAtomicNode, parent)
        {
            mStructNode = new RwAtomicStructNode(frameIndex, geometryIndex, flag1, flag2, this);
            mExtensionNode = new RwExtensionNode(this);
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RwAtomicNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
                : base(header)
        {
            mStructNode = RwNodeFactory.GetNode<RwAtomicStructNode>(this, reader);
            mExtensionNode = RwNodeFactory.GetNode<RwExtensionNode>(this, reader);
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            mStructNode.Write(writer);
            mExtensionNode.Write(writer);
        }
    }
}
