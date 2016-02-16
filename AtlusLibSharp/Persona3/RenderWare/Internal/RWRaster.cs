namespace AtlusLibSharp.Persona3.RenderWare
{
    using System.Drawing;
    using System.IO;

    /// <summary>
    /// Represents a container for internal RenderWare texture info and data.
    /// </summary>
    internal class RWRaster : RWNode
    {
        private RWRasterInfo _rasterInfo;
        private RWRasterData _rasterData;

        /// <summary>
        /// Gets or sets the <see cref="RWRasterInfo"/> containing internal info about the texture.
        /// </summary>
        public RWRasterInfo Info
        {
            get { return _rasterInfo; }
            set
            {
                _rasterInfo = value;
                _rasterInfo.Parent = this;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="RWRasterData"/> containing the texture data.
        /// </summary>
        public RWRasterData Data
        {
            get { return _rasterData; }
            set
            {
                _rasterData = value;
                _rasterData.Parent = this;
            }
        }

        /// <summary>
        /// Initialize a new instance of <see cref="RWRaster"/> using a bitmap to encode using the given pixel format.
        /// </summary>
        /// <param name="bitmap">Bitmap to encode to the specified pixel format.</param>
        /// <param name="pixelFormat">Pixel format to encode the bitmap to.</param>
        /// <param name="parent">Parent of this <see cref="RWRaster"/> node. Value is null if not specified.</param>
        public RWRaster(Bitmap bitmap, PS2.Graphics.PixelFormat pixelFormat, RWNode parent = null)
            : base(RWType.Struct, parent)
        {
            Info = new RWRasterInfo(bitmap.Width, bitmap.Height, pixelFormat);
            Data = new RWRasterData(bitmap, pixelFormat);
        }

        /// <summary>
        /// Initialize a new instance of <see cref="RWRaster"/> using a width, height, palette, pixel indices and pixel format.
        /// </summary>
        /// <param name="width">Width of the texture.</param>
        /// <param name="height">Height of the texture.</param>
        /// <param name="palette">Palette colors of the texture.</param>
        /// <param name="indices">Per-pixel palette color indices of the texture.</param>
        /// <param name="pixelFormat">PS2 pixel format of the given data.</param>
        /// <param name="parent">Parent of this <see cref="RWRaster"/> node. Value is null if not specified.</param>
        public RWRaster(int width, int height, Color[] palette, byte[] indices, 
            PS2.Graphics.PixelFormat pixelFormat, RWNode parent = null)
            : base(RWType.Struct, parent)
        {
            Info = new RWRasterInfo(width, height, pixelFormat);
            Data = new RWRasterData(palette, indices, pixelFormat);
        }

        /// <summary>
        /// Initializer only to be called by <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RWRaster(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            _rasterInfo = RWNodeFactory.GetNode<RWRasterInfo>(this, reader);
            _rasterData = RWNodeFactory.GetNode<RWRasterData>(this, reader);
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            _rasterInfo.InternalWrite(writer);
            _rasterData.InternalWrite(writer);
        }
    }
}