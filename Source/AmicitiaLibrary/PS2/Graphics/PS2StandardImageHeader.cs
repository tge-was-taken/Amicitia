using System.IO;

namespace AmicitiaLibrary.PS2.Graphics
{
    using Registers;
    using Interfaces.GIF;

    /// <summary>
    /// sceGsLoadImage struct in the sdk
    /// </summary>
    internal class PS2StandardImageHeader
    {
        public const int Size = 0x50;
        private const ulong TRXPOS_ADDRESS = 0x51;
        private const ulong TRXREG_ADDRESS = 0x52;
        private const ulong TRXDIR_ADDRESS = 0x53;

        private GifTag mGifTag0;
        private TRXPOSRegister mTrxPos;
        private TRXREGRegister mTrxReg;
        private TRXDIRRegister mTrxDir;
        private GifTag mGifTag1;

        public PS2StandardImageHeader(
            GifTag regListTag,
            TRXPOSRegister trxPos, TRXREGRegister TRXRegister, TRXDIRRegister trxDir,
            GifTag imageDataTag)
        {
            mGifTag0 = regListTag;
            mTrxPos = trxPos;
            mTrxReg = TRXRegister;
            mTrxDir = trxDir;
            mGifTag1 = imageDataTag;
        }

        internal PS2StandardImageHeader(BinaryReader reader)
        {
            InternalRead(reader);
        }

        private void InternalRead(BinaryReader reader)
        {
            mGifTag0 = new GifTag(reader);
            mTrxPos = new TRXPOSRegister(reader);
            ulong trxPosAddress = reader.ReadUInt64();
            mTrxReg = new TRXREGRegister(reader);
            ulong trxRegisterAddress = reader.ReadUInt64();
            mTrxDir = new TRXDIRRegister(reader);
            ulong trxDirAddress = reader.ReadUInt64();
            mGifTag1 = new GifTag(reader);
        }

        internal byte[] GetBytes()
        {
            byte[] data;
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                // Reglist GIFTag
                writer.Write(mGifTag0.GetBytes());

                // TRXPos
                writer.Write(mTrxPos.GetBytes());
                writer.Write(TRXPOS_ADDRESS);

                // TRXRegister
                writer.Write(mTrxReg.GetBytes());
                writer.Write(TRXREG_ADDRESS);

                // TRXDir
                writer.Write(mTrxDir.GetBytes());
                writer.Write(TRXDIR_ADDRESS);

                // Image data GIFTag
                writer.Write(mGifTag1.GetBytes());

                data = (writer.BaseStream as MemoryStream).ToArray();
            }
            return data;
        }
    }
}
