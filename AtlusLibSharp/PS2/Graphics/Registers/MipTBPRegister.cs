namespace AtlusLibSharp.PS2.Graphics.Registers
{
    using System;
    using System.IO;
    using Common.Utilities;

    public class MipTBPRegister
    {
        private ulong _rawData;

        #region Properties

        public ulong Mip1BasePointer
        {
            get { return BitHelper.GetBits(_rawData, 14, 0); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 14, value, 0); }
        }

        public ulong Mip1BufferWidth
        {
            get { return BitHelper.GetBits(_rawData, 6, 14); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 6, value, 14); }
        }

        public ulong Mip2BasePointer
        {
            get { return BitHelper.GetBits(_rawData, 14, 20); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 14, value, 20); }
        }

        public ulong Mip2BufferWidth
        {
            get { return BitHelper.GetBits(_rawData, 6, 34); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 6, value, 34); }
        }

        public ulong Mip3BasePointer
        {
            get { return BitHelper.GetBits(_rawData, 14, 40); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 14, value, 40); }
        }

        public ulong Mip3BufferWidth
        {
            get { return BitHelper.GetBits(_rawData, 6, 54); }
            set { BitHelper.ClearAndSetBits(ref _rawData, 6, value, 54); }
        }

        #endregion Properties

        public MipTBPRegister()
        {
            Mip1BasePointer = 0;
            Mip1BufferWidth = 1;
            Mip2BasePointer = 0;
            Mip2BufferWidth = 1;
            Mip3BasePointer = 0;
            Mip3BufferWidth = 1;
        }

        public MipTBPRegister(
            ulong mip1BasePointer, ulong mip1BufferWidth,
            ulong mip2BasePointer, ulong mip2BufferWidth,
            ulong mip3BasePointer, ulong mip3BufferWidth)
        {
            Mip1BasePointer = mip1BasePointer;
            Mip1BufferWidth = mip1BufferWidth;
            Mip2BasePointer = mip2BasePointer;
            Mip2BufferWidth = mip2BufferWidth;
            Mip3BasePointer = mip3BasePointer;
            Mip3BufferWidth = mip3BufferWidth;
        }

        internal MipTBPRegister(BinaryReader reader)
        {
            _rawData = reader.ReadUInt64();
        }

        internal byte[] GetBytes()
        {
            return BitConverter.GetBytes(_rawData);
        }
    }
}
