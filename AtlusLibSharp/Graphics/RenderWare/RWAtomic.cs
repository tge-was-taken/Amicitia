namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Encapsulates a RenderWare Atomic (draw call) and all of its corresponding data structures.
    /// </summary>
    public class RWAtomic : RWNode
    {
        private RWAtomicStruct _struct;
        private RWExtension _extension;

        /// <summary>
        /// Gets or sets the index of the <see cref="RWFrame"/> (bone) to assign to this Atomic. 
        /// </summary>
        public int FrameIndex
        {
            get { return _struct.FrameIndex; }
            set { _struct.FrameIndex = value; }
        }

        /// <summary>
        /// Gets or sets the index of the <see cref="RWGeometry"/> to assign to this Atomic.
        /// </summary>
        public int GeometryIndex
        {
            get { return _struct.GeometryIndex; }
            set { _struct.GeometryIndex = value; }
        }

        /// <summary>
        /// Gets or sets the Flag1 value for this Atomic.
        /// </summary>
        public int Flag1
        {
            get { return _struct.Flag1; }
            set { _struct.Flag1 = value; }
        }

        /// <summary>
        /// Gets or sets the Flag2 value for this Atomic.
        /// </summary>
        public int Flag2
        {
            get { return _struct.Flag2; }
            set { _struct.Flag2 = value; }
        }

        /// <summary>
        /// Gets the extension nodes for this Atomic.
        /// </summary>
        public List<RWNode> Extensions
        {
            get { return _extension.Children; }
        }

        /// <summary>
        /// Initialize a new empty <see cref="RWAtomic"/> instance.
        /// </summary>
        public RWAtomic()
            : base(RWType.Atomic)
        {
            _struct = new RWAtomicStruct(0, 0, 0, 0, this);
            _extension = new RWExtension(this);
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RWAtomic(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
                : base(header)
        {
            _struct = RWNodeFactory.GetNode<RWAtomicStruct>(this, reader);
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
