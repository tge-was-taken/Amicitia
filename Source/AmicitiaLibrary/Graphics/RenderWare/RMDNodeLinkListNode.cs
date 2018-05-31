using System.Collections;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;

    /// <summary>
    /// Represents a RenderWare node structure containing a list of <see cref="RmdNodeLink"/> used for attaching objects to frames.
    /// </summary>
    public class RmdNodeLinkListNode : RwNode, IList<RmdNodeLink>
    {
        private List<RmdNodeLink> mNodeLinks;

        /// <summary>
        /// Initialize a new <see cref="RmdNodeLinkListNode"/> instance using a list of frame links.
        /// </summary>
        /// <param name="frameLinks"></param>
        public RmdNodeLinkListNode(IList<RmdNodeLink> frameLinks)
            : base(RwNodeId.RmdNodeLinkListNode)
        {
            mNodeLinks = frameLinks.ToList();
        }

        public RmdNodeLinkListNode(Stream stream)
            : base(RwNodeId.RmdNodeLinkListNode)
        {
            using (var reader = new BinaryReader(stream))
            {
                Read(reader);
            }
        }

        /// <summary>
        /// Initialize a new empty <see cref="RmdNodeLinkListNode"/> instance.
        /// </summary>
        /// <param name="frameLinks"></param>
        public RmdNodeLinkListNode()
            : base(RwNodeId.RmdNodeLinkListNode)
        {
            mNodeLinks = new List<RmdNodeLink>();
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RmdNodeLinkListNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            Read(reader);
        }

        protected internal  void Read(BinaryReader reader)
        {
            short numAttachFrames = reader.ReadInt16();

            mNodeLinks = new List<RmdNodeLink>(numAttachFrames);

            for (int i = 0; i < numAttachFrames; i++)
            {
                mNodeLinks.Add(new RmdNodeLink(reader));
            }
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write((short)mNodeLinks.Count);

            foreach (RmdNodeLink attachFrame in mNodeLinks)
            {
                attachFrame.Write(writer);
            }
        }

        // IList<RmdNodeLink> implementation
        public IEnumerator<RmdNodeLink> GetEnumerator()
        {
            return mNodeLinks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) mNodeLinks).GetEnumerator();
        }

        public void Add(RmdNodeLink item)
        {
            mNodeLinks.Add(item);
        }

        public void Clear()
        {
            mNodeLinks.Clear();
        }

        public bool Contains(RmdNodeLink item)
        {
            return mNodeLinks.Contains(item);
        }

        public void CopyTo(RmdNodeLink[] array, int arrayIndex)
        {
            mNodeLinks.CopyTo(array, arrayIndex);
        }

        public bool Remove(RmdNodeLink item)
        {
            return mNodeLinks.Remove(item);
        }

        public int Count
        {
            get { return mNodeLinks.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((IList<RmdNodeLink>) mNodeLinks).IsReadOnly; }
        }

        public int IndexOf(RmdNodeLink item)
        {
            return mNodeLinks.IndexOf(item);
        }

        public void Insert(int index, RmdNodeLink item)
        {
            mNodeLinks.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            mNodeLinks.RemoveAt(index);
        }

        public RmdNodeLink this[int index]
        {
            get { return mNodeLinks[index]; }
            set { mNodeLinks[index] = value; }
        }
    }
}
