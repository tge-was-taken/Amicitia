namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Numerics;
    using Utilities;
    using IO;

    /// <summary>
    /// Represents a RenderWare node structure containing a list of <see cref="RMDNodeLink"/> used for attaching objects to frames.
    /// </summary>
    internal class RMDFrameLinkList : RWNode
    {
        private List<RMDNodeLink> _frameLinks;

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
        public List<RMDNodeLink> FrameLinks
        {
            get { return _frameLinks; }
        }

        /// <summary>
        /// Initialize a new <see cref="RMDFrameLinkList"/> instance using a list of frame links.
        /// </summary>
        /// <param name="frameLinks"></param>
        public RMDFrameLinkList(IList<RMDNodeLink> frameLinks)
            : base(RWNodeType.RMDFrameLinkList)
        {
            _frameLinks = frameLinks.ToList();
        }

        /// <summary>
        /// Initialize a new empty <see cref="RMDFrameLinkList"/> instance.
        /// </summary>
        /// <param name="frameLinks"></param>
        public RMDFrameLinkList()
            : base(RWNodeType.RMDFrameLinkList)
        {
            _frameLinks = new List<RMDNodeLink>();
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RMDFrameLinkList(RWNodeFactory.RWNodeInfo header, BinaryReader reader)
            : base(header)
        {
            short numAttachFrames = reader.ReadInt16();

            _frameLinks = new List<RMDNodeLink>(numAttachFrames);

            for (int i = 0; i < numAttachFrames; i++)
            {
                _frameLinks.Add(new RMDNodeLink(reader));
            }
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write((short)FrameLinkCount);

            foreach (RMDNodeLink attachFrame in _frameLinks)
            {
                attachFrame.InternalWrite(writer);
            }
        }
    }

    /// <summary>
    /// Used to eg. attach weapons to frames by specifying which frame on the source model attaches to a frame on the weapon model.
    /// <para>It also provides a matrix pivot point for the weapon (or any other object) to orient itself around.</para>
    /// </summary>
    public class RMDNodeLink : BinaryFileBase
    {
        private int _frameANameID;
        private int _frameBNameID;
        private Matrix4x4 _matrix;

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
        public Matrix4x4 Matrix
        {
            get { return _matrix; }
            set { _matrix = value; }
        }

        /// <summary>
        /// Initialize a new empty <see cref="RMDNodeLink"/> instance.
        /// </summary>
        public RMDNodeLink()
        {
            _frameANameID = 0;
            _frameBNameID = 0;
            _matrix = Matrix4x4.Identity;
        }

        public RMDNodeLink(byte[] data)
            : this(new MemoryStream(data))
        {
        }

        public RMDNodeLink(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                InternalRead(reader);
            }
        }

        /// <summary>
        /// Initialize a <see cref="RMDNodeLink"/> by reading the data from a <see cref="Stream"/> using a <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/> used to read the data from the <see cref="Stream"/>.</param>
        internal RMDNodeLink(BinaryReader reader)
        {
            InternalRead(reader);
        }

        internal void InternalRead(BinaryReader reader)
        {
            _frameANameID = reader.ReadInt32();
            _frameBNameID = reader.ReadInt32();
            _matrix = reader.ReadMatrix4();
        }

        /// <summary>
        /// Write the <see cref="RMDNodeLink"/> instance to the stream using a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> used to write to the stream.</param>
        internal override void InternalWrite(BinaryWriter writer)
        {
            writer.Write(_frameANameID);
            writer.Write(_frameBNameID);
            writer.Write(_matrix, false);
        }
    }
}
