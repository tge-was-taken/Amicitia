namespace AtlusLibSharp.PS2.Graphics.Interfaces.VIF
{
    using System.Collections.Generic;
    using System.IO;

    public static class VIFCodeEvaluator
    {
        // Methods
        public static List<VIFPacket> EvaluateBlock(BinaryReader reader, ushort blockSize)
        {
            List<VIFPacket> results = new List<VIFPacket>();
            long blockEnd = reader.BaseStream.Position + (blockSize << 4);
            while (reader.BaseStream.Position < blockEnd)
            {
                VIFTag vt = ReadVifTagHeader(reader);

                // Check if it's an unpack first
                if ((vt.Command & 0xF0) == 0x60 || (vt.Command & 0xF0) == 0x70)
                {
                    results.Add(new VIFUnpack(vt, reader));
                }
                else
                {
                    results.Add(new VIFPacket(vt));
                }   
            }

            return results;
        }

        private static VIFTag ReadVifTagHeader(BinaryReader reader)
        {
            return new VIFTag
            {
                Immediate = reader.ReadUInt16(),
                Count = reader.ReadByte(),
                Command = reader.ReadByte()
            };
        }
    }
}
