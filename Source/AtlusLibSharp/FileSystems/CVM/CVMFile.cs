using System.IO;

namespace AtlusLibSharp.FileSystems.CVM
{
    using ISO;
    using AtlusLibSharp.Utilities;

    public class CvmFile
    {
        internal const int CVM_HEADER_SIZE = 0x1800;
        internal const int ISO_RESERVED_SIZE = 0x8000;
        internal const int ISO_BLOCKSIZE = 0x800;
        internal const int ISO_ROOTDIRECTORY_OFFSET = 0x9C;
        internal const int ID_PRIM_VOLUME_DESC = 0x01;

        private IsoDirectoryRecord mRootDirectory;

        public IsoDirectoryRecord RootDirectory
        {
            get { return mRootDirectory; }
        }

        public CvmFile(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                InternalRead(reader);
        }

        public CvmFile(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
                InternalRead(reader);
        }

        private void InternalRead(BinaryReader reader)
        {
            reader.Seek(CVM_HEADER_SIZE + ISO_RESERVED_SIZE, SeekOrigin.Begin);

            byte sectorType = reader.ReadByte();

            while (sectorType != ID_PRIM_VOLUME_DESC)
            {
                reader.Seek(ISO_BLOCKSIZE - 1, SeekOrigin.Current);
                sectorType = reader.ReadByte();
            }

            reader.Seek(ISO_ROOTDIRECTORY_OFFSET - 1, SeekOrigin.Current);
            mRootDirectory = new IsoDirectoryRecord(reader, null);
        }
    }
}
