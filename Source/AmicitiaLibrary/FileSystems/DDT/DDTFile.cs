namespace AmicitiaLibrary.FileSystems.DDT
{
    using System;
    using System.Diagnostics;
    using AmicitiaLibrary.Utilities;
    using System.IO;

    public class DdtFile
    {
        internal const int IMG_BLOCKSIZE = 0x800;

        private DdtDirectoryEntry mRootDirectory;

        public DdtDirectoryEntry RootDirectory
        {
            get { return mRootDirectory; }
        }

        public DdtFile(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                mRootDirectory = (DdtDirectoryEntry)DdtEntryFactory.GetEntry(null, reader);
            }
        }
    }

    internal class DdtWritingContext
    {
        /*
        public BinaryWriter Writer;
        public long DataOffsetBase;
        public long NameOffsetBase;
        public long FileOffsetBase;
        */
    }

    public abstract class DdtEntry
    {
        internal const int SizeInBytes = 12;

        protected int NameOffset;
        protected uint DataOffset;
        protected string _name;
        protected DdtEntry _parent;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public DdtEntry Parent
        {
            get { return _parent; }
        }

        public override string ToString()
        {
            return Name;
        }

        public abstract void Export(FileStream img, string rootPath);

        internal abstract void InternalWrite(DdtWritingContext ctx);
    }

    public class DdtDirectoryEntry : DdtEntry
    {
        private int mNumChildren;
        private DdtEntry[] mChildEntries;

        public DdtEntry[] Entries
        {
            get { return mChildEntries; }
        }

        public DdtDirectoryEntry(BinaryReader reader, DdtEntry parent, int nameOffset, uint dataOffset, int numChildren)
        {
            _parent = parent;
            NameOffset = nameOffset;
            DataOffset = dataOffset;
            _name = reader.ReadCStringAtOffset(NameOffset);
            mNumChildren = numChildren;
            mChildEntries = new DdtEntry[numChildren];

            reader.Seek(dataOffset, SeekOrigin.Begin);
            for (int i = 0; i < numChildren; i++)
            {
                mChildEntries[i] = DdtEntryFactory.GetEntry(this, reader);
            }
        }

        public override void Export(FileStream img, string rootPath)
        {
            Debug.WriteLine($"Exporting folder: {_name}");
            string newRootPath = rootPath + "//" + _name;
            Directory.CreateDirectory(newRootPath);
            for (int i = 0; i < mNumChildren; i++)
            {
                mChildEntries[i].Export(img, newRootPath);
            }
        }

        internal override void InternalWrite(DdtWritingContext ctx)
        {
            throw new NotImplementedException();
        }
    }

    public class DdtFileEntry : DdtEntry
    {
        private int mSize;

        public DdtFileEntry(BinaryReader reader, DdtEntry parent, int nameOffset, uint dataOffset, int size)
        {
            _parent = parent;
            NameOffset = nameOffset;
            _name = reader.ReadCStringAtOffset(NameOffset);
            DataOffset = dataOffset * DdtFile.IMG_BLOCKSIZE;
            mSize = size;
        }

        public override void Export(FileStream img, string rootPath)
        {
            Debug.WriteLine($"Exporting file: {_name}");
            byte[] data = new byte[mSize];
            img.Position = DataOffset;
            img.Read(data, 0, mSize);
            Directory.CreateDirectory(rootPath);
            File.WriteAllBytes(rootPath + "\\" + _name, data);
        }

        internal override void InternalWrite(DdtWritingContext ctx)
        {
            throw new NotImplementedException();
        }
    }

    internal static class DdtEntryFactory
    {
        public static DdtEntry GetEntry(DdtEntry parent, BinaryReader reader)
        {
            int nameOffset = reader.ReadInt32();
            uint dataOffset = reader.ReadUInt32();
            int numData = reader.ReadInt32();

            long posNextEntry = reader.GetPosition();

            DdtEntry entry;

            if (numData < 0)
            {
                int numChildren = ~numData + 1;
                entry = new DdtDirectoryEntry(reader, parent, nameOffset, dataOffset, numChildren);
            }
            else
            {
                entry = new DdtFileEntry(reader, parent, nameOffset, dataOffset, numData);
            }

            reader.Seek(posNextEntry, SeekOrigin.Begin);

            return entry;
        }
    }
}
