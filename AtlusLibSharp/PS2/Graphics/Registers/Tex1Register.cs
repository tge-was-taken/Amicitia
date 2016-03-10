using System;

namespace AtlusLibSharp.PS2.Graphics.Registers
{
    using AtlusLibSharp.Utilities;
    using System.IO;

    public enum TEX1LODCalculationMethod
    {
        /// <summary>
        /// Due to formula (LOD = (log2(1 / |Q|) &lt;&lt;L)+K)
        /// </summary>
        Formula = 0,

        /// <summary>
        /// Fixed Value (LOD = K)
        /// </summary>
        FixedValue = 1
    }

    public enum TEX1BaseAddressSpecification
    {
        /// <summary>
        /// Value specified by MIPTBP1 and MIPTBP2 is used.
        /// </summary>
        UseSpecified = 0,

        /// <summary>
        /// Base address of TBP1 - TBP3 is automatically set.
        /// </summary>
        Auto = 1
    }

    /// <summary>
    /// These registers set information on the sampling method of the textures. TEX1_1 sets Context 1 and TEX1_2 sets Context 2.
    /// </summary>
    public class Tex1Register
    {
        private ulong _rawData;

        #region Properties

        /// <summary>
        /// LOD Calculation Method
        /// </summary>
        public TEX1LODCalculationMethod LodCalculationMethod
        {
            get { return (TEX1LODCalculationMethod)BitHelper.GetBits(_rawData, 1, 0); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 1, (ulong)value, 0); }
        }

        /// <summary>
        /// Maximum MIP Level (0-6)
        /// </summary>
        public ulong MaxMipLevel
        {
            get { return BitHelper.GetBits(_rawData, 3, 2); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 3, value, 2); }
        }

        /// <summary>
        /// Filter when Texture is Expanded (LOD &lt; 0)
        /// </summary>
        public PS2FilterMode MipMaxFilter
        {
            get { return (PS2FilterMode)BitHelper.GetBits(_rawData, 1, 5); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 1, (ulong)value, 5); }
        }

        /// <summary>
        /// Filter when Texture is Reduced (LOD >= 0)
        /// </summary>
        public PS2FilterMode MipMinFilter
        {
            get { return (PS2FilterMode)BitHelper.GetBits(_rawData, 3, 6); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 3, (ulong)value, 6); }
        }

        /// <summary>
        /// Base Address Specification of MIPMAP Texture of Level 1 or More
        /// </summary>
        public TEX1BaseAddressSpecification MipTexBaseAddressSpecification
        {
            get { return (TEX1BaseAddressSpecification)BitHelper.GetBits(_rawData, 1, 9); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 1, (ulong)value, 9); }
        }

        /// <summary>
        /// LOD Parameter Value L
        /// </summary>
        public ulong MipL
        {
            get { return BitHelper.GetBits(_rawData, 2, 19); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 2, value, 19); }
        }

        /// <summary>
        /// LOD Parameter Value K
        /// </summary>
        public ulong MipK
        {
            get { return BitHelper.GetBits(_rawData, 12, 32); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 12, value, 32); }
        }

        #endregion Properties

        public Tex1Register()
        {
            LodCalculationMethod = TEX1LODCalculationMethod.Formula;
            MaxMipLevel = 0;
            MipMaxFilter = PS2FilterMode.None;
            MipMinFilter = PS2FilterMode.Nearest;
            MipTexBaseAddressSpecification = TEX1BaseAddressSpecification.UseSpecified;
            MipL = 0;
            MipK = 0;
        }

        internal Tex1Register(BinaryReader reader)
        {
            _rawData = reader.ReadUInt64();
        }

        internal byte[] GetBytes()
        {
            return BitConverter.GetBytes(_rawData);
        }
    }
}
