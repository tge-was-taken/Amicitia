namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.IO;
    using IO;
    using AtlusLibSharp.Utilities;
    using System;

    /// <summary>
    /// Base class for RenderWare nodes.
    /// </summary>
    public class RwNode : BinaryBase
    {
        private const uint HEADER_SIZE = 12;
        private RwNodeId mId;
        private uint mRawVersion;
        private RwNode mParent;
        private List<RwNode> mChildren = new List<RwNode>();
        private byte[] mData;
        private uint mSize;

        /// <summary>
        /// RenderWare version of nodes used by Persona 3, Persona 3 FES, and Persona 4.
        /// </summary>
        public const uint VersionPersona3 = 0x1C020037;

        /// <summary>
        /// RenderWare version used when exporting an <see cref="RwNode"/>. Set to <see cref="VersionPersona3"/> by default.
        /// </summary>
        public static uint ExportVersion = VersionPersona3;

        /// <summary>
        /// Gets the RenderWare node id.
        /// </summary>
        public RwNodeId Id
        {
            get { return mId; }
            internal set { mId = value; }
        }

        //public float Version
        //{
        //    get
        //    {
        //        return 3 + ((float)((_rawVersion & 0xFFFF0000) >> 16) / 10000);
        //    }
        //}

        //public uint Revision
        //{
        //    get
        //    {
        //        return (_rawVersion & 0xFFFF);
        //    }

        //    set
        //    {
        //        _rawVersion = (_rawVersion & 0xFFFF0000) | value & 0xFFFF;
        //    }
        //}

        /// <summary>
        /// Gets or sets the RenderWare parent node of this node.
        /// </summary>
        public RwNode Parent
        {
            get { return mParent; }
            internal set
            {
                if (value == null) return;

                if (value.mChildren == null)
                    value.mChildren = new List<RwNode>();

                if (!value.mChildren.Contains(this))
                    value.mChildren.Add(this);

                mParent = value;
            }
        }

        /// <summary>
        /// Gets or sets the list of child nodes of this node.
        /// </summary>
        public List<RwNode> Children
        {
            get { return mChildren; }
            protected set { mChildren = value; }
        }

        /// <summary>
        /// Gets the size of this node.
        /// </summary>
        protected uint Size
        {
            get { return mSize; }
        }

        /// <summary>
        /// Initialize a RenderWare node using the given RenderWare node id.
        /// </summary>
        protected RwNode(RwNodeId id)
        {
            mId = id;
            mSize = 0;
            mRawVersion = ExportVersion;
            mParent = null;
        }

        /// <summary>
        /// Initialize a RenderWare node using the given RenderWare node id and parent node.
        /// </summary>
        protected RwNode(RwNodeId id, RwNode parent)
        {
            mId = id;
            mSize = 0;
            mRawVersion = ExportVersion;
            Parent = parent;
        }

        /// <summary>
        /// Initializer only to be called by <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RwNode(RwNodeFactory.RwNodeHeader header)
        {
            mId = header.Id;
            mSize = header.Size;
            mRawVersion = header.Version;
            Parent = header.Parent;
        }


        /// <summary>
        /// Initializer only to be called by <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RwNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
        {
            mId = header.Id;
            mSize = header.Size;
            mRawVersion = header.Version;
            Parent = header.Parent;
            mData = reader.ReadBytes((int)mSize);

            switch (mId)
            {
                case RwNodeId.RmdParticleListNode:
                    reader.AlignPosition(16);
                    break;
            }
        }

        /// <summary>
        /// Loads a RenderWare file from the given path.
        /// </summary>
        /// <param name="path">Path to the RenderWare file to load.</param>
        /// <returns>RenderWare node loaded from the path.</returns>
        public static RwNode Load(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                return RwNodeFactory.GetNode(null, reader);
        }

        /// <summary>
        /// Loads a RenderWare file from the given stream.
        /// </summary>
        /// <param name="path">Path to the RenderWare file to load.</param>
        /// <param name="leaveOpen">Option to keep the stream open after reading instead of disposing it.</param>
        /// <returns>RenderWare node loaded from the path.</returns>
        public static RwNode Load(Stream stream, bool leaveOpen = false)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen))
                return RwNodeFactory.GetNode(null, reader);
        }

        public static RwNode Load(byte[] data)
        {
            return Load(new MemoryStream(data), false);
        }

        public RwNode FindParentNode(RwNodeId nodeId)
        {
            RwNode node = this;

            while (true)
            {
                var parent = node.Parent;

                if ( parent == null )
                    return null;

                if (parent.Id == nodeId)
                    return parent;

                if ( parent.Parent != null )
                    node = parent;
                else
                    return null;
            }
        }

        public RwNode FindNode(RwNodeId nodeId)
        {
            RwNode FindNode(RwNode node)
            {
                foreach ( var child in node.Children )
                {
                    if ( child.Id == nodeId )
                        return child;
                }

                foreach ( var child in node.Children )
                {
                    var foundNode = FindNode(child);
                    if ( foundNode != null )
                        return foundNode;
                }

                return null;
            }

            return FindNode(this);
        }

        /// <summary>
        /// Inherited from <see cref="BinaryBase"/>. Writes the data to the stream using given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> used to write to the stream.</param>
        internal override void Write(BinaryWriter writer)
        {
            long headerPosition = writer.BaseStream.Position;
            writer.BaseStream.Position += HEADER_SIZE;

            WriteBody(writer);

            // Calculate size of this node
            long endPosition = writer.BaseStream.Position;
            mSize = (uint)(endPosition - (headerPosition + HEADER_SIZE));

            // Seek back to where the header should be, and write it using the calculated size.
            writer.BaseStream.Position = headerPosition;
            writer.Write((uint)mId);
            writer.Write(mSize);
            writer.Write(ExportVersion);

            // Seek to the end of this node
            writer.BaseStream.Position = endPosition;
        }

        /// <summary>
        /// Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal virtual void WriteBody(BinaryWriter writer)
        {
            writer.Write(mData);
        }

        protected internal virtual void ReadBody( BinaryReader reader ) { }
    }
}
