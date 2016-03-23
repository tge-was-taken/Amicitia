using System.IO;

namespace AtlusLibSharp.PS2.Graphics
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

        private GIFTag _gifTag0;
        private TRXPOSRegister _trxPos;
        private TRXREGRegister _trxReg;
        private TRXDIRRegister _trxDir;
        private GIFTag _gifTag1;

        public PS2StandardImageHeader(
            GIFTag regListTag,
            TRXPOSRegister trxPos, TRXREGRegister TRXRegister, TRXDIRRegister trxDir,
            GIFTag imageDataTag)
        {
            _gifTag0 = regListTag;
            _trxPos = trxPos;
            _trxReg = TRXRegister;
            _trxDir = trxDir;
            _gifTag1 = imageDataTag;
        }

        internal PS2StandardImageHeader(BinaryReader reader)
        {
            InternalRead(reader);
        }

        private void InternalRead(BinaryReader reader)
        {
            _gifTag0 = new GIFTag(reader);
            _trxPos = new TRXPOSRegister(reader);
            ulong trxPosAddress = reader.ReadUInt64();
            _trxReg = new TRXREGRegister(reader);
            ulong trxRegisterAddress = reader.ReadUInt64();
            _trxDir = new TRXDIRRegister(reader);
            ulong trxDirAddress = reader.ReadUInt64();
            _gifTag1 = new GIFTag(reader);
        }

        internal byte[] GetBytes()
        {
            byte[] data;
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                // Reglist GIFTag
                writer.Write(_gifTag0.GetBytes());

                // TRXPos
                writer.Write(_trxPos.GetBytes());
                writer.Write(TRXPOS_ADDRESS);

                // TRXRegister
                writer.Write(_trxReg.GetBytes());
                writer.Write(TRXREG_ADDRESS);

                // TRXDir
                writer.Write(_trxDir.GetBytes());
                writer.Write(TRXDIR_ADDRESS);

                // Image data GIFTag
                writer.Write(_gifTag1.GetBytes());

                data = (writer.BaseStream as MemoryStream).ToArray();
            }
            return data;
        }
    }
}
