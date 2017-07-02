namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Represents a RenderWare node containing a list of uv animations.
    /// </summary>
    public class RwUVAnimationDictionary : RwNode
    {
        // promote this to a list of RWAnimation later
        private RWUVAnimationDictionaryStructNode mStructNode;
        private List<RwNode> _uvAnimations;

        /// <summary>
        /// Gets the number of UV animations in the uv animation dictionary.
        /// </summary>
        public int UVAnimationCount
        {
            get { return _uvAnimations.Count; }
        }

        /// <summary>
        /// Gets or sets the list of uv animations in the uv animation dictionary.
        /// </summary>
        public List<RwNode> UVAnimations
        {
            get { return _uvAnimations; }
            set { _uvAnimations = value; }
        }

        /// <summary>
        /// Initialize a RenderWare UV animation dictionary using a list of uv animations.
        /// </summary>
        /// <param name="uvAnimations">The list of uv animations to initialize the uv animation dictionary with.</param>
        /// <param name="parent">The parent of the uv animation dictionary node. Value is null if not specified.</param>
        public RwUVAnimationDictionary(IList<RwNode> uvAnimations, RwNode parent = null)
            : base(RwNodeId.RwUVAnimationDictionaryNode, parent)
        {
            _uvAnimations = uvAnimations.ToList();
        }

        /// <summary>
        /// Initialize an empty RenderWare UV animation dictionary.
        /// </summary>
        /// <param name="parent">The parent of the uv animation dictionary node. Value is null if not specified.</param>
        public RwUVAnimationDictionary(RwNode parent = null)
            : base(RwNodeId.RwUVAnimationDictionaryNode, parent)
        {
            _uvAnimations = new List<RwNode>();
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RwUVAnimationDictionary(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            mStructNode = RwNodeFactory.GetNode<RWUVAnimationDictionaryStructNode>(this, reader);
            _uvAnimations = new List<RwNode>(mStructNode.UvAnimationCount);

            for (int i = 0; i < mStructNode.UvAnimationCount; i++)
            {
                _uvAnimations.Add(RwNodeFactory.GetNode(this, reader));
            }
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            // update the struct and write it
            mStructNode.UvAnimationCount = UVAnimationCount;
            mStructNode.Write(writer);

            // write the uv anims
            foreach (RwNode uvAnim in _uvAnimations)
            {
                uvAnim.Write(writer);
            }
        }
    }
}
