namespace AtlusLibSharp.SMT3.ChunkResources.Scripting
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    using Utilities;
    using Graphics;
    using Modeling;

    public class BFChunk : Chunk
    {
        // Constants
        internal const int FLW0_FLAG = 0;
        internal const string FLW0_TAG = "FLW0";

        // Fields
        private int _numTypeTableEntries;
        private int _numUnknown;
        private TypeTableEntry[] _typeTable;

        // Constructors
        internal BFChunk(ushort id, int length, BinaryReader reader)
            : base(FLW0_FLAG, id, length, FLW0_TAG)
        {
            Read(reader);
        }

        public BFChunk(string xmlPath)
            : base(FLW0_FLAG, 0, 0, FLW0_TAG)
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

        public void Extract(string baseName)
        {
            XDocument xDoc = new XDocument();
            XElement xRoot = new XElement("FLW0");
            for (int i = 0; i < _numTypeTableEntries; i++)
            {
                string fileName = $"{baseName}_{i}";

                if (_typeTable[i].ElementLength == 1 && _typeTable[i].ElementCount > 16)
                {
                    AddExtensionIfMatched(_typeTable[i].Data, MSGChunk.MSG1_TAG, ".BMD", ref fileName);
                    AddExtensionIfMatched(_typeTable[i].Data, MDChunk.MD00_TAG, ".MB", ref fileName);
                    AddExtensionIfMatched(_typeTable[i].Data, TXPChunk.TXP0_TAG, ".TB", ref fileName);
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
            xDoc.Save(Path.GetPathRoot(baseName) + Path.GetFileNameWithoutExtension(baseName) + ".XML");
        }

        internal override void InternalWrite(BinaryWriter writer)
        {
            int fp = (int)writer.BaseStream.Position;

            writer.BaseStream.Seek(CHUNK_HEADER_SIZE, SeekOrigin.Current);
            writer.AlignPosition(16);

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

            Length = (int)writer.BaseStream.Position - fp;

            writer.BaseStream.Seek(arrayDescriptorPos, SeekOrigin.Begin);
            for (int i = 0; i < _numTypeTableEntries; i++)
            {
                _typeTable[i].WriteEntryInfo(writer);
            }

            writer.BaseStream.Seek(fp, SeekOrigin.Begin);
            writer.Write(Flags);
            writer.Write(UserID);
            writer.Write(Length);
            writer.WriteCString(Tag, 4);

            writer.BaseStream.Seek(fp + Length, SeekOrigin.Begin);
        }

        private void Read(BinaryReader reader)
        {
            reader.AlignPosition(16);

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
