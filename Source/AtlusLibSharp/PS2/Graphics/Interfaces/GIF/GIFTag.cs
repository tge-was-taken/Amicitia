namespace AtlusLibSharp.PS2.Graphics.Interfaces.GIF
{
    using System;
    using System.IO;
    using AtlusLibSharp.Utilities;

    public enum GifTerminationInfo
    {
        FollowingPrimitive = 0,
        NoFollowingPrimitive = 1
    }

    public enum GifPrimitiveDataUsage
    {
        /// <summary>
        /// Ignores PRIM field.
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// Outputs PRIM field value to PRIM register.
        /// </summary>
        OutputToRegister = 1
    }

    public enum GifMode
    {
        Packed = 0,
        Reglist = 1,
        Image = 2,
        Disable = 3
    }

    /// <summary>
    /// The GIFTag has a 128-bit fixed length, and specifies the size and structure of the subsequent data and the data format (mode).
    /// </summary>
    public class GifTag
    {
        private ulong mGifTag0;
        private ulong mGifTag1;

        /// <summary>
        /// Original name: NLOOP
        /// <para>Repeat count (GS primitive data size)</para>
        /// <para>PACKED mode   NREG x NLOOP(qword)</para>
        /// <para>REGLIST mode  NREG x NLOOP(dword)</para>
        /// <para>IMAGE mode    NLOOP(qword)</para>
        /// </summary>
        public ulong RepeatCount
        {
            get { return BitHelper.GetBits(mGifTag0, 15, 0); }
            set { BitHelper.ClearAndSetBits(ref mGifTag0, 15, value, 0); }
        }

        /// <summary>
        /// Original name: EOP
        /// <para>Termination Information (End Of Packet)</para>
        /// </summary>
        public GifTerminationInfo TerminationInfo
        {
            get { return (GifTerminationInfo)BitHelper.GetBits(mGifTag0, 1, 15); }
            set { BitHelper.ClearAndSetBits(ref mGifTag0, 1, (ulong)value, 15); }
        }

        /// <summary>
        /// Original name: PRE
        /// <para>PRIM field enable</para>
        /// </summary>
        public GifPrimitiveDataUsage PrimitiveDataUsage
        {
            get { return (GifPrimitiveDataUsage)BitHelper.GetBits(mGifTag0, 1, 46); }
            set { BitHelper.ClearAndSetBits(ref mGifTag0, 1, (ulong)value, 46); }
        }

        /// <summary>
        /// Original name: PRIM
        /// <para>DataStructNode to be set to the PRIM register of GS</para>
        /// </summary>
        public ulong PrimitiveData
        {
            get { return BitHelper.GetBits(mGifTag0, 11, 47); }
            set { BitHelper.ClearAndSetBits(ref mGifTag0, 11, value, 47); }
        }

        /// <summary>
        /// Original name: FLG
        /// </summary>
        public GifMode Mode
        {
            get { return (GifMode)BitHelper.GetBits(mGifTag0, 2, 58); }
            set { BitHelper.ClearAndSetBits(ref mGifTag0, 2, (ulong)value, 58); }
        }

        /// <summary>
        /// Original name: NREG
        /// <para>Number of tegister descriptors in REGS field</para>
        /// <para>When the value is 0, the number of descriptors is 16</para>
        /// </summary>
        public ulong RegisterDescriptorCount
        {
            get { return BitHelper.GetBits(mGifTag0, 4, 60); }
            set { BitHelper.ClearAndSetBits(ref mGifTag0, 4, value, 60); }
        }

        internal GifTag(BinaryReader reader)
        {
            mGifTag0 = reader.ReadUInt64();
            mGifTag1 = reader.ReadUInt64();
        }

        internal byte[] GetBytes()
        {
            byte[] data;
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                writer.Write(mGifTag0);
                writer.Write(mGifTag1);
                data = (writer.BaseStream as MemoryStream).ToArray();
            }
            return data;
        }
    }
}
