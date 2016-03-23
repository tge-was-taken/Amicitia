namespace AtlusLibSharp.PS2.ELF
{
    using System.Collections.Generic;
    using System.IO;
    using AtlusLibSharp.Utilities;

    internal class ELFFile
    { 
        // header
        private ELFHeader _header;

        // backing stores
        private List<ELFSection> _sections;
        
        public ELFFile(string filepath)
            : this()
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filepath)))
            {
                InternalRead(reader);
            }
        }

        private ELFFile()
        {
            _header = new ELFHeader();
            _sections = new List<ELFSection>();
        }

        /// <summary>
        /// Gets the size format of the ELF file.
        /// </summary>
        public ELFSizeFormat SizeFormat
        {
            get { return (ELFSizeFormat)_header.ei_class; }
        }

        /// <summary>
        /// Gets the endianness of the ELF file.
        /// </summary>
        public ELFEndianness Endianness
        {
            get { return (ELFEndianness)_header.ei_data; }
        }

        /// <summary>
        /// Gets the version of the ELF file.
        /// </summary>
        public int Version
        {
            get { return _header.ei_version; }
        }

        /// <summary>
        /// Gets the target platform of the ELF file.
        /// </summary>
        public ELFTargetPlatform TargetPlatform
        {
            get { return (ELFTargetPlatform)_header.ei_osabi; }
        }

        /// <summary>
        /// Gets the target platform version of the ELF file.
        /// </summary>
        public int TargetPlatformVersion
        {
            get { return _header.ei_abiversion; }
        }

        /// <summary>
        /// Gets the object type of the ELF file.
        /// </summary>
        public ELFType Type
        {
            get { return (ELFType)_header.e_type; }
        }

        /// <summary>
        /// Gets the instruction set used in the ELF file.
        /// </summary>
        public ELFInstructionSet InstructionSet
        {
            get { return (ELFInstructionSet)_header.e_machine; }
        }

        /// <summary>
        /// Gets the main entry point address in the ELF file.
        /// </summary>
        public ulong EntryPoint
        {
            get { return _header.e_entry; }
        }

        /// <summary>
        /// Gets the ELF file flags.
        /// </summary>
        public uint Flags
        {
            get { return _header.e_flags; }
        }
        
        internal void InternalRead(BinaryReader reader)
        {
            _header = new ELFHeader(reader);
            _sections = new List<ELFSection>(_header.e_shnum);

            if (_header.e_shoff != 0)
            {
                reader.Seek((long)_header.e_shoff, SeekOrigin.Begin);

                _sections[_header.e_shstrndx] = new ELFSection(reader, SizeFormat);

                for (int i = 0; i < _header.e_shnum; i++)
                {
                    _sections.Add(new ELFSection(reader, SizeFormat));
                }

                for (int i = 0; i < _header.e_shnum; i++)
                {
                    _sections[i].GetNameFromStringTable(_sections[_header.e_shstrndx], reader);
                }
            }
        }
    }
}
