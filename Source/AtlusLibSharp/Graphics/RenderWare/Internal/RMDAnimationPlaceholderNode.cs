namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;

    /// <summary>
    /// Represents a RenderWare node that indicates the a placeholder <see cref="RMDAnimationSet"/>. Its purpose is to keep the animations set order intact. 
    /// <para>It does not contain any data other than itself.</para>
    /// <para>This node is an extension to the RenderWare nodes developed by Atlus.</para>
    /// </summary>
    internal class RmdAnimationPlaceholderNode : RwNode
    {
        /// <summary>
        /// Initializes a new <see cref="RMDAnimationSetTerminator"/>.
        /// </summary>
        public RmdAnimationPlaceholderNode() : base(RwNodeId.RmdAnimationPlaceholderNode) { }

        /// <summary>
        /// Initializer only to be called in <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RmdAnimationPlaceholderNode(RwNodeFactory.RwNodeHeader header) : base(header) { }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer) { }
    }
}
