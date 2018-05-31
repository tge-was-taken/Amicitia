using System;
using System.IO;
using AmicitiaLibrary.Utilities;

namespace AmicitiaLibrary.PS2.Graphics.Registers
{
    using AmicitiaLibrary.Utilities;

    /// <summary>
    /// <para>This register specifies the size of the rectangular area, where the transmission between buffers is implemented, in units of pixels.</para>
    /// <para>The pixel mode must be the one set by the BITBLTBUF register.</para>
    /// </summary>
    public class TRXREGRegister
    {
        private ulong mRawData;

        /// <summary>
        /// Width of Transmission Area
        /// </summary>
        public ulong TransmissionWidth
        {
            get { return BitHelper.GetBits(mRawData, 12, 0); }
            set { BitHelper.ClearAndSetBits(ref mRawData, 12, value, 0); }
        }

        /// <summary>
        /// Height of Transmission Area
        /// </summary>
        public ulong TransmissionHeight
        {
            get { return BitHelper.GetBits(mRawData, 12, 32); }
            set { BitHelper.ClearAndSetBits(ref mRawData, 12, value, 32); }
        }

        public TRXREGRegister(
            ulong transWidth, ulong transHeight)
        {
            TransmissionWidth = transWidth;
            TransmissionHeight = transHeight;
        }

        internal TRXREGRegister(BinaryReader reader)
        {
            mRawData = reader.ReadUInt64();
        }

        internal byte[] GetBytes()
        {
            return BitConverter.GetBytes(mRawData);
        }
    }
}
