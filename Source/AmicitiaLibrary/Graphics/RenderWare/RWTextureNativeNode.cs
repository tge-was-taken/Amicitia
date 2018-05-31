using AmicitiaLibrary.Utilities;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.Drawing;
    using System.IO;
    using PS2.Graphics;
    using AmicitiaLibrary.Utilities;
    using System.Collections.Generic;
    using IO;
    using System;
    using System.Runtime.InteropServices;
    /// <summary>
    /// Encapsulates a RenderWare texture node and all of its corresponding data structures.
    /// </summary>
    public class RwTextureNativeNode : RwNode, ITextureFile
    {
        private const string EXCEPTION_NOT_POW2 = "The image dimensions have to be a power of 2.";
        private const string EXCEPTION_DIMENSION_TOO_BIG = "The image can not be larger than 1024 pixels in width or height.";

        private RwTextureNativeStructNode mStructNode;
        private RwStringNode mName;
        private RwStringNode mMaskName;
        private RwRasterStructNode mRasterStructNode;
        private RwExtensionNode mExtensionNode;
        private Bitmap mBitmap;

        /// <summary>
        /// Property exposed for use in <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RwTextureNativeStructNode StructNode
        {
            get { return mStructNode; }
        }

        /// <summary>
        /// Property exposed for use in <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RwRasterStructNode RasterStructNode
        {
            get { return mRasterStructNode; }
        }

        /// <summary>
        /// Gets if the texture node uses an indexed format with indices and a palette.
        /// </summary>
        public bool IsIndexed
        {
            get { return PS2PixelFormatHelper.IsIndexedPixelFormat(Tex0Register.TexturePixelFormat); }
        }

        /// <summary>
        /// Gets the <see cref="RwPlatformId"/> of this texture node, indicating the platform for which the data has been compiled.
        /// </summary>
        public RwPlatformId PlatformId
        {
            get { return mStructNode.PlatformId; }
        }

        /// <summary>
        /// Gets and sets the <see cref="PS2.Graphics.PS2FilterMode"/> of this texture node.
        /// </summary>
        public PS2FilterMode FilterMode
        {
            get { return mStructNode.FilterMode; }
            set { mStructNode.FilterMode = value; }
        }

        /// <summary>
        /// Gets and sets the horizontal <see cref="PS2AddressingMode"/> of this texture node used to control texture wrapping.
        /// </summary>
        public PS2AddressingMode HorrizontalAddressingMode
        {
            get { return mStructNode.HorizontalAddressingMode; }
            set { mStructNode.HorizontalAddressingMode = value; }
        }

        /// <summary>
        /// Gets and sets the vertical <see cref="PS2AddressingMode"/> of this texture node used to control texture wrapping.
        /// </summary>
        public PS2AddressingMode VerticalAddressingMode
        {
            get { return mStructNode.VerticalAddressingMode; }
            set { mStructNode.VerticalAddressingMode = value; }
        }

        /// <summary>
        /// Gets and sets the name of this texture node.
        /// </summary>
        public string Name
        {
            get { return mName.Value; }
            set { mName = new RwStringNode(value, this); }
        }

        /// <summary>
        /// (Unused) Gets and sets the name of the alpha mask applied to this texture node.
        /// </summary>
        public string MaskName
        {
            get { return mMaskName.Value; }
            set { mMaskName = new RwStringNode(value, this); }
        }

        /// <summary>
        /// Gets the texture width of this texture node.
        /// </summary>
        public int Width
        {
            get { return mRasterStructNode.InfoStructNode.Width; }
        }

        /// <summary>
        /// Gets the texture height of this texture node.
        /// </summary>
        public int Height
        {
            get { return mRasterStructNode.InfoStructNode.Height; }
        }
        
        /// <summary>
        /// Gets the texture depth of this texture node.
        /// </summary>
        public int Depth
        {
            get { return mRasterStructNode.InfoStructNode.Depth; }
        }

        /// <summary>
        /// Gets the <see cref="RwRasterFormats"/> of this texture node.
        /// </summary>
        public RwRasterFormats Format
        {
            get { return mRasterStructNode.InfoStructNode.Format; }
        }

        /// <summary>
        /// Gets the PS2 <see cref="PS2.Graphics.Registers.Tex0Register"/> settings of this texture node.
        /// </summary>
        public PS2.Graphics.Registers.Tex0Register Tex0Register
        {
            get { return mRasterStructNode.InfoStructNode.Tex0Register; }
        }

        /// <summary>
        /// Gets the PS2 <see cref="PS2.Graphics.Registers.Tex1Register"/> settings of this texture node.
        /// </summary>
        public PS2.Graphics.Registers.Tex1Register Tex1Register
        {
            get { return mRasterStructNode.InfoStructNode.Tex1Register; }
        }

        /// <summary>
        /// Gets the per-pixel palette color indices of this texture node. Returns null if the texture node is not indexed.
        /// </summary>
        public byte[] PixelIndices
        {
            get { return mRasterStructNode.DataStructNode.PixelIndices; }
        }

        /// <summary>
        /// Gets the color palette of this texture node. Returns null if the texture node is not indexed.
        /// </summary>
        public Color[] Palette
        {
            get { return mRasterStructNode.DataStructNode.Palette; }
        }

        /// <summary>
        /// Gets the pixels of this texture node. Returns null if the texture node is indexed.
        /// </summary>
        public Color[] Pixels
        {
            get { return mRasterStructNode.DataStructNode.Pixels; }
        }

        /// <summary>
        /// Gets the list of extension nodes applied to this texture node.
        /// </summary>
        public List<RwNode> Extensions
        {
            get { return mExtensionNode.Children; }
        }

        /// <summary>
        /// Initializes an <see cref="RwTextureNativeNode"/> instance using a path to a bitmap and a PS2 pixel format to encode to.
        /// </summary>
        /// <param name="path">Path to a bitmap.</param>
        /// <param name="pixelFormat">PS2 Pixel format to encode the bitmap to.</param>
        public RwTextureNativeNode(string path, PS2PixelFormat pixelFormat) 
            : this(Path.GetFileNameWithoutExtension(path), new Bitmap(path), pixelFormat)
        {

        }

        public RwTextureNativeNode(string path)
            : this(Path.GetFileNameWithoutExtension(path), new Bitmap(path))
        {
            
        }

        /// <summary>
        /// Initializes an <see cref="RwTextureNativeNode"/> instance using a bitmap, name and a PS2 pixel format to encode to.
        /// </summary>
        /// <param name="name">Name of the texture used for material references.</param>
        /// <param name="bitmap">Source bitmap used to encode.</param>
        /// <param name="pixelFormat">PS2 Pixel format to encode the bitmap to.</param>
        public RwTextureNativeNode(string name, Bitmap bitmap, PS2PixelFormat pixelFormat)
            : base(RwNodeId.RwTextureNativeNode)
        {
            if (bitmap.Width % 2 != 0 || bitmap.Height % 2 != 0)
            {
                throw new ArgumentException(EXCEPTION_NOT_POW2);
            }

            if (bitmap.Width > 1024 || bitmap.Width > 1024)
            {
                throw new ArgumentException(EXCEPTION_DIMENSION_TOO_BIG);
            }

            mStructNode = new RwTextureNativeStructNode(this);
            mName = new RwStringNode(name, this);
            mMaskName = new RwStringNode(string.Empty, this);
            mRasterStructNode = new RwRasterStructNode(bitmap, pixelFormat, this);
            mExtensionNode = new RwExtensionNode(this, new RwSkyMipMapValueNode());
        }

        public RwTextureNativeNode(string name, Bitmap bitmap)
            : this(name, bitmap, PS2PixelFormatHelper.GetBestPixelFormat(bitmap))
        {
            
        }

        /// <summary>
        /// Initializes an <see cref="RwTextureNativeNode"/> instance using a name, width, height, palette, indices and a PS2 pixel format to encode to.
        /// </summary>
        /// <param name="name">Name of the texture used for material references.</param>
        /// <param name="width">Width of the texture.</param>
        /// <param name="height">Height of the texture.</param>
        /// <param name="palette">Texture palette data.</param>
        /// <param name="indices">Texture pixel indices into the palette.</param>
        /// <param name="pixelFormat">PS2 Pixel format to encode the bitmap to.</param>
        public RwTextureNativeNode(string name, int width, int height, Color[] palette, byte[] indices, PS2PixelFormat pixelFormat)
            : base(RwNodeId.RwTextureNativeNode)
        {
            if (width % 2 != 0 || height % 2 != 0)
            {
                throw new ArgumentException(EXCEPTION_NOT_POW2);
            }

            if (width > 1024 || width > 1024)
            {
                throw new ArgumentException(EXCEPTION_DIMENSION_TOO_BIG);
            }

            mStructNode = new RwTextureNativeStructNode(this);
            mName = new RwStringNode(name, this);
            mMaskName = new RwStringNode(string.Empty, this);
            mRasterStructNode = new RwRasterStructNode(width, height, palette, indices, pixelFormat, this);
            mExtensionNode = new RwExtensionNode(this, new RwSkyMipMapValueNode());
        }

        public RwTextureNativeNode(Stream stream, bool leaveOpen = false)
            : base(RwNodeId.RwTextureNativeNode)
        {
            var node = (RwTextureNativeNode) Load(stream, leaveOpen);

            mStructNode = node.mStructNode;
            mStructNode.Parent = this;

            mName = node.mName;
            mName.Parent = this;

            mMaskName = node.mMaskName;
            mMaskName.Parent = this;

            mRasterStructNode = node.mRasterStructNode;
            mRasterStructNode.Parent = this;

            mExtensionNode = node.mExtensionNode;
            mExtensionNode.Parent = this;
        }

        /// <summary>
        /// Constructor only to be called in <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RwTextureNativeNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            mStructNode = RwNodeFactory.GetNode<RwTextureNativeStructNode>(this, reader);
            mName = RwNodeFactory.GetNode<RwStringNode>(this, reader);
            mMaskName = RwNodeFactory.GetNode<RwStringNode>(this, reader);
            mRasterStructNode = RwNodeFactory.GetNode<RwRasterStructNode>(this, reader);
            mExtensionNode = RwNodeFactory.GetNode<RwExtensionNode>(this, reader);
        }

        /// <summary>
        /// <para>Construct a bitmap from the data in this instance.</para>
        /// <para>Subsequent calls to this method will return the same bitmap instance without constructing a new one.</para>
        /// </summary>
        public Bitmap GetBitmap()
        {
            if (mBitmap != null)
                return mBitmap;

            if (IsIndexed)
            {
                mBitmap = BitmapHelper.Create(Palette, PixelIndices, Width, Height);
            }
            else
            {
                mBitmap = BitmapHelper.Create(Pixels, Width, Height);
            }

            return mBitmap;
        }

        public Color[] GetPixels()
        {
            if (IsIndexed && mRasterStructNode.DataStructNode.Pixels == null)
            {
                mRasterStructNode.DataStructNode.Pixels = new Color[Width * Height];
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        mRasterStructNode.DataStructNode.Pixels[x + y * Width] = Palette[PixelIndices[x + y * Width]];
            }

            return mRasterStructNode.DataStructNode.Pixels;
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            mStructNode.Write(writer);
            mName.Write(writer);
            mMaskName.Write(writer);
            mRasterStructNode.Write(writer);
            mExtensionNode.Write(writer);
        }
    }
}