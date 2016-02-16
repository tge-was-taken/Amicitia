using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    /// <summary>
    /// Encapsulates a RenderWare material and all of its corresponding data structures.
    /// </summary>
    public class RWMaterial : RWNode
    {
        private RWMaterialStruct _struct;
        private RWTextureReference _textureReference;
        private RWExtension _extension;

        /// <summary>
        /// Gets or sets the material color for this material.
        /// </summary>
        public Color Color
        {
            get { return _struct.Color; }
            set { _struct.Color = value; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether or not the material uses a texture map.
        /// </summary>
        public bool IsTextured
        {
            get { return _textureReference != null; }
        }

        /// <summary>
        /// Gets or sets the material ambient value for this material.
        /// </summary>
        public float Ambient
        {
            get { return _struct.Ambient; }
            set { _struct.Ambient = value; }
        }

        /// <summary>
        /// Gets or sets the material specular value for this material.
        /// </summary>
        public float Specular
        {
            get { return _struct.Specular; }
            set { _struct.Specular = value; }
        }

        /// <summary>
        /// Gets or sets the material diffuse value for this material.
        /// </summary>
        public float Diffuse
        {
            get { return _struct.Diffuse; }
            set { _struct.Diffuse = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="RWTextureReference"/> instance for this material.
        /// </summary>
        public RWTextureReference TextureReference
        {
            get { return _textureReference; }
            set
            {
                _textureReference = value;
                _textureReference.Parent = this;
            }
        }

        /// <summary>
        /// Gets the list of extension nodes for this material.
        /// </summary>
        public List<RWNode> Extension
        {
            get { return _extension.Children; }
        }

        /// <summary>
        /// Initializes a RenderWare material instance with default properties.
        /// </summary>
        public RWMaterial(RWNode parent = null)
            : base(RWType.Material, parent)
        {
            _struct = new RWMaterialStruct(this);
            _struct.IsTextured = false;
            _textureReference = null;
            _extension = new RWExtension(this);
        }

        /// <summary>
        /// Initializes a RenderWare material instance with default properties and reference texture name set.
        /// </summary>
        /// <param name="textureName">Name of the texture to be referenced by the material.</param>
        public RWMaterial(string textureName, RWNode parent = null)
            : this(parent)
        {
            _struct.IsTextured = true;
            _textureReference = new RWTextureReference(textureName, this);
        }

        /// <summary>
        /// Initializes a RenderWare material using data from the <see cref="RWNodeFactory"/>.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="reader"></param>
        internal RWMaterial(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            _struct = RWNodeFactory.GetNode<RWMaterialStruct>(this, reader);

            if (_struct.IsTextured)
                _textureReference = RWNodeFactory.GetNode<RWTextureReference>(this, reader);

            _extension = RWNodeFactory.GetNode<RWExtension>(this, reader);
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            // Update the IsTextured bool in the struct
            _struct.IsTextured = IsTextured;

            // Write it
            _struct.InternalWrite(writer);

            // If the material uses a texture map, write the texture reference
            if (IsTextured)
            {
                _textureReference.InternalWrite(writer);
            }

            // Write the extension
            _extension.InternalWrite(writer);
        }
    }
}