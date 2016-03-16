namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a RenderWare node set which get used to display a single animation. 
    /// </summary>
    public class RMDAnimationSet : RWNode
    {
        private List<RWNode> _animationNodes;

        /// <summary>
        /// Gets or sets the list of animation nodes in the animation set.
        /// </summary>
        public List<RWNode> Nodes
        {
            get { return _animationNodes; }
            set { _animationNodes = value; }
        }

        /// <summary>
        /// Initialize a new <see cref="RMDAnimationSet"/> instance using a <see cref="List{T}"/> of RenderWare nodes.
        /// </summary>
        /// <param name="animationNodes"><see cref="List{T}"/> of <see cref="RWNode"/> to initialize the animation node set with.</param>
        /// <param name="parent">The parent of the new <see cref="RMDAnimationSet"/>. Value is null if not specified.</param>
        public RMDAnimationSet(List<RWNode> animationNodes, RWNode parent = null)
            : base(RWNodeType.RMDAnimationSet, parent)
        {
            _animationNodes = animationNodes;

            for (int i = 0; i < _animationNodes.Count; i++)
            {
                _animationNodes[i].Parent = this;
            }
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RMDAnimationSet(RWNodeFactory.RWNodeInfo header, BinaryReader reader)
            : base(header)
        {
            _animationNodes = new List<RWNode>();

            var node = RWNodeFactory.GetNode(this, reader);
            while (node.Type != RWNodeType.RMDAnimationSetTerminator)
            {
                _animationNodes.Add(node);
                node = RWNodeFactory.GetNode(this, reader);
            }
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            // Write the animation nodes
            foreach (RWNode animationNode in _animationNodes)
            {
                animationNode.InternalWrite(writer);
            }

            // Write the animation set terminator
            var terminator = new RMDAnimationSetTerminator();
            terminator.InternalWrite(writer);
        }
    }
}
