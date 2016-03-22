namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Drawing;
    using System.IO;
    using PS2.Graphics;
    using AtlusLibSharp.Utilities;
    using System.Collections.Generic;
    using IO;
    using System;

    /// <summary>
    /// Encapsulates a RenderWare texture node and all of its corresponding data structures.
    /// </summary>
    public class RWTextureNative : RWNode, ITextureFile
    {
        private const string EXCEPTION_NOT_POW2 = "The image dimensions have to be a power of 2.";
        private const string EXCEPTION_DIMENSION_TOO_BIG = "The image can not be larger than 1024 pixels in width or height.";

        private RWTextureNativeStruct _struct;
        private RWString _name;
        private RWString _maskName;
        private RWRaster _raster;
        private RWExtension _extension;
        private Bitmap _bitmap;

        #region Properties

        /// <summary>
        /// Property exposed for use in <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RWTextureNativeStruct Struct
        {
            get { return _struct; }
        }

        /// <summary>
        /// Property exposed for use in <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RWRaster Raster
        {
            get { return _raster; }
        }

        /// <summary>
        /// Gets if the texture node uses an indexed format with indices and a palette.
        /// </summary>
        public bool IsIndexed
        {
            get { return PS2PixelFormatHelper.IsIndexedPixelFormat(Tex0Register.TexturePixelFormat); }
        }

        /**********************************************/
        /* RWTextureNativeStruct forwarded properties */
        /**********************************************/

        /// <summary>
        /// Gets the <see cref="RWPlatformID"/> of this texture node, indicating the platform for which the data has been compiled.
        /// </summary>
        public RWPlatformID PlatformID
        {
            get { return _struct.PlatformID; }
        }

        /// <summary>
        /// Gets and sets the <see cref="PS2.Graphics.PS2FilterMode"/> of this texture node.
        /// </summary>
        public PS2FilterMode FilterMode
        {
            get { return _struct.FilterMode; }
            set { _struct.FilterMode = value; }
        }

        /// <summary>
        /// Gets and sets the horizontal <see cref="PS2AddressingMode"/> of this texture node used to control texture wrapping.
        /// </summary>
        public PS2AddressingMode HorrizontalAddressingMode
        {
            get { return _struct.HorizontalAddressingMode; }
            set { _struct.HorizontalAddressingMode = value; }
        }

        /// <summary>
        /// Gets and sets the vertical <see cref="PS2AddressingMode"/> of this texture node used to control texture wrapping.
        /// </summary>
        public PS2AddressingMode VerticalAddressingMode
        {
            get { return _struct.VerticalAddressingMode; }
            set { _struct.VerticalAddressingMode = value; }
        }

        /*********************************/
        /* RWString forwarded properties */
        /*********************************/

        /// <summary>
        /// Gets and sets the name of this texture node.
        /// </summary>
        public string Name
        {
            get { return _name.Value; }
            set { _name = new RWString(value, this); }
        }

        /// <summary>
        /// (Unused) Gets and sets the name of the alpha mask applied to this texture node.
        /// </summary>
        public string MaskName
        {
            get { return _maskName.Value; }
            set { _maskName = new RWString(value, this); }
        }

        /*************************************/
        /* RWRasterInfo forwarded properties */
        /*************************************/

        /// <summary>
        /// Gets the texture width of this texture node.
        /// </summary>
        public int Width
        {
            get { return _raster.Info.Width; }
        }

        /// <summary>
        /// Gets the texture height of this texture node.
        /// </summary>
        public int Height
        {
            get { return _raster.Info.Height; }
        }
        
        /// <summary>
        /// Gets the texture depth of this texture node.
        /// </summary>
        public int Depth
        {
            get { return _raster.Info.Depth; }
        }

        /// <summary>
        /// Gets the <see cref="RWRasterFormats"/> of this texture node.
        /// </summary>
        public RWRasterFormats Format
        {
            get { return _raster.Info.Format; }
        }

        /// <summary>
        /// Gets the PS2 <see cref="PS2.Graphics.Registers.Tex0Register"/> settings of this texture node.
        /// </summary>
        public PS2.Graphics.Registers.Tex0Register Tex0Register
        {
            get { return _raster.Info.Tex0Register; }
        }

        /// <summary>
        /// Gets the PS2 <see cref="PS2.Graphics.Registers.Tex1Register"/> settings of this texture node.
        /// </summary>
        public PS2.Graphics.Registers.Tex1Register Tex1Register
        {
            get { return _raster.Info.Tex1Register; }
        }

        /*************************************/
        /* RWRasterData forwarded properties */
        /*************************************/

        /// <summary>
        /// Gets the per-pixel palette color indices of this texture node. Returns null if the texture node is not indexed.
        /// </summary>
        public byte[] PixelIndices
        {
            get { return _raster.Data.PixelIndices; }
        }

        /// <summary>
        /// Gets the color palette of this texture node. Returns null if the texture node is not indexed.
        /// </summary>
        public Color[] Palette
        {
            get { return _raster.Data.Palette; }
        }

        /// <summary>
        /// Gets the pixels of this texture node. Returns null if the texture node is indexed.
        /// </summary>
        public Color[] Pixels
        {
            get { return _raster.Data.Pixels; }
        }

        /************************/
        /* Extension properties */
        /************************/

        /// <summary>
        /// Gets the list of extension nodes applied to this texture node.
        /// </summary>
        public List<RWNode> Extensions
        {
            get { return _extension.Children; }
        }

        #endregion Properties

        /// <summary>
        /// Initializes an <see cref="RWTextureNative"/> instance using a path to a bitmap and a PS2 pixel format to encode to.
        /// </summary>
        /// <param name="path">Path to a bitmap.</param>
        /// <param name="pixelFormat">PS2 Pixel format to encode the bitmap to.</param>
        public RWTextureNative(string path, PS2PixelFormat pixelFormat) 
            : this(Path.GetFileNameWithoutExtension(path), new Bitmap(path), pixelFormat)
        {

        }

        /// <summary>
        /// Initializes an <see cref="RWTextureNative"/> instance using a bitmap, name and a PS2 pixel format to encode to.
        /// </summary>
        /// <param name="name">Name of the texture used for material references.</param>
        /// <param name="bitmap">Source bitmap used to encode.</param>
        /// <param name="pixelFormat">PS2 Pixel format to encode the bitmap to.</param>
        public RWTextureNative(string name, Bitmap bitmap, PS2PixelFormat pixelFormat)
            : base(RWNodeType.TextureNative)
        {
            if (bitmap.Width % 2 != 0 || bitmap.Height % 2 != 0)
            {
                throw new ArgumentException(EXCEPTION_NOT_POW2);
            }

            if (bitmap.Width > 1024 || bitmap.Width > 1024)
            {
                throw new ArgumentException(EXCEPTION_DIMENSION_TOO_BIG);
            }

            _struct = new RWTextureNativeStruct(this);
            _name = new RWString(name, this);
            _maskName = new RWString(string.Empty, this);
            _raster = new RWRaster(bitmap, pixelFormat, this);
            _extension = new RWExtension(new RWSkyMipMapValue());
            _extension.Parent = this;
        }

        /// <summary>
        /// Initializes an <see cref="RWTextureNative"/> instance using a name, width, height, palette, indices and a PS2 pixel format to encode to.
        /// </summary>
        /// <param name="name">Name of the texture used for material references.</param>
        /// <param name="width">Width of the texture.</param>
        /// <param name="height">Height of the texture.</param>
        /// <param name="palette">Texture palette data.</param>
        /// <param name="indices">Texture pixel indices into the palette.</param>
        /// <param name="pixelFormat">PS2 Pixel format to encode the bitmap to.</param>
        public RWTextureNative(string name, int width, int height, Color[] palette, byte[] indices, PS2PixelFormat pixelFormat)
            : base(RWNodeType.TextureNative)
        {
            if (width % 2 != 0 || height % 2 != 0)
            {
                throw new ArgumentException(EXCEPTION_NOT_POW2);
            }

            if (width > 1024 || width > 1024)
            {
                throw new ArgumentException(EXCEPTION_DIMENSION_TOO_BIG);
            }

            _struct = new RWTextureNativeStruct(this);
            _name = new RWString(name, this);
            _maskName = new RWString(string.Empty, this);
            _raster = new RWRaster(width, height, palette, indices, pixelFormat, this);
            _extension = new RWExtension(new RWSkyMipMapValue());
            _extension.Parent = this;
        }

        /// <summary>
        /// Constructor only to be called in <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RWTextureNative(RWNodeFactory.RWNodeInfo header, BinaryReader reader)
            : base(header)
        {
            _struct = RWNodeFactory.GetNode<RWTextureNativeStruct>(this, reader);
            _name = RWNodeFactory.GetNode<RWString>(this, reader);
            _maskName = RWNodeFactory.GetNode<RWString>(this, reader);
            _raster = RWNodeFactory.GetNode<RWRaster>(this, reader);
            _extension = RWNodeFactory.GetNode<RWExtension>(this, reader);
        }

        /// <summary>
        /// <para>Construct a bitmap from the data in this instance.</para>
        /// <para>Subsequent calls to this method will return the same bitmap instance without constructing a new one.</para>
        /// </summary>
        public Bitmap GetBitmap()
        {
            if (_bitmap != null)
                return _bitmap;

            if (IsIndexed)
            {
                _bitmap = BitmapHelper.Create(Palette, PixelIndices, Width, Height);
            }
            else
            {
                _bitmap = BitmapHelper.Create(Pixels, Width, Height);
            }

            return _bitmap;
        }

        public Color[] GetPixels()
        {
            if (IsIndexed && _raster.Data.Pixels == null)
            {
                _raster.Data.Pixels = new Color[Width * Height];
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        _raster.Data.Pixels[x + y * Width] = Palette[PixelIndices[x + y * Width]];
            }

            return _raster.Data.Pixels;
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            _struct.InternalWrite(writer);
            _name.InternalWrite(writer);
            _maskName.InternalWrite(writer);
            _raster.InternalWrite(writer);
            _extension.InternalWrite(writer);
        }
    }
}