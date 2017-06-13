using System.IO.MemoryMappedFiles;
using AtlusLibSharp.Utilities;

namespace AtlusLibSharp.FileSystems.CVM
{
    public class CVMFileRewrite
    {
        internal CVMHeader m_header;

        public void Load(string path)
        {
            using (SafeBufferAccessor access = new SafeBufferAccessor(path))
            {
                InternalReadHeader(access);
            }
        }

        private void InternalReadHeader(SafeBufferAccessor accessor)
        {
            accessor.Read(4, out m_header.blockTag);
            accessor.ReadBE(out m_header.blockLength);
            accessor.Position += 16; // unused
            accessor.ReadBE(out m_header.totalSize);
            accessor.ReadBE(out m_header.dateTime);
            accessor.ReadBE(out m_header.field2C);
            accessor.ReadBE(out m_header.flags);
            accessor.Read(4, out m_header.formatTag);
            accessor.Read(64, out m_header.makerTag);
            accessor.ReadBE(out m_header.field78);
            accessor.ReadBE(out m_header.field7C);
            accessor.ReadBE(out m_header.numEntries);
            accessor.ReadBE(out m_header.zoneInfoIndex);
            accessor.ReadBE(out m_header.isoStartIndex);
            accessor.Position += 116; // unused
            accessor.ReadBE((int)m_header.numEntries, out m_header.entryTable);
        }

        private CVMSection InternalReadSection(MemoryMappedViewAccessor accessor, ref long pos)
        {
            CVMSection section;
            accessor.Read(ref pos, 4, out section.tag);
            accessor.ReadBE(ref pos, out section.length);
            return section;
        }

        internal struct CVMHeader
        {
            public string blockTag;     // 0x00, 4 bytes
            public ulong blockLength;   // 0x04, big endian
                                        // 0x0C, 16 bytes unused
            public ulong totalSize;     // 0x1C, big endian
            public ulong dateTime;      // 0x24, iso datetime format
            public uint field2C;        // 0x2C, value = 0x01010000
            public uint flags;          // 0x30, 0x10 = encrypted TOC
            public string formatTag;    // 0x34, 4 bytes long
            public string makerTag;     // 0x38, 64 bytes long
            public uint field78;        // 0x78, value = 0x011f0000
            public uint field7C;        // 0x7C, value = 0x03000000
            public uint numEntries;     // 0x80, number of entries in the sector table
            public uint zoneInfoIndex;  // 0x84, index of zone info sector
            public uint isoStartIndex;  // 0x88, index of iso start sector
                                        // 0x8C, 116 bytes unused
            public uint[] entryTable;   // table of entry sector indices
        }
    }

    public class CVMSection
    {
        protected Header m_header;

        protected internal struct Header
        {
            public string tag;      // 0x00, 4 bytes
            public ulong length;    // 0x04, big endian
        }

        protected void InternalReadHeader(SafeBufferAccessor)
    }
}
