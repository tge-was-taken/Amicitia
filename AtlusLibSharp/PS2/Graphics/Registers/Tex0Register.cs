using System;
using System.IO;

namespace AtlusLibSharp.PS2.Graphics.Registers
{
    using Common.Utilities;

    public enum TEX0CLUTBufferLoadControl
    {
        /// <summary>
        /// Temporary buffer contents are not changed.
        /// </summary>
        TempBufferNotChanged = 0,

        /// <summary>
        /// Load is performed to CSA position of buffer.
        /// </summary>
        Load = 1,

        /// <summary>
        /// Load is performed to CSA position of buffer and CBP is copied to CBP0.
        /// </summary>
        LoadCopyCBP0 = 2,

        /// <summary>
        /// Load is performed to CSA position of buffer and CBP is copied to CBP1.
        /// </summary>
        LoadCopyCBP1 = 3,

        /// <summary>
        /// If CBP0 != CBP, load is performed and CBP is copied to CBP0.
        /// </summary>
        LoadCopyCBP0NEQ = 4,

        /// <summary>
        /// If CBP1 != CBP, load is performed and CBP is copied to CBP1.
        /// </summary>
        LoadCopyCBP1NEQ = 5
    }

    /// <summary>
    /// These registers set various kinds of information regarding the textures to be used. TEX0_1 sets Context 1 and TEX0_2 sets Context 2
    /// </summary>
    public class Tex0Register
    {
        private ulong _rawData;

        #region Properties

        /// <summary>
        /// Word Address / 64
        /// </summary>
        public ulong TextureBasePointer
        {
            get { return BitHelper.GetBits(_rawData, 14, 0); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 14, value, 0); }
        }

        /// <summary>
        /// Width in Units of texels / 64
        /// </summary>
        public ulong TextureBufferWidth
        {
            get { return BitHelper.GetBits(_rawData, 6, 14); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 6, value, 14); }
        }

        public PixelFormat TexturePixelFormat
        {
            get { return (PixelFormat)BitHelper.GetBits(_rawData, 6, 20); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 6, (ulong)value, 20); }
        }

        /// <summary>
        /// Width = 2^TW (max 2^10)
        /// </summary>
        public ulong TextureWidth
        {
            get { return (ulong)Math.Pow(2, BitHelper.GetBits(_rawData, 4, 26)); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 4, (ulong)Math.Log(value, 2), 26); }
        }

        /// <summary>
        /// Height = 2^TW (max 2^10)
        /// </summary>
        public ulong TextureHeight
        {
            get { return (ulong)Math.Pow(2, BitHelper.GetBits(_rawData, 4, 30)); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 4, (ulong)Math.Log(value, 2), 30); }
        }

        public ColorComponent TextureColorComponent
        {
            get { return (ColorComponent)BitHelper.GetBits(_rawData, 1, 34); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 1, (ulong)value, 34); }
        }

        public TextureFunction TextureFunction
        {
            get { return (TextureFunction)BitHelper.GetBits(_rawData, 2, 35); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 2, (ulong)value, 35); }
        }

        /// <summary>
        /// Word Address / 64
        /// </summary>
        public ulong CLUTBufferBasePointer
        {
            get { return BitHelper.GetBits(_rawData, 14, 37); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 14, value, 37); }
        } 

        public PixelFormat CLUTPixelFormat
        {
            get { return (PixelFormat)BitHelper.GetBits(_rawData, 4, 51); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 4, (ulong)value, 51); }
        }

        public CLUTStorageMode CLUTStorageMode
        {
            get { return (CLUTStorageMode)BitHelper.GetBits(_rawData, 1, 55); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 1, (ulong)value, 55); }
        }

        /// <summary>
        /// Offset / 16. In CSM2 mode, this value must be 0
        /// </summary>
        public ulong CLUTEntryOffset 
        {
            get { return BitHelper.GetBits(_rawData, 5, 56); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 5, value, 56); }
        }

        public TEX0CLUTBufferLoadControl CLUTBufferLoadControl
        {
            get { return (TEX0CLUTBufferLoadControl)BitHelper.GetBits(_rawData, 3, 61); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 3, (ulong)value, 61); }
        }

        #endregion Properties

        public Tex0Register()
        {
            TextureColorComponent = ColorComponent.RGBA;
            TextureFunction = TextureFunction.Modulate;
            CLUTPixelFormat = PixelFormat.PSMCT32;
            CLUTStorageMode = CLUTStorageMode.CSM1;
            CLUTEntryOffset = 0;
            CLUTBufferLoadControl = TEX0CLUTBufferLoadControl.Load;
        }

        public Tex0Register(
            PixelFormat txFmt, 
            int txWidth, int txHeight)
            : this()
        {
            TextureBufferWidth = (ulong)GetBufferWidth(txWidth, txFmt);
            TexturePixelFormat = txFmt;
            TextureWidth = (ulong)txWidth;
            TextureHeight = (ulong)txHeight;
        }

        internal Tex0Register(BinaryReader reader)
        {
            _rawData = reader.ReadUInt64();
        }

        internal static int GetBufferWidth(int width, PixelFormat pixelFormat)
        {
            int divisor;

            switch (pixelFormat)
            {
                case PixelFormat.PSMT8:
                case PixelFormat.PSMT8H:
                    divisor = 64;
                    break;
                case PixelFormat.PSMT4:
                case PixelFormat.PSMT4HL:
                case PixelFormat.PSMT4HH:
                    divisor = 32; // because every pixel only takes up half a texel
                    break;
                default:
                    throw new ArgumentException();
            }

            int bufferWidth = width / divisor;

            if (bufferWidth < 1)
            {
                bufferWidth = 1;
            }
            else if (bufferWidth == 1)
            {
                bufferWidth = 2;
            }
            else
            {
                bufferWidth = (int)Math.Ceiling((double)bufferWidth);
            }

            return bufferWidth;
        }

        internal byte[] GetBytes()
        {
            return BitConverter.GetBytes(_rawData);
        }
    }
}
