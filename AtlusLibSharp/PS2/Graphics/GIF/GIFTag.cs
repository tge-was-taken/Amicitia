namespace AtlusLibSharp.PS2.Graphics.GIF
{
    using System;
    using System.IO;
    using Common.Utilities;

    public enum GIFTerminationInfo
    {
        FollowingPrimitive = 0,
        NoFollowingPrimitive = 1
    }

    public enum GIFPrimitiveDataUsage
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

    public enum GIFMode
    {
        Packed = 0,
        Reglist = 1,
        Image = 2,
        Disable = 3
    }

    /// <summary>
    /// The GIFTag has a 128-bit fixed length, and specifies the size and structure of the subsequent data and the data format (mode).
    /// </summary>
    public class GIFTag
    {
        private ulong _gifTag0;
        private ulong _gifTag1;

        /// <summary>
        /// Original name: NLOOP
        /// <para>Repeat count (GS primitive data size)</para>
        /// <para>PACKED mode   NREG x NLOOP(qword)</para>
        /// <para>REGLIST mode  NREG x NLOOP(dword)</para>
        /// <para>IMAGE mode    NLOOP(qword)</para>
        /// </summary>
        public ulong RepeatCount
        {
            get { return BitHelper.GetBits(_gifTag0, 15, 0); }
            set { BitHelper.ClearAndSetBits(ref _gifTag0, 15, value, 0); }
        }

        /// <summary>
        /// Original name: EOP
        /// <para>Termination Information (End Of Packet)</para>
        /// </summary>
        public GIFTerminationInfo TerminationInfo
        {
            get { return (GIFTerminationInfo)BitHelper.GetBits(_gifTag0, 1, 15); }
            set { BitHelper.ClearAndSetBits(ref _gifTag0, 1, (ulong)value, 15); }
        }

        /// <summary>
        /// Original name: PRE
        /// <para>PRIM field enable</para>
        /// </summary>
        public GIFPrimitiveDataUsage PrimitiveDataUsage
        {
            get { return (GIFPrimitiveDataUsage)BitHelper.GetBits(_gifTag0, 1, 46); }
            set { BitHelper.ClearAndSetBits(ref _gifTag0, 1, (ulong)value, 46); }
        }

        /// <summary>
        /// Original name: PRIM
        /// <para>Data to be set to the PRIM register of GS</para>
        /// </summary>
        public ulong PrimitiveData
        {
            get { return BitHelper.GetBits(_gifTag0, 11, 47); }
            set { BitHelper.ClearAndSetBits(ref _gifTag0, 11, value, 47); }
        }

        /// <summary>
        /// Original name: FLG
        /// </summary>
        public GIFMode Mode
        {
            get { return (GIFMode)BitHelper.GetBits(_gifTag0, 2, 58); }
            set { BitHelper.ClearAndSetBits(ref _gifTag0, 2, (ulong)value, 58); }
        }

        /// <summary>
        /// Original name: NREG
        /// <para>Number of tegister descriptors in REGS field</para>
        /// <para>When the value is 0, the number of descriptors is 16</para>
        /// </summary>
        public ulong RegisterDescriptorCount
        {
            get { return BitHelper.GetBits(_gifTag0, 4, 60); }
            set { BitHelper.ClearAndSetBits(ref _gifTag0, 4, value, 60); }
        }

        internal GIFTag(BinaryReader reader)
        {
            _gifTag0 = reader.ReadUInt64();
            _gifTag1 = reader.ReadUInt64();
        }

        internal byte[] GetBytes()
        {
            byte[] data;
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                writer.Write(_gifTag0);
                writer.Write(_gifTag1);
                data = (writer.BaseStream as MemoryStream).ToArray();
            }
            return data;
        }
    }
}
