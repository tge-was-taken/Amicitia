namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.IO;
    using PS2.Graphics;

    /// <summary>
    /// Encapsulates a RenderWare texture reference and its corresponding data structures.
    /// </summary>
    public class RWTextureReference : RWNode
    {
        private RWTextureReferenceStruct _struct;
        private RWString _refTexName;
        private RWString _refTexMaskName;
        private RWExtension _extension;

        #region Properties

        /*************************************************/
        /* RWTextureReferenceStruct forwarded properties */
        /*************************************************/

        /// <summary>
        /// Gets and sets the <see cref="FilterMode"/> of the referenced texture.
        /// </summary>
        public PS2FilterMode FilterMode
        {
            get { return _struct.FilterMode; }
            set { _struct.FilterMode = value; }
        }

        /// <summary>
        /// Gets and sets the horizontal (x-axis) <see cref="PS2AddressingMode"/> of the referenced texture.
        /// </summary>
        public PS2AddressingMode HorizontalAddressingMode
        {
            get { return _struct.HorizontalAdressingMode; }
            set { _struct.HorizontalAdressingMode = value; }
        }

        /// <summary>
        /// Gets and sets the vertical (y-axis) <see cref="PS2AddressingMode"/> of the referenced texture.
        /// </summary>
        public PS2AddressingMode VerticalAddressingMode
        {
            get { return _struct.VerticalAdressingMode; }
            set { _struct.VerticalAdressingMode = value; }
        }

        /// <summary>
        /// Gets and sets the boolean value indicating whether or not the referenced texture uses mipmaps.
        /// </summary>
        public bool HasMipMaps
        {
            get { return _struct.HasMipMaps; }
            set { _struct.HasMipMaps = value; }
        }

        /*********************************/
        /* RWString forwarded properties */
        /*********************************/

        /// <summary>
        /// Gets and sets the name of the referenced texture.
        /// </summary>
        public string ReferencedTextureName
        {
            get { return _refTexName.Value; }
            set { _refTexName = new RWString(value, this);  }
        }

        /// <summary>
        /// (Unused) Gets and sets the name of the referenced texture alpha mask.
        /// </summary>
        public string ReferencedTextureMaskName
        {
            get { return _refTexMaskName.Value; }
            set { _refTexMaskName = new RWString(value, this); }
        }

        /****************************/
        /* Non-forwarded properties */
        /****************************/

        /// <summary>
        /// Gets the list of extension nodes applied to this texture reference node. 
        /// </summary>
        public List<RWNode> Extensions
        {
            get { return _extension.Children; }
        }

        #endregion

        /// <summary>
        /// Initialize a texture reference instance with a referenced texture name.
        /// </summary>
        /// <param name="refTextureName">Name of the referenced texture.</param>
        /// <param name="parent">Parent of the texture reference node. Value is null if not specified.</param>
        public RWTextureReference(string refTextureName, RWNode parent = null)
            : base(RWNodeType.TextureReference, parent)
        {
            // Create a struct with default values
            _struct = new RWTextureReferenceStruct(this);
            _refTexName = new RWString(refTextureName, this);
            _refTexMaskName = new RWString(string.Empty, this);
            _extension = new RWExtension(new RWSkyMipMapValue());
            _extension.Parent = this;
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RWNodeFactory.GetNode(RWNode, BinaryReader)"/>
        /// </summary>
        internal RWTextureReference(RWNodeFactory.RWNodeInfo header, BinaryReader reader)
            : base(header)
        {
            _struct = RWNodeFactory.GetNode<RWTextureReferenceStruct>(this, reader);
            _refTexName = RWNodeFactory.GetNode<RWString>(this, reader);
            _refTexMaskName = RWNodeFactory.GetNode<RWString>(this, reader);
            _extension = RWNodeFactory.GetNode<RWExtension>(this, reader);
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            _struct.InternalWrite(writer);
            _refTexName.InternalWrite(writer);
            _refTexMaskName.InternalWrite(writer);
            _extension.InternalWrite(writer);
        }
    }
}