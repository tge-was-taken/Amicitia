namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.IO;
    using IO;
    using AmicitiaLibrary.Utilities;
    using System;

    /// <summary>
    /// Base class for RenderWare nodes.
    /// </summary>
    public class RwNode : BinaryBase
    {
        private const uint HEADER_SIZE = 12;
        private RwNode mParent;
        private List<RwNode> mChildren = new List<RwNode>();
        private readonly byte[] mData;
        private uint mSize;

        /// <summary>
        /// RenderWare version of nodes used by Persona 3, Persona 3 FES, and Persona 4.
        /// </summary>
        public const uint VERSION_PERSONA3 = 0x1C020037;

        /// <summary>
        /// RenderWare version of nodes used by Persona 3, Persona 3 FES, and Persona 4. Used for RMD specific nodes.
        /// </summary>
        public const uint VERSION_PERSONA3_ALT = 0x40000000;

        /// <summary>
        /// Gets the RenderWare node id.
        /// </summary>
        public RwNodeId Id { get; internal set; }

        public uint Version { get; set; }
        
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
            Id = id;
            mSize = 0;
            Version = GetVersionForNode(id);
            mParent = null;
        }

        /// <summary>
        /// Initialize a RenderWare node using the given RenderWare node id and parent node.
        /// </summary>
        protected RwNode(RwNodeId id, RwNode parent)
        {
            Id = id;
            mSize = 0;
            Version = GetVersionForNode( id );
            Parent = parent;
        }

        /// <summary>
        /// Initialize a RenderWare node using the given RenderWare node id and parent node.
        /// </summary>
        protected RwNode( RwNodeId id, RwNode parent, uint version )
        {
            Id = id;
            mSize = 0;
            Version = version;
            Parent = parent;
        }

        /// <summary>
        /// Initializer only to be called by <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RwNode(RwNodeFactory.RwNodeHeader header)
        {
            Id = header.Id;
            mSize = header.Size;
            Version = header.Version;
            Parent = header.Parent;
        }


        /// <summary>
        /// Initializer only to be called by <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RwNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
        {
            Id = header.Id;
            mSize = header.Size;
            Version = header.Version;
            Parent = header.Parent;
            mData = reader.ReadBytes((int)mSize);

            switch (Id)
            {
                case RwNodeId.RmdParticleListNode:
                    reader.AlignPosition(16);
                    break;
            }
        }

        private static uint GetVersionForNode( RwNodeId id )
        {
            var version = VERSION_PERSONA3;
            if ( id >= RwNodeId.RmdAnimationPlaceholderNode )
                version = VERSION_PERSONA3_ALT;

            return version;
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

        public void AddChild( RwNode node )
        {
            node.Parent = this;
            Children.Add( node );
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
            writer.Write((uint)Id);
            writer.Write(mSize);
            writer.Write(Version);

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
