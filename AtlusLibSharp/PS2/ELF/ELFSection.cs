using AtlusLibSharp.Utilities;
using System.IO;

namespace AtlusLibSharp.PS2.ELF
{
    internal class ELFSection
    {
        // header
        private ELFSectionHeader _header;

        // backing stores
        private string _name;

        internal ELFSection(BinaryReader reader, ELFSizeFormat fmt)
        {
            _header = new ELFSectionHeader(reader, fmt);
        }

        internal void GetNameFromStringTable(ELFSection stringTableSection, BinaryReader reader)
        {
            reader.Seek((long)(stringTableSection._header.sh_offset + _header.sh_name), SeekOrigin.Begin);
            _name = reader.ReadCString();
        }

        /// <summary>
        /// Gets or sets the name of the section.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets the type description of the section.
        /// </summary>
        public ELFSectionType Type
        {
            get { return (ELFSectionType)_header.sh_type; }
        }
    }
}
