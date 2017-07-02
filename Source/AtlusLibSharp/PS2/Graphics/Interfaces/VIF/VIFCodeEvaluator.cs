namespace AtlusLibSharp.PS2.Graphics.Interfaces.VIF
{
    using System.Collections.Generic;
    using System.IO;

    public static class VifCodeEvaluator
    {
        // Methods
        public static List<VifPacket> EvaluateBlock(BinaryReader reader, ushort blockSize)
        {
            List<VifPacket> results = new List<VifPacket>();
            long blockEnd = reader.BaseStream.Position + (blockSize << 4);
            while (reader.BaseStream.Position < blockEnd)
            {
                VifTag vt = ReadVifTagHeader(reader);

                // Check if it's an unpack first
                if ((vt.Command & 0xF0) == 0x60 || (vt.Command & 0xF0) == 0x70)
                {
                    results.Add(new VifUnpack(vt, reader));
                }
                else
                {
                    results.Add(new VifPacket(vt));
                }   
            }

            return results;
        }

        private static VifTag ReadVifTagHeader(BinaryReader reader)
        {
            return new VifTag
            {
                Immediate = reader.ReadUInt16(),
                Count = reader.ReadByte(),
                Command = reader.ReadByte()
            };
        }
    }
}
