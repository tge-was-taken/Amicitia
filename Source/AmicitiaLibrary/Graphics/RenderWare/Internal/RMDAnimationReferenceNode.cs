namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.IO;

    /// <summary>
    /// Represents a RenderWare node storing a redirect index to another animation set which in turn gets loaded when this node is accessed.
    /// </summary>
    internal class RmdAnimationInstanceNode : RwNode
    {
        private short mAnimationIndex;

        /// <summary>
        /// Gets or sets the index of the animation set to redirect to.
        /// </summary>
        public short AnimationIndex
        {
            get { return mAnimationIndex; }
            set { mAnimationIndex = value; }
        }

        /// <summary>
        /// Initialize a new instance of <see cref="RmdAnimationInstanceNode"/> using a given animation set index.
        /// </summary>
        /// <param name="referencedAnimationSetIndex">The index of the animation set redirected to by this <see cref="RmdAnimationInstanceNode"/>.</param>
        public RmdAnimationInstanceNode(short referencedAnimationSetIndex) : base(RwNodeId.RmdAnimationInstanceNode)
        {
            mAnimationIndex = referencedAnimationSetIndex;
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RmdAnimationInstanceNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            mAnimationIndex = reader.ReadInt16();
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write(mAnimationIndex);
        }
    }
}
