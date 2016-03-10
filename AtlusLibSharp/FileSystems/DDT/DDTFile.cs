namespace AtlusLibSharp.FileSystems.DDT
{
    using System;
    using System.Diagnostics;
    using AtlusLibSharp.Utilities;
    using System.IO;

    public class DDTFile
    {
        internal const int IMG_BLOCKSIZE = 0x800;

        private DDTDirectoryEntry _rootDirectory;

        public DDTDirectoryEntry RootDirectory
        {
            get { return _rootDirectory; }
        }

        public DDTFile(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                _rootDirectory = (DDTDirectoryEntry)DDTEntryFactory.GetEntry(null, reader);
            }
        }
    }

    internal class DDTWritingContext
    {
        /*
        public BinaryWriter Writer;
        public long DataOffsetBase;
        public long NameOffsetBase;
        public long FileOffsetBase;
        */
    }

    public abstract class DDTEntry
    {
        internal const int SizeInBytes = 12;

        protected int _nameOffset;
        protected uint _dataOffset;
        protected string _name;
        protected DDTEntry _parent;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public DDTEntry Parent
        {
            get { return _parent; }
        }

        public override string ToString()
        {
            return Name;
        }

        public abstract void Export(FileStream img, string rootPath);

        internal abstract void InternalWrite(DDTWritingContext ctx);
    }

    public class DDTDirectoryEntry : DDTEntry
    {
        private int _numChildren;
        private DDTEntry[] _childEntries;

        public DDTEntry[] Entries
        {
            get { return _childEntries; }
        }

        public DDTDirectoryEntry(BinaryReader reader, DDTEntry parent, int nameOffset, uint dataOffset, int numChildren)
        {
            _parent = parent;
            _nameOffset = nameOffset;
            _dataOffset = dataOffset;
            _name = reader.ReadCStringAtOffset(_nameOffset);
            _numChildren = numChildren;
            _childEntries = new DDTEntry[numChildren];

            reader.Seek(dataOffset, SeekOrigin.Begin);
            for (int i = 0; i < numChildren; i++)
            {
                _childEntries[i] = DDTEntryFactory.GetEntry(this, reader);
            }
        }

        public override void Export(FileStream img, string rootPath)
        {
            Debug.WriteLine($"Exporting folder: {_name}");
            string newRootPath = rootPath + "//" + _name;
            Directory.CreateDirectory(newRootPath);
            for (int i = 0; i < _numChildren; i++)
            {
                _childEntries[i].Export(img, newRootPath);
            }
        }

        internal override void InternalWrite(DDTWritingContext ctx)
        {
            throw new NotImplementedException();
        }
    }

    public class DDTFileEntry : DDTEntry
    {
        private int _size;

        public DDTFileEntry(BinaryReader reader, DDTEntry parent, int nameOffset, uint dataOffset, int size)
        {
            _parent = parent;
            _nameOffset = nameOffset;
            _name = reader.ReadCStringAtOffset(_nameOffset);
            _dataOffset = dataOffset * DDTFile.IMG_BLOCKSIZE;
            _size = size;
        }

        public override void Export(FileStream img, string rootPath)
        {
            Debug.WriteLine($"Exporting file: {_name}");
            byte[] data = new byte[_size];
            img.Position = _dataOffset;
            img.Read(data, 0, _size);
            Directory.CreateDirectory(rootPath);
            File.WriteAllBytes(rootPath + "\\" + _name, data);
        }

        internal override void InternalWrite(DDTWritingContext ctx)
        {
            throw new NotImplementedException();
        }
    }

    internal static class DDTEntryFactory
    {
        public static DDTEntry GetEntry(DDTEntry parent, BinaryReader reader)
        {
            int nameOffset = reader.ReadInt32();
            uint dataOffset = reader.ReadUInt32();
            int numData = reader.ReadInt32();

            long posNextEntry = reader.GetPosition();

            DDTEntry entry;

            if (numData < 0)
            {
                int numChildren = ~numData + 1;
                entry = new DDTDirectoryEntry(reader, parent, nameOffset, dataOffset, numChildren);
            }
            else
            {
                entry = new DDTFileEntry(reader, parent, nameOffset, dataOffset, numData);
            }

            reader.Seek(posNextEntry, SeekOrigin.Begin);

            return entry;
        }
    }
}
