namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Represents a RenderWare node containing a list of uv animations.
    /// </summary>
    public class RWUVAnimationDictionary : RWNode
    {
        // promote this to a list of RWAnimation later
        private RWUVAnimationDictionaryStruct _struct;
        private List<RWNode> _uvAnimations;

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
        public List<RWNode> UVAnimations
        {
            get { return _uvAnimations; }
            set { _uvAnimations = value; }
        }

        /// <summary>
        /// Initialize a RenderWare UV animation dictionary using a list of uv animations.
        /// </summary>
        /// <param name="uvAnimations">The list of uv animations to initialize the uv animation dictionary with.</param>
        /// <param name="parent">The parent of the uv animation dictionary node. Value is null if not specified.</param>
        public RWUVAnimationDictionary(IList<RWNode> uvAnimations, RWNode parent = null)
            : base(RWNodeType.UVAnimationDictionary, parent)
        {
            _uvAnimations = uvAnimations.ToList();
        }

        /// <summary>
        /// Initialize an empty RenderWare UV animation dictionary.
        /// </summary>
        /// <param name="parent">The parent of the uv animation dictionary node. Value is null if not specified.</param>
        public RWUVAnimationDictionary(RWNode parent = null)
            : base(RWNodeType.UVAnimationDictionary, parent)
        {
            _uvAnimations = new List<RWNode>();
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RWUVAnimationDictionary(RWNodeFactory.RWNodeInfo header, BinaryReader reader)
            : base(header)
        {
            _struct = RWNodeFactory.GetNode<RWUVAnimationDictionaryStruct>(this, reader);
            _uvAnimations = new List<RWNode>(_struct.UVAnimationCount);

            for (int i = 0; i < _struct.UVAnimationCount; i++)
            {
                _uvAnimations.Add(RWNodeFactory.GetNode(this, reader));
            }
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            // update the struct and write it
            _struct.UVAnimationCount = UVAnimationCount;
            _struct.InternalWrite(writer);

            // write the uv anims
            foreach (RWNode uvAnim in _uvAnimations)
            {
                uvAnim.InternalWrite(writer);
            }
        }
    }
}
