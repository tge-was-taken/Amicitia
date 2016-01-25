using System;
using System.IO;
using AtlusLibSharp.Utilities;
using System.Diagnostics;

namespace AtlusLibSharp.SMT3
{
    public class DirectoryDataTable
    {
        internal const int IMG_BLOCKSIZE = 0x800;

        private DirectoryDataTableDirectoryEntry _rootDirectory;

        public DirectoryDataTableDirectoryEntry RootDirectory
        {
            get { return _rootDirectory; }
        }

        public DirectoryDataTable(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                _rootDirectory = (DirectoryDataTableDirectoryEntry)DirectoryDataTableEntryFactory.GetEntry(null, reader);
            }
        }
    }

    internal class DirectoryDataTableWritingContext
    {
        /*
        public BinaryWriter Writer;
        public long DataOffsetBase;
        public long NameOffsetBase;
        public long FileOffsetBase;
        */
    }

    public abstract class DirectoryDataTableEntry
    {
        internal const int SizeInBytes = 12;

        protected int _nameOffset;
        protected uint _dataOffset;
        protected string _name;
        protected DirectoryDataTableEntry _parent;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public DirectoryDataTableEntry Parent
        {
            get { return _parent; }
        }

        public override string ToString()
        {
            return Name;
        }

        public abstract void Export(FileStream img, string rootPath);

        internal abstract void InternalWrite(DirectoryDataTableWritingContext ctx);
    }

    public class DirectoryDataTableDirectoryEntry : DirectoryDataTableEntry
    {
        private int _numChildren;
        private DirectoryDataTableEntry[] _childEntries;

        public DirectoryDataTableEntry[] Entries
        {
            get { return _childEntries; }
        }

        public DirectoryDataTableDirectoryEntry(BinaryReader reader, DirectoryDataTableEntry parent, int nameOffset, uint dataOffset, int numChildren)
        {
            _parent = parent;
            _nameOffset = nameOffset;
            _dataOffset = dataOffset;
            _name = reader.ReadCStringAtOffset(_nameOffset);
            _numChildren = numChildren;
            _childEntries = new DirectoryDataTableEntry[numChildren];

            reader.Seek(dataOffset, SeekOrigin.Begin);
            for (int i = 0; i < numChildren; i++)
            {
                _childEntries[i] = DirectoryDataTableEntryFactory.GetEntry(this, reader);
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

        internal override void InternalWrite(DirectoryDataTableWritingContext ctx)
        {
            throw new NotImplementedException();
        }
    }

    public class DirectoryDataTableFileEntry : DirectoryDataTableEntry
    {
        private int _size;

        public DirectoryDataTableFileEntry(BinaryReader reader, DirectoryDataTableEntry parent, int nameOffset, uint dataOffset, int size)
        {
            _parent = parent;
            _nameOffset = nameOffset;
            _name = reader.ReadCStringAtOffset(_nameOffset);
            _dataOffset = dataOffset * DirectoryDataTable.IMG_BLOCKSIZE;
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

        internal override void InternalWrite(DirectoryDataTableWritingContext ctx)
        {
            throw new NotImplementedException();
        }
    }

    internal static class DirectoryDataTableEntryFactory
    {
        public static DirectoryDataTableEntry GetEntry(DirectoryDataTableEntry parent, BinaryReader reader)
        {
            int nameOffset = reader.ReadInt32();
            uint dataOffset = reader.ReadUInt32();
            int numData = reader.ReadInt32();

            long posNextEntry = reader.GetPosition();

            DirectoryDataTableEntry entry;

            if (numData < 0)
            {
                int numChildren = ~numData + 1;
                entry = new DirectoryDataTableDirectoryEntry(reader, parent, nameOffset, dataOffset, numChildren);
            }
            else
            {
                entry = new DirectoryDataTableFileEntry(reader, parent, nameOffset, dataOffset, numData);
            }

            reader.Seek(posNextEntry, SeekOrigin.Begin);

            return entry;
        }
    }
}
