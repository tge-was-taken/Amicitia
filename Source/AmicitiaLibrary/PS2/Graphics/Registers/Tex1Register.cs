using System;
using AmicitiaLibrary.Utilities;

namespace AmicitiaLibrary.PS2.Graphics.Registers
{
    using AmicitiaLibrary.Utilities;
    using System.IO;

    public enum Tex1LodCalculationMethod
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

    public enum Tex1BaseAddressSpecification
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
        private ulong mRawData;

        #region Properties

        /// <summary>
        /// LOD Calculation Method
        /// </summary>
        public Tex1LodCalculationMethod LodCalculationMethod
        {
            get { return (Tex1LodCalculationMethod)BitHelper.GetBits(mRawData, 1, 0); }
            set { BitHelper.ClearAndSetBits(ref mRawData, 1, (ulong)value, 0); }
        }

        /// <summary>
        /// Maximum MIP Level (0-6)
        /// </summary>
        public ulong MaxMipLevel
        {
            get { return BitHelper.GetBits(mRawData, 3, 2); }
            set { BitHelper.ClearAndSetBits(ref mRawData, 3, value, 2); }
        }

        /// <summary>
        /// Filter when Texture is Expanded (LOD &lt; 0)
        /// </summary>
        public PS2FilterMode MipMaxFilter
        {
            get { return (PS2FilterMode)BitHelper.GetBits(mRawData, 1, 5); }
            set { BitHelper.ClearAndSetBits(ref mRawData, 1, (ulong)value, 5); }
        }

        /// <summary>
        /// Filter when Texture is Reduced (LOD >= 0)
        /// </summary>
        public PS2FilterMode MipMinFilter
        {
            get { return (PS2FilterMode)BitHelper.GetBits(mRawData, 3, 6); }
            set { BitHelper.ClearAndSetBits(ref mRawData, 3, (ulong)value, 6); }
        }

        /// <summary>
        /// Base Address Specification of MIPMAP Texture of Level 1 or More
        /// </summary>
        public Tex1BaseAddressSpecification MipTexBaseAddressSpecification
        {
            get { return (Tex1BaseAddressSpecification)BitHelper.GetBits(mRawData, 1, 9); }
            set { BitHelper.ClearAndSetBits(ref mRawData, 1, (ulong)value, 9); }
        }

        /// <summary>
        /// LOD Parameter Value L
        /// </summary>
        public ulong MipL
        {
            get { return BitHelper.GetBits(mRawData, 2, 19); }
            set { BitHelper.ClearAndSetBits(ref mRawData, 2, value, 19); }
        }

        /// <summary>
        /// LOD Parameter Value K
        /// </summary>
        public ulong MipK
        {
            get { return BitHelper.GetBits(mRawData, 12, 32); }
            set { BitHelper.ClearAndSetBits(ref mRawData, 12, value, 32); }
        }

        #endregion Properties

        public Tex1Register()
        {
            LodCalculationMethod = Tex1LodCalculationMethod.Formula;
            MaxMipLevel = 0;
            MipMaxFilter = PS2FilterMode.None;
            MipMinFilter = PS2FilterMode.Nearest;
            MipTexBaseAddressSpecification = Tex1BaseAddressSpecification.UseSpecified;
            MipL = 0;
            MipK = 0;
        }

        internal Tex1Register(BinaryReader reader)
        {
            mRawData = reader.ReadUInt64();
        }

        internal byte[] GetBytes()
        {
            return BitConverter.GetBytes(mRawData);
        }
    }
}
