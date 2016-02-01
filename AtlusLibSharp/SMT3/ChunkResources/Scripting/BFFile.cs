namespace AtlusLibSharp.SMT3.ChunkResources.Scripting
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    using Utilities;
    using Graphics;

    public class BFFile : BinaryFileBase
    {
        // Constants
        internal const byte HEADER_SIZE = 0x10;
        internal const short FLAG = 0;
        internal const string TAG = "FLW0";

        // Fields
        private int _numTypeTableEntries;
        private int _numUnknown;
        private TypeTableEntry[] _typeTable;

        // Constructors
        internal BFFile(BinaryReader reader)
        {
            InternalRead(reader);
        }

        public BFFile(string xmlPath)
        {
            XDocument xDoc = XDocument.Load(xmlPath);
            XElement xRoot = xDoc.Root;

            if (xRoot.Name != "FLW0")
            {
                throw new InvalidDataException($"Root element name is \"{xRoot.Name}\". Expected \"FLW0\"");
            }

            XElement[] arrayDescriptorElements = xRoot.Elements().ToArray();

            _numTypeTableEntries = arrayDescriptorElements.Length;
            _typeTable = new TypeTableEntry[_numTypeTableEntries];

            for (int i = 0; i < _numTypeTableEntries; i++)
            {
                _typeTable[i] = new TypeTableEntry(arrayDescriptorElements[i]);
            }
        }

        // Public Static Methods
        public static BFFile LoadFrom(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                return new BFFile(reader);
        }

        public static BFFile LoadFrom(Stream stream, bool leaveStreamOpen)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveStreamOpen))
                return new BFFile(reader);
        }

        // Public methods
        public void Extract(string baseName)
        {
            XDocument xDoc = new XDocument();
            XElement xRoot = new XElement("FLW0");
            for (int i = 0; i < _numTypeTableEntries; i++)
            {
                string fileName = $"{baseName}_{i}";

                if (_typeTable[i].ElementLength == 1 && _typeTable[i].ElementCount > 16)
                {
                    AddExtensionIfMatched(_typeTable[i].Data, BMDFile.TAG, ".BMD", ref fileName);
                    AddExtensionIfMatched(_typeTable[i].Data, TBFile.TAG, ".TB", ref fileName);
                    AddExtensionIfMatched(_typeTable[i].Data, "PIB0", ".PB", ref fileName);
                    if (!Path.HasExtension(fileName))
                    {
                        fileName += ".BIN";
                    }
                }
                else
                {
                    fileName += ".BIN";
                }

                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                File.WriteAllBytes(fileName, _typeTable[i].Data);

                xRoot.Add(_typeTable[i].ConvertToXml(fileName));
            }

            xDoc.Add(xRoot);
            string xmlPath = Path.GetDirectoryName(baseName) + "\\" + Path.GetFileNameWithoutExtension(baseName) + ".XML";
            xDoc.Save(xmlPath);
        }

        internal override void InternalWrite(BinaryWriter writer)
        {
            int posFileStart = (int)writer.BaseStream.Position;

            writer.BaseStream.Seek(HEADER_SIZE, SeekOrigin.Current);

            writer.Write(_numTypeTableEntries);
            writer.Write(_numUnknown);
            writer.AlignPosition(16);

            long arrayDescriptorPos = writer.BaseStream.Position;
            writer.BaseStream.Seek(TypeTableEntry.TYPETABLE_ENTRY_LENGTH * _numTypeTableEntries, SeekOrigin.Current);

            for (int i = 0; i < _numTypeTableEntries; i++)
            {
                _typeTable[i].DataOffset = (int)writer.BaseStream.Position;
                writer.Write(_typeTable[i].Data);
            }

            long posFileEnd = writer.BaseStream.Position;
            int length = (int)posFileEnd - posFileStart;

            writer.BaseStream.Seek(arrayDescriptorPos, SeekOrigin.Begin);
            for (int i = 0; i < _numTypeTableEntries; i++)
            {
                _typeTable[i].WriteEntryInfo(writer);
            }

            writer.BaseStream.Seek(posFileStart, SeekOrigin.Begin);
            writer.Write(FLAG);
            writer.Write((short)0); // userID
            writer.Write(length);
            writer.WriteCString(TAG, 4);

            writer.BaseStream.Seek(posFileEnd, SeekOrigin.Begin);
            writer.AlignPosition(64);
        }

        private void InternalRead(BinaryReader reader)
        {
            long posFileStart = reader.GetPosition();
            short flag = reader.ReadInt16();
            short userID = reader.ReadInt16();
            int length = reader.ReadInt32();
            string tag = reader.ReadCString(4);
            reader.AlignPosition(16);

            if (tag != TAG)
            {
                throw new InvalidDataException();
            }


            _numTypeTableEntries = reader.ReadInt32();
            _numUnknown = reader.ReadInt32();

            reader.AlignPosition(16);

            _typeTable = new TypeTableEntry[_numTypeTableEntries];
            for (int i = 0; i < _numTypeTableEntries; i++)
            {
                _typeTable[i] = new TypeTableEntry(reader);
            }
        }

        private void AddExtensionIfMatched(byte[] data, string tag, string extension, ref string baseName)
        {
            if (CheckSignature(data, tag))
            {
                baseName += extension;
            }
        }

        private bool CheckSignature(byte[] data, string tag)
        {
            return (data[08] == tag[0] &&
                    data[09] == tag[1] &&
                    data[10] == tag[2] &&
                    data[11] == tag[3]);
        }
    }
}
