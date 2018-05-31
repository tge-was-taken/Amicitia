namespace AmicitiaLibrary.PS2.Graphics.Registers
{
    using System;
    using System.IO;
    using AmicitiaLibrary.Utilities;

    /// <summary>
    /// <para>This register specifies the transmission direction in the transmission between buffers, and activates transmission.</para>
    /// <para>Appropriate settings must be made by the BITBLTBUF/TRXPOS/TRXREG before activating the transmission.</para>
    /// </summary>
    public class TRXDIRRegister
    {
        private ulong mRawData;

        public PS2TrxdirTransmissionDirection TransmissionDirection
        {
            get { return (PS2TrxdirTransmissionDirection)BitHelper.GetBits(mRawData, 2, 0); }
            set { BitHelper.ClearAndSetBits(ref mRawData, 2, (ulong)value, 0); }
        }

        public TRXDIRRegister(PS2TrxdirTransmissionDirection transDirection)
        {
            TransmissionDirection = transDirection;
        }

        internal TRXDIRRegister(BinaryReader reader)
        {
            mRawData = reader.ReadUInt64();
        }

        internal byte[] GetBytes()
        {
            return BitConverter.GetBytes(mRawData);
        }
    }
}
