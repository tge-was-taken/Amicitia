namespace AtlusLibSharp.SMT3.Scripting
{
    using System.IO;
    using System.Xml.Linq;

    using Common.Utilities;

    public class TypeTableEntry
    {
        internal const int TYPETABLE_ENTRY_LENGTH = 16;

        private int _type;
        private int _elementLength;
        private int _elementCount;
        private int _dataOffset;
        private byte[] _data;

        internal TypeTableEntry(BinaryReader reader)
        {
            _type = reader.ReadInt32();
            _elementLength = reader.ReadInt32();
            _elementCount = reader.ReadInt32();
            _dataOffset = reader.ReadInt32();
            _data = reader.ReadBytesAtOffset(_elementCount * _elementLength, _dataOffset);
        }

        internal TypeTableEntry(XElement xElement)
        {
            _type = int.Parse(xElement.Attribute("Type").Value);
            _elementLength = int.Parse(xElement.Attribute("ElementLength").Value);
            _data = File.ReadAllBytes(xElement.Attribute("FileName").Value);
            _elementCount = _data.Length / _elementLength;
        }

        public int Type
        {
            get { return _type; }
        }

        public int ElementLength
        {
            get { return _elementLength; }
        }

        public int ElementCount
        {
            get { return _elementCount; }
        }

        public byte[] Data
        {
            get { return _data; }
        }

        internal int DataOffset
        {
            get { return _dataOffset; }
            set { _dataOffset = value; }
        }

        internal void WriteEntryInfo(BinaryWriter writer)
        {
            writer.Write(_type);
            writer.Write(_elementLength);
            writer.Write(_elementCount);
            writer.Write(_dataOffset);
        }

        internal XElement ConvertToXml(string fileName)
        {
            XElement element =
                new XElement("TypeTableEntry",
                new XAttribute("Type", _type),
                new XAttribute("ElementLength", _elementLength),
                new XAttribute("FileName", fileName));
            return element;
        }
    }
}
