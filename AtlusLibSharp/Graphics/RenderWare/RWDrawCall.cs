namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Encapsulates a RenderWare draw call and all of its corresponding data structures.
    /// </summary>
    public class RWDrawCall : RWNode
    {
        private RWDrawCallStruct _struct;
        private RWExtension _extension;

        /// <summary>
        /// Gets or sets the index of the <see cref="RWSceneNode"/> to assign to this draw call. 
        /// </summary>
        public int NodeIndex
        {
            get { return _struct.FrameIndex; }
            set { _struct.FrameIndex = value; }
        }

        /// <summary>
        /// Gets or sets the index of the <see cref="RWMesh"/> to assign to this draw call.
        /// </summary>
        public int MeshIndex
        {
            get { return _struct.GeometryIndex; }
            set { _struct.GeometryIndex = value; }
        }

        /// <summary>
        /// Gets or sets the Flag1 value for this draw call.
        /// </summary>
        public int Flag1
        {
            get { return _struct.Flag1; }
            set { _struct.Flag1 = value; }
        }

        /// <summary>
        /// Gets or sets the Flag2 value for this draw call.
        /// </summary>
        public int Flag2
        {
            get { return _struct.Flag2; }
            set { _struct.Flag2 = value; }
        }

        /// <summary>
        /// Gets the extension nodes for this draw call.
        /// </summary>
        public List<RWNode> Extensions
        {
            get { return _extension.Children; }
        }

        /// <summary>
        /// Initialize a new empty <see cref="RWDrawCall"/> instance.
        /// </summary>
        public RWDrawCall()
            : base(RWNodeType.DrawCall)
        {
            _struct = new RWDrawCallStruct(0, 0, 0, 0, this);
            _extension = new RWExtension(this);
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RWDrawCall(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
                : base(header)
        {
            _struct = RWNodeFactory.GetNode<RWDrawCallStruct>(this, reader);
            _extension = RWNodeFactory.GetNode<RWExtension>(this, reader);
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            _struct.InternalWrite(writer);
            _extension.InternalWrite(writer);
        }
    }
}
