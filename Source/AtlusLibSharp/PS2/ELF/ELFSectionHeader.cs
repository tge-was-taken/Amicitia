using System.IO;

namespace AtlusLibSharp.PS2.ELF
{
    internal struct ELFSectionHeader
    {
        public uint sh_name;
        public uint sh_type;
        public ulong sh_flags;
        public ulong sh_addr;
        public ulong sh_offset;
        public ulong sh_size;
        public uint sh_link;
        public uint sh_info;
        public ulong sh_addralign;
        public ulong sh_entsize;

        internal ELFSectionHeader(BinaryReader reader, ELFSizeFormat sizeFormat)
        {
            sh_name = reader.ReadUInt32();
            sh_type = reader.ReadUInt32();

            if (sizeFormat == ELFSizeFormat.ELF32)
            {
                sh_flags = reader.ReadUInt32();
                sh_addr = reader.ReadUInt32();
                sh_offset = reader.ReadUInt32();
                sh_size = reader.ReadUInt32();
                sh_link = reader.ReadUInt32();
                sh_info = reader.ReadUInt32();
                sh_addralign = reader.ReadUInt32();
                sh_entsize = reader.ReadUInt32();
            }
            else if (sizeFormat == ELFSizeFormat.ELF64)
            {
                sh_flags = reader.ReadUInt64();
                sh_addr = reader.ReadUInt64();
                sh_offset = reader.ReadUInt64();
                sh_size = reader.ReadUInt64();
                sh_link = reader.ReadUInt32();
                sh_info = reader.ReadUInt32();
                sh_addralign = reader.ReadUInt64();
                sh_entsize = reader.ReadUInt64();
            }
            else
            {
                throw new InvalidDataException("Unknown ELF size format value");
            }
        }
    }
}
