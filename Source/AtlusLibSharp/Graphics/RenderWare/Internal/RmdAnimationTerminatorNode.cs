namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;

    /// <summary>
    /// Represents a RenderWare node that indicates the end of an <see cref="RmdAnimation"/>. It does not contain any data other than itself.
    /// <para>This node is an extension to the RenderWare nodes developed by Atlus.</para>
    /// </summary>
    internal class RmdAnimationTerminatorNode : RwNode
    {
        /// <summary>
        /// Initializes a new <see cref="RmdAnimationTerminatorNode"/>.
        /// </summary>
        public RmdAnimationTerminatorNode(RwNode parent) : base(RwNodeId.RmdAnimationTerminatorNode, parent) { }

        /// <summary>
        /// Initializer only to be called in <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RmdAnimationTerminatorNode(RwNodeFactory.RwNodeHeader header) : base(header) { }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer) { }
    }
}
