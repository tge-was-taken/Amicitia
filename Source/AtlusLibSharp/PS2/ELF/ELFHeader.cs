using AtlusLibSharp.Utilities;
using System.IO;

namespace AtlusLibSharp.PS2.ELF
{
    internal struct ELFHeader
    {
        private const uint ELF_MAGIC = 0x464C457F;
        public byte ei_class;
        public byte ei_data;
        public byte ei_version;
        public byte ei_osabi;
        public byte ei_abiversion;
        public ushort e_type;
        public ushort e_machine;
        public uint e_version;
        public ulong e_entry;
        public ulong e_phoff;
        public ulong e_shoff;
        public uint e_flags;
        public ushort e_ehsize;
        public ushort e_phentsize;
        public ushort e_phnum;
        public ushort e_shentsize;
        public ushort e_shnum;
        public ushort e_shstrndx;

        internal ELFHeader(BinaryReader reader)
        {
            uint elfMagic = reader.ReadUInt32();

            if (elfMagic != ELF_MAGIC)
            {
                throw new InvalidDataException("ELF header signature mismatch.");
            }

            ei_class = reader.ReadByte();
            ei_data = reader.ReadByte();
            ei_version = reader.ReadByte();
            ei_osabi = reader.ReadByte();
            ei_abiversion = reader.ReadByte();

            // seek past padding
            reader.Seek(0x07, SeekOrigin.Current);

            e_type = reader.ReadUInt16();
            e_machine = reader.ReadUInt16();
            e_version = reader.ReadUInt32();

            if (ei_class == (byte)ELFSizeFormat.ELF32)
            {
                e_entry = reader.ReadUInt32();
                e_phoff = reader.ReadUInt32();
                e_shoff = reader.ReadUInt32();
            }
            else if (ei_class == (byte)ELFSizeFormat.ELF64)
            {
                e_entry = reader.ReadUInt64();
                e_phoff = reader.ReadUInt64();
                e_shoff = reader.ReadUInt64();
            }
            else
            {
                throw new InvalidDataException("Unknown ELF size format value");
            }

            e_flags = reader.ReadUInt32();
            e_ehsize = reader.ReadUInt16();
            e_phentsize = reader.ReadUInt16();
            e_phnum = reader.ReadUInt16();
            e_shentsize = reader.ReadUInt16();
            e_shnum = reader.ReadUInt16();
            e_shstrndx = reader.ReadUInt16();
        }
    }

}
