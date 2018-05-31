namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.IO;

    /// <summary>
    /// Represents a RenderWare node storing the number of <see cref="RmdAnimation"/> in the <see cref="RmdScene"/>.
    /// </summary>
    internal class RmdAnimationCountNode : RwNode
    {
        private short mAnimationSetCount;

        /// <summary>
        /// Initialize a new instance of <see cref="RmdAnimationCountNode"/> using a given number of animations sets.
        /// </summary>
        /// <param name="animationSetCount">The number of animation sets in the <see cref="RmdScene"/>.</param>
        public RmdAnimationCountNode(short animationSetCount) : base(RwNodeId.RmdAnimationCountNode)
        {
            mAnimationSetCount = animationSetCount;
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RmdAnimationCountNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader) 
            : base(header)
        {
            mAnimationSetCount = reader.ReadInt16();
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write(mAnimationSetCount);
        }
    }
}
