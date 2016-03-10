using System.IO;

namespace AtlusLibSharp.PS2.ELF
{
    internal struct ELFProgramHeader
    {
        public uint p_type;
        public ulong p_offset;
        public ulong p_vaddr;
        public ulong p_paddr;
        public uint p_filesz;
        public uint p_memsz;
        public uint p_flags;
        public uint p_align;

        internal ELFProgramHeader(BinaryReader reader, ELFSizeFormat sizeFormat)
        {
            p_type = reader.ReadUInt32();

            if (sizeFormat == ELFSizeFormat.ELF32)
            {
                p_offset = reader.ReadUInt32();
                p_vaddr = reader.ReadUInt32();
                p_paddr = reader.ReadUInt32();
            }
            else if (sizeFormat == ELFSizeFormat.ELF64)
            {
                p_offset = reader.ReadUInt64();
                p_vaddr = reader.ReadUInt64();
                p_paddr = reader.ReadUInt64();
            }
            else
            {
                throw new InvalidDataException("Unknown ELF size format value");
            }

            p_filesz = reader.ReadUInt32();
            p_memsz = reader.ReadUInt32();
            p_flags = reader.ReadUInt32();
            p_align = reader.ReadUInt32();
        }
    }
}
