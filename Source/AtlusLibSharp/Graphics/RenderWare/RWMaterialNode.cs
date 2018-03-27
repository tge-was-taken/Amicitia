using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace AtlusLibSharp.Graphics.RenderWare
{
    /// <summary>
    /// Encapsulates a RenderWare material and all of its corresponding data structures.
    /// </summary>
    public class RwMaterial : RwNode
    {
        private RwMaterialStructNode mStructNode;
        private RwTextureReferenceNode mTextureReferenceNode;
        private RwExtensionNode mExtensionNode;

        /// <summary>
        /// Gets or sets the material color for this material.
        /// </summary>
        public Color Color
        {
            get { return mStructNode.Color; }
            set { mStructNode.Color = value; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether or not the material uses a texture map.
        /// </summary>
        public bool IsTextured
        {
            get { return mTextureReferenceNode != null; }
        }

        /// <summary>
        /// Gets or sets the material ambient value for this material.
        /// </summary>
        public float Ambient
        {
            get { return mStructNode.Ambient; }
            set { mStructNode.Ambient = value; }
        }

        /// <summary>
        /// Gets or sets the material specular value for this material.
        /// </summary>
        public float Specular
        {
            get { return mStructNode.Specular; }
            set { mStructNode.Specular = value; }
        }

        /// <summary>
        /// Gets or sets the material diffuse value for this material.
        /// </summary>
        public float Diffuse
        {
            get { return mStructNode.Diffuse; }
            set { mStructNode.Diffuse = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="RwTextureReferenceNode"/> instance for this material.
        /// </summary>
        public RwTextureReferenceNode TextureReferenceNode
        {
            get { return mTextureReferenceNode; }
            set
            {
                mTextureReferenceNode = value;
                mTextureReferenceNode.Parent = this;
            }
        }

        /// <summary>
        /// Gets the list of extension nodes for this material.
        /// </summary>
        public List<RwNode> Extension
        {
            get { return mExtensionNode.Children; }
        }

        /// <summary>
        /// Initializes a RenderWare material instance with default properties.
        /// </summary>
        public RwMaterial(RwNode parent = null)
            : base(RwNodeId.RwMaterialNode, parent)
        {
            mStructNode = new RwMaterialStructNode(this) {IsTextured = false};
            mTextureReferenceNode = null;
            mExtensionNode = new RwExtensionNode(this);
        }

        /// <summary>
        /// Initializes a RenderWare material instance with default properties and reference texture name set.
        /// </summary>
        /// <param name="textureName">Name of the texture to be referenced by the material.</param>
        public RwMaterial(string textureName, RwNode parent = null)
            : this(parent)
        {
            mStructNode.IsTextured = true;
            mTextureReferenceNode = new RwTextureReferenceNode(textureName, this);
        }

        /// <summary>
        /// Initializes a RenderWare material using data from the <see cref="RwNodeFactory"/>.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="reader"></param>
        internal RwMaterial(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            mStructNode = RwNodeFactory.GetNode<RwMaterialStructNode>(this, reader);

            if (mStructNode.IsTextured)
                mTextureReferenceNode = RwNodeFactory.GetNode<RwTextureReferenceNode>(this, reader);

            mExtensionNode = RwNodeFactory.GetNode<RwExtensionNode>(this, reader);
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            // Update the IsTextured bool in the struct
            mStructNode.IsTextured = IsTextured;

            // Write it
            mStructNode.Write(writer);

            // If the material uses a texture map, write the texture reference
            if (IsTextured)
            {
                mTextureReferenceNode.Write(writer);
            }

            // Write the extension
            mExtensionNode.Write(writer);
        }
    }
}