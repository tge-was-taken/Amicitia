namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;

    /// <summary>
    /// Represents a RenderWare node storing the number of <see cref="RMDAnimationSet"/> in the <see cref="RMDScene"/>.
    /// </summary>
    internal class RMDAnimationSetCount : RWNode
    {
        private short _animationSetCount;

        /// <summary>
        /// Initialize a new instance of <see cref="RMDAnimationSetCount"/> using a given number of animations sets.
        /// </summary>
        /// <param name="animationSetCount">The number of animation sets in the <see cref="RMDScene"/>.</param>
        public RMDAnimationSetCount(short animationSetCount) : base(RWNodeType.RMDAnimationSetCount)
        {
            _animationSetCount = animationSetCount;
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RMDAnimationSetCount(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader) 
            : base(header)
        {
            _animationSetCount = reader.ReadInt16();
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(_animationSetCount);
        }
    }
}
