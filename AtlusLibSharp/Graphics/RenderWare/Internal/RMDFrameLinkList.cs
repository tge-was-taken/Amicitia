namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using OpenTK;
    using AtlusLibSharp.Utilities;

    /// <summary>
    /// Represents a RenderWare node structure containing a list of <see cref="RMDFrameLink"/> used for attaching objects to frames.
    /// </summary>
    internal class RMDFrameLinkList : RWNode
    {
        private List<RMDFrameLink> _frameLinks;

        /// <summary>
        /// Gets the number of frame links in the <see cref="RMDFrameLinkList"/>.
        /// </summary>
        public int FrameLinkCount
        {
            get { return _frameLinks.Count; }
        }

        /// <summary>
        /// Gets the list of frame links in the <see cref="RMDFrameLinkList"/>.
        /// </summary>
        public List<RMDFrameLink> FrameLinks
        {
            get { return _frameLinks; }
        }

        /// <summary>
        /// Initialize a new <see cref="RMDFrameLinkList"/> instance using a list of frame links.
        /// </summary>
        /// <param name="frameLinks"></param>
        public RMDFrameLinkList(IList<RMDFrameLink> frameLinks)
            : base(RWType.RMDFrameLinkList)
        {
            _frameLinks = frameLinks.ToList();
        }

        /// <summary>
        /// Initialize a new empty <see cref="RMDFrameLinkList"/> instance.
        /// </summary>
        /// <param name="frameLinks"></param>
        public RMDFrameLinkList()
            : base(RWType.RMDFrameLinkList)
        {
            _frameLinks = new List<RMDFrameLink>();
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RMDFrameLinkList(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            short numAttachFrames = reader.ReadInt16();

            _frameLinks = new List<RMDFrameLink>(numAttachFrames);

            for (int i = 0; i < numAttachFrames; i++)
            {
                _frameLinks.Add(new RMDFrameLink(reader));
            }
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write((short)FrameLinkCount);

            foreach (RMDFrameLink attachFrame in _frameLinks)
            {
                attachFrame.InternalWrite(writer);
            }
        }
    }

    /// <summary>
    /// Used to eg. attach weapons to frames by specifying which frame on the source model attaches to a frame on the weapon model.
    /// <para>It also provides a matrix pivot point for the weapon (or any other object) to orient itself around.</para>
    /// </summary>
    public class RMDFrameLink
    {
        private int _frameANameID;
        private int _frameBNameID;
        private Matrix4 _matrix;

        public int FrameANameID
        {
            get { return _frameANameID; }
            set { _frameANameID = value; }
        }

        public int FrameBNameID
        {
            get { return _frameBNameID; }
            set { _frameBNameID = value; }
        }

        /// <summary>
        /// Gets or sets the matrix pivot point of the attachment link.
        /// </summary>
        public Matrix4 Matrix
        {
            get { return _matrix; }
            set { _matrix = value; }
        }

        /// <summary>
        /// Initialize a new empty <see cref="RMDFrameLink"/> instance.
        /// </summary>
        public RMDFrameLink()
        {
            _frameANameID = 0;
            _frameBNameID = 0;
            _matrix = Matrix4.Identity;
        }

        /// <summary>
        /// Initialize a <see cref="RMDFrameLink"/> by reading the data from a <see cref="Stream"/> using a <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/> used to read the data from the <see cref="Stream"/>.</param>
        internal RMDFrameLink(BinaryReader reader)
        {
            _frameANameID = reader.ReadInt32();
            _frameBNameID = reader.ReadInt32();
            _matrix = reader.ReadMatrix4();
        }

        /// <summary>
        /// Write the <see cref="RMDFrameLink"/> instance to the stream using a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> used to write to the stream.</param>
        internal void InternalWrite(BinaryWriter writer)
        {
            writer.Write(_frameANameID);
            writer.Write(_frameBNameID);
            writer.Write(_matrix);
        }
    }
}
