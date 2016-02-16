namespace AtlusLibSharp.Persona3.RenderWare
{
    using System.Collections.Generic;
    using System.IO;
    using Common;

    /// <summary>
    /// Base class for RenderWare nodes.
    /// </summary>
    public class RWNode : BinaryFileBase
    {
        private const uint HEADER_SIZE = 12;
        private RWType _type;
        private uint _rawVersion;
        private RWNode _parent;
        private List<RWNode> _children;
        private byte[] _data;
        private uint _size;

        /// <summary>
        /// RenderWare version of nodes used by Persona 3, Persona 3 FES, and Persona 4.
        /// </summary>
        public const uint VersionPersona3 = 0x1C020037;

        /// <summary>
        /// RenderWare version used when exporting an <see cref="RWNode"/>. Set to <see cref="VersionPersona3"/> by default.
        /// </summary>
        public static uint ExportVersion = VersionPersona3;

        /// <summary>
        /// Gets the RenderWare node type.
        /// </summary>
        public RWType Type
        {
            get { return _type; }
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
        public RWNode Parent
        {
            get { return _parent; }
            internal set
            {
                if (value == null) return;

                if (value._children == null)
                    value._children = new List<RWNode>();

                if (!value._children.Contains(this))
                    value._children.Add(this);

                _parent = value;
            }
        }

        /// <summary>
        /// Gets or sets the list of child nodes of this node.
        /// </summary>
        public List<RWNode> Children
        {
            get { return _children; }
            protected set { _children = value; }
        }

        /// <summary>
        /// Gets the size of this node.
        /// </summary>
        protected uint Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Initialize a RenderWare node using the given RenderWare node type.
        /// </summary>
        protected RWNode(RWType type)
        {
            _type = type;
            _size = 0;
            _rawVersion = ExportVersion;
            _parent = null;
        }

        /// <summary>
        /// Initialize a RenderWare node using the given RenderWare node type and parent node.
        /// </summary>
        protected RWNode(RWType type, RWNode parent)
        {
            _type = type;
            _size = 0;
            _rawVersion = ExportVersion;
            _parent = parent;
        }

        /// <summary>
        /// Initializer only to be called by <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RWNode(RWNodeFactory.RWNodeProcHeader header)
        {
            _type = header.Type;
            _size = header.Size;
            _rawVersion = header.Version;
            Parent = header.Parent;
        }


        /// <summary>
        /// Initializer only to be called by <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RWNode(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
        {
            _type = header.Type;
            _size = header.Size;
            _rawVersion = header.Version;
            Parent = header.Parent;
            _data = reader.ReadBytes((int)_size);
        }

        /// <summary>
        /// Loads a RenderWare file from the given path.
        /// </summary>
        /// <param name="path">Path to the RenderWare file to load.</param>
        /// <returns>RenderWare node loaded from the path.</returns>
        public static RWNode LoadFromFile(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                return RWNodeFactory.GetNode(null, reader);
        }

        /// <summary>
        /// Loads a RenderWare file from the given stream.
        /// </summary>
        /// <param name="path">Path to the RenderWare file to load.</param>
        /// <param name="leaveOpen">Option to keep the stream open after reading instead of disposing it.</param>
        /// <returns>RenderWare node loaded from the path.</returns>
        public static RWNode LoadFromStream(Stream stream, bool leaveOpen = false)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen))
                return RWNodeFactory.GetNode(null, reader);
        }

        /// <summary>
        /// Inherited from <see cref="BinaryFileBase"/>. Writes the data to the stream using given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> used to write to the stream.</param>
        internal override void InternalWrite(BinaryWriter writer)
        {
            long headerPosition = writer.BaseStream.Position;
            writer.BaseStream.Position += HEADER_SIZE;

            InternalWriteInnerData(writer);

            // Calculate size of this node
            long endPosition = writer.BaseStream.Position;
            _size = (uint)(endPosition - (headerPosition + HEADER_SIZE));

            // Seek back to where the header should be, and write it using the calculated size.
            writer.BaseStream.Position = headerPosition;
            writer.Write((uint)_type);
            writer.Write(_size);
            writer.Write(ExportVersion);

            // Seek to the end of this node
            writer.BaseStream.Position = endPosition;
        }

        /// <summary>
        /// Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal virtual void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(_data);
        }
    }
}
