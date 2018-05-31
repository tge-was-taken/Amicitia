namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.Drawing;
    using System.IO;

    /// <summary>
    /// Represents a container for internal RenderWare texture info and data.
    /// </summary>
    internal class RwRasterStructNode : RwNode
    {
        private RwRasterInfoStructNode mRasterInfoStructNode;
        private RwRasterDataStructNode mRasterDataStructNode;

        /// <summary>
        /// Gets or sets the <see cref="RwRasterInfoStructNode"/> containing internal info about the texture.
        /// </summary>
        public RwRasterInfoStructNode InfoStructNode
        {
            get { return mRasterInfoStructNode; }
            set
            {
                mRasterInfoStructNode = value;
                mRasterInfoStructNode.Parent = this;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="RwRasterDataStructNode"/> containing the texture data.
        /// </summary>
        public RwRasterDataStructNode DataStructNode
        {
            get { return mRasterDataStructNode; }
            set
            {
                mRasterDataStructNode = value;
                mRasterDataStructNode.Parent = this;
            }
        }

        /// <summary>
        /// Initialize a new instance of <see cref="RwRasterStructNode"/> using a bitmap to encode using the given pixel format.
        /// </summary>
        /// <param name="bitmap">Bitmap to encode to the specified pixel format.</param>
        /// <param name="pixelFormat">Pixel format to encode the bitmap to.</param>
        /// <param name="parent">Parent of this <see cref="RwRasterStructNode"/> node. Value is null if not specified.</param>
        public RwRasterStructNode(Bitmap bitmap, PS2.Graphics.PS2PixelFormat pixelFormat, RwNode parent = null)
            : base(RwNodeId.RwStructNode, parent)
        {
            InfoStructNode = new RwRasterInfoStructNode(bitmap.Width, bitmap.Height, pixelFormat);
            DataStructNode = new RwRasterDataStructNode(bitmap, pixelFormat);
        }

        /// <summary>
        /// Initialize a new instance of <see cref="RwRasterStructNode"/> using a width, height, palette, pixel indices and pixel format.
        /// </summary>
        /// <param name="width">Width of the texture.</param>
        /// <param name="height">Height of the texture.</param>
        /// <param name="palette">Palette colors of the texture.</param>
        /// <param name="indices">Per-pixel palette color indices of the texture.</param>
        /// <param name="pixelFormat">PS2 pixel format of the given data.</param>
        /// <param name="parent">Parent of this <see cref="RwRasterStructNode"/> node. Value is null if not specified.</param>
        public RwRasterStructNode(int width, int height, Color[] palette, byte[] indices, 
            PS2.Graphics.PS2PixelFormat pixelFormat, RwNode parent = null)
            : base(RwNodeId.RwStructNode, parent)
        {
            InfoStructNode = new RwRasterInfoStructNode(width, height, pixelFormat);
            DataStructNode = new RwRasterDataStructNode(palette, indices, pixelFormat);
        }

        /// <summary>
        /// Initializer only to be called by <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RwRasterStructNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            mRasterInfoStructNode = RwNodeFactory.GetNode<RwRasterInfoStructNode>(this, reader);
            mRasterDataStructNode = RwNodeFactory.GetNode<RwRasterDataStructNode>(this, reader);
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            mRasterInfoStructNode.Write(writer);
            mRasterDataStructNode.Write(writer);
        }
    }
}