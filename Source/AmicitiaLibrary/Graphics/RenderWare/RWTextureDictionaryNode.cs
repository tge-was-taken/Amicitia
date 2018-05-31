namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Encapsulates a RenderWare texture dictionary and all of its corresponding data structures.
    /// </summary>
    public class RwTextureDictionaryNode : RwNode
    {
        private RwTextureDictionaryStructNode mStructNode;
        private List<RwTextureNativeNode> mTextures;
        private RwExtensionNode mExtensionNode;

        /// <summary>
        /// Gets the <see cref="RwDeviceId"/> of this texture dictionary. Used to indicate for which platform the data is compiled.
        /// </summary>
        public RwDeviceId DeviceId
        {
            get
            {
                if (mStructNode != null)
                    return mStructNode.DeviceId;
                else
                    return RwDeviceId.PS2;
            }
        }

        /// <summary>
        /// Gets the amount of textures contained in this texture dictionary.
        /// </summary>
        public int TextureCount
        {
            get
            {
                if (mTextures != null)
                    return mTextures.Count;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Gets the list of texture nodes contained in this texture dictionary.
        /// </summary>
        public List<RwTextureNativeNode> Textures
        {
            get { return mTextures; }
            set
            {
                mTextures = value;

                if (mTextures == null)
                    return;

                for (int i = 0; i < mTextures.Count; i++)
                {
                    if (mTextures[i] != null)
                        mTextures[i].Parent = this;
                }
            }
        }

        /// <summary>
        /// Gets the list of extension nodes applied to this texture dictionary.
        /// </summary>
        public List<RwNode> ExtensionNodes
        {
            get
            {
                if (mExtensionNode != null)
                    return mExtensionNode.Children;
                else
                    return null;
            }
        }

        /// <summary>
        /// Initialize a new <see cref="RwTextureDictionaryNode"/> instance with an <see cref="IList{T}"/> of texture nodes.
        /// </summary>
        /// <param name="textures"><see cref="IList{T}"/>containing texture nodes to initialize the dictionary with.</param> 
        public RwTextureDictionaryNode(IList<RwTextureNativeNode> textures)
            : base(RwNodeId.RwTextureDictionaryNode)
        {
            Textures = textures.ToList();
            mExtensionNode = new RwExtensionNode(this);
            mStructNode = new RwTextureDictionaryStructNode(this);
        }

        /// <summary>
        /// Initialize a new empty <see cref="RwTextureDictionaryNode"/> instance.
        /// </summary>
        public RwTextureDictionaryNode()
            : base(RwNodeId.RwTextureDictionaryNode)
        {
            mStructNode = new RwTextureDictionaryStructNode(this);
            mTextures = new List<RwTextureNativeNode>();
            mExtensionNode = new RwExtensionNode(this);
        }

        public RwTextureDictionaryNode(Stream stream, bool leaveOpen)
            : base(RwNodeId.RwTextureDictionaryNode)
        {
            var node = Load(stream, leaveOpen) as RwTextureDictionaryNode;
            if (node == null)
                throw new System.Exception();

            mStructNode = node.mStructNode;
            mTextures = node.mTextures;
            mExtensionNode = node.mExtensionNode;
        }

        /// <summary>
        /// Constructor only to be called in <see cref="RwNodeFactory.GetNode(RwNode, BinaryReader)"/>.
        /// </summary>
        internal RwTextureDictionaryNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            mStructNode = RwNodeFactory.GetNode<RwTextureDictionaryStructNode>(this, reader);
            mTextures = new List<RwTextureNativeNode>(mStructNode.TextureCount);

            for (int i = 0; i < mStructNode.TextureCount; i++)
            {
                mTextures.Add(RwNodeFactory.GetNode<RwTextureNativeNode>(this, reader));
            }

            mExtensionNode = RwNodeFactory.GetNode<RwExtensionNode>(this, reader);
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            // Update the texture count in the struct
            mStructNode.TextureCount = (ushort)TextureCount;

            // And write the updated struct to the stream
            mStructNode.Write(writer);

            // Write textures
            foreach (RwTextureNativeNode texture in mTextures)
            {
                texture.Write(writer);
            }

            // Write extension
            mExtensionNode.Write(writer);
        }
    }
}
