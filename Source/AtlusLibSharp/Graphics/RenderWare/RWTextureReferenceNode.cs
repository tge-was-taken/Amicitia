namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.IO;
    using PS2.Graphics;

    /// <summary>
    /// Encapsulates a RenderWare texture reference and its corresponding data structures.
    /// </summary>
    public class RwTextureReferenceNode : RwNode
    {
        private RwTextureReferenceStruct mStruct;
        private RwStringNode mRefTexName;
        private RwStringNode mRefTexMaskName;
        private RwExtensionNode mExtensionNode;

        #region Properties

        /*************************************************/
        /* RWTextureReferenceStruct forwarded properties */
        /*************************************************/

        /// <summary>
        /// Gets and sets the <see cref="FilterMode"/> of the referenced texture.
        /// </summary>
        public PS2FilterMode FilterMode
        {
            get { return mStruct.FilterMode; }
            set { mStruct.FilterMode = value; }
        }

        /// <summary>
        /// Gets and sets the horizontal (x-axis) <see cref="PS2AddressingMode"/> of the referenced texture.
        /// </summary>
        public PS2AddressingMode HorizontalAddressingMode
        {
            get { return mStruct.HorizontalAdressingMode; }
            set { mStruct.HorizontalAdressingMode = value; }
        }

        /// <summary>
        /// Gets and sets the vertical (y-axis) <see cref="PS2AddressingMode"/> of the referenced texture.
        /// </summary>
        public PS2AddressingMode VerticalAddressingMode
        {
            get { return mStruct.VerticalAdressingMode; }
            set { mStruct.VerticalAdressingMode = value; }
        }

        /// <summary>
        /// Gets and sets the boolean value indicating whether or not the referenced texture uses mipmaps.
        /// </summary>
        public bool HasMipMaps
        {
            get { return mStruct.HasMipMaps; }
            set { mStruct.HasMipMaps = value; }
        }

        /*********************************/
        /* RWString forwarded properties */
        /*********************************/

        /// <summary>
        /// Gets and sets the name of the referenced texture.
        /// </summary>
        public string ReferencedTextureName
        {
            get { return mRefTexName.Value; }
            set { mRefTexName = new RwStringNode(value, this);  }
        }

        /// <summary>
        /// (Unused) Gets and sets the name of the referenced texture alpha mask.
        /// </summary>
        public string ReferencedTextureMaskName
        {
            get { return mRefTexMaskName.Value; }
            set { mRefTexMaskName = new RwStringNode(value, this); }
        }

        /****************************/
        /* Non-forwarded properties */
        /****************************/

        /// <summary>
        /// Gets the list of extension nodes applied to this texture reference node. 
        /// </summary>
        public List<RwNode> Extensions
        {
            get { return mExtensionNode.Children; }
        }

        #endregion

        /// <summary>
        /// Initialize a texture reference instance with a referenced texture name.
        /// </summary>
        /// <param name="refTextureName">Name of the referenced texture.</param>
        /// <param name="parent">Parent of the texture reference node. Value is null if not specified.</param>
        public RwTextureReferenceNode(string refTextureName, RwNode parent = null)
            : base(RwNodeId.RwTextureReferenceNode, parent)
        {
            // Create a struct with default values
            mStruct = new RwTextureReferenceStruct(this);
            mRefTexName = new RwStringNode(refTextureName, this);
            mRefTexMaskName = new RwStringNode(string.Empty, this);
            mExtensionNode = new RwExtensionNode(this, new RwSkyMipMapValueNode());
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RwNodeFactory.GetNode(RwNode, BinaryReader)"/>
        /// </summary>
        internal RwTextureReferenceNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            mStruct = RwNodeFactory.GetNode<RwTextureReferenceStruct>(this, reader);
            mRefTexName = RwNodeFactory.GetNode<RwStringNode>(this, reader);
            mRefTexMaskName = RwNodeFactory.GetNode<RwStringNode>(this, reader);
            mExtensionNode = RwNodeFactory.GetNode<RwExtensionNode>(this, reader);
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            mStruct.Write(writer);
            mRefTexName.Write(writer);
            mRefTexMaskName.Write(writer);
            mExtensionNode.Write(writer);
        }
    }
}