using System.Collections;

namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a RenderWare node set which get used to display a single animation. 
    /// </summary>
    public class RmdAnimation : RwNode, IList<RwNode>
    {
        private List<RwNode> mAnimationNodes;

        /// <summary>
        /// Initialize a new <see cref="RmdAnimation"/> instance using a <see cref="List{T}"/> of RenderWare nodes.
        /// </summary>
        /// <param name="animationNodes"><see cref="List{T}"/> of <see cref="RwNode"/> to initialize the animation node set with.</param>
        /// <param name="parent">The parent of the new <see cref="RmdAnimation"/>. Value is null if not specified.</param>
        public RmdAnimation(List<RwNode> animationNodes, RwNode parent = null)
            : base(RwNodeId.RmdAnimation, parent)
        {
            mAnimationNodes = animationNodes;

            for (int i = 0; i < mAnimationNodes.Count; i++)
            {
                mAnimationNodes[i].Parent = this;
            }
        }

        public RmdAnimation( Stream stream, bool leaveOpen = false )
            : this(new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen))
        {
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RmdAnimation(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            ReadBody( reader );
        }

        internal RmdAnimation(BinaryReader reader)
            : base(RwNodeFactory.ReadHeader(reader, null))
        {
            ReadBody( reader );
        }

        protected internal override void ReadBody( BinaryReader reader )
        {
            mAnimationNodes = new List<RwNode>();

            var node = RwNodeFactory.GetNode( this, reader );
            while ( node.Id != RwNodeId.RmdAnimationTerminatorNode )
            {
                mAnimationNodes.Add( node );
                node = RwNodeFactory.GetNode( this, reader );
            }
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            // Write the animation nodes
            foreach (RwNode animationNode in mAnimationNodes)
            {
                animationNode.Write(writer);
            }

            // Write the animation set terminator
            var terminator = new RmdAnimationTerminatorNode(this);
            terminator.Write(writer);
        }

        public IEnumerator<RwNode> GetEnumerator()
        {
            return mAnimationNodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) mAnimationNodes).GetEnumerator();
        }

        public void Add(RwNode item)
        {
            mAnimationNodes.Add(item);
        }

        public void Clear()
        {
            mAnimationNodes.Clear();
        }

        public bool Contains(RwNode item)
        {
            return mAnimationNodes.Contains(item);
        }

        public void CopyTo(RwNode[] array, int arrayIndex)
        {
            mAnimationNodes.CopyTo(array, arrayIndex);
        }

        public bool Remove(RwNode item)
        {
            return mAnimationNodes.Remove(item);
        }

        public int Count
        {
            get { return mAnimationNodes.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((ICollection<RwNode>) mAnimationNodes).IsReadOnly; }
        }

        public int IndexOf(RwNode item)
        {
            return mAnimationNodes.IndexOf(item);
        }

        public void Insert(int index, RwNode item)
        {
            mAnimationNodes.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            mAnimationNodes.RemoveAt(index);
        }

        public RwNode this[int index]
        {
            get { return mAnimationNodes[index]; }
            set { mAnimationNodes[index] = value; }
        }
    }
}
