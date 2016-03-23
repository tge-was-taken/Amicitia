using System;
using System.IO;

namespace AtlusLibSharp.PS2.Graphics.Registers
{
    using AtlusLibSharp.Utilities;

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

        public PS2PixelFormat TexturePixelFormat
        {
            get { return (PS2PixelFormat)BitHelper.GetBits(_rawData, 6, 20); }
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

        public PS2ColorComponent TextureColorComponent
        {
            get { return (PS2ColorComponent)BitHelper.GetBits(_rawData, 1, 34); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 1, (ulong)value, 34); }
        }

        public PS2TextureFunction TextureFunction
        {
            get { return (PS2TextureFunction)BitHelper.GetBits(_rawData, 2, 35); }
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

        public PS2PixelFormat CLUTPixelFormat
        {
            get { return (PS2PixelFormat)BitHelper.GetBits(_rawData, 4, 51); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 4, (ulong)value, 51); }
        }

        public PS2CLUTStorageMode CLUTStorageMode
        {
            get { return (PS2CLUTStorageMode)BitHelper.GetBits(_rawData, 1, 55); }
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
            TextureColorComponent = PS2ColorComponent.RGBA;
            TextureFunction = PS2TextureFunction.Modulate;
            CLUTPixelFormat = PS2PixelFormat.PSMCT32;
            CLUTStorageMode = PS2CLUTStorageMode.CSM1;
            CLUTEntryOffset = 0;
            CLUTBufferLoadControl = TEX0CLUTBufferLoadControl.Load;
        }

        public Tex0Register(
            PS2PixelFormat txFmt, 
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

        internal static int GetBufferWidth(int width, PS2PixelFormat pixelFormat)
        {
            int divisor;

            switch (pixelFormat)
            {
                case PS2PixelFormat.PSMCT32:
                case PS2PixelFormat.PSMCT24:
                    divisor = 256; // this might be wrong
                    break;
                case PS2PixelFormat.PSMT8:
                case PS2PixelFormat.PSMT8H:
                    divisor = 64;
                    break;
                case PS2PixelFormat.PSMT4:
                case PS2PixelFormat.PSMT4HL:
                case PS2PixelFormat.PSMT4HH:
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
