namespace AtlusLibSharp.Persona3.RenderWare
{
    using System.IO;

    /// <summary>
    /// Represents a RenderWare node storing a redirect index to another animation set which in turn gets loaded when this node is accessed.
    /// </summary>
    internal class RMDAnimationSetRedirect : RWNode
    {
        private short _animationSetRedirectIndex;

        /// <summary>
        /// Gets or sets the index of the animation set to redirect to.
        /// </summary>
        public short AnimationSetRedirectIndex
        {
            get { return _animationSetRedirectIndex; }
            set { _animationSetRedirectIndex = value; }
        }

        /// <summary>
        /// Initialize a new instance of <see cref="RMDAnimationSetRedirect"/> using a given animation set index.
        /// </summary>
        /// <param name="referencedAnimationSetIndex">The index of the animation set redirected to by this <see cref="RMDAnimationSetRedirect"/>.</param>
        public RMDAnimationSetRedirect(short referencedAnimationSetIndex) : base(RWType.RMDAnimationSetRedirect)
        {
            _animationSetRedirectIndex = referencedAnimationSetIndex;
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RMDAnimationSetRedirect(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            _animationSetRedirectIndex = reader.ReadInt16();
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(_animationSetRedirectIndex);
        }
    }
}
