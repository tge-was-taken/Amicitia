namespace AtlusLibSharp.SMT3.Scripting
{
    using System;
    using System.IO;
    using System.Xml.Linq;

    internal static class BMDMessageFactory
    {
        internal static BMDMessage GetMessage(BinaryReader reader, int filePointer, BMDMessageTable msgTbl)
        {
            reader.BaseStream.Seek(filePointer + BMDFile.DATA_START_ADDRESS + msgTbl.Offset, SeekOrigin.Begin);
            switch (msgTbl.Type)
            {
                case MessageType.Regular:
                    return new BMDRegularMessage(reader, filePointer);

                case MessageType.Selection:
                    return new BMDSelectionMessage(reader, filePointer);

                default:
                    throw new ArgumentException("Unknown message type: " + msgTbl.Type);
            }
        }

        internal static BMDMessage GetMessage(XElement xElement, MessageType type)
        {
            switch (type)
            {
                case MessageType.Regular:
                    return new BMDRegularMessage(xElement);

                case MessageType.Selection:
                    return new BMDSelectionMessage(xElement);

                default:
                    throw new ArgumentException("Unknown message type: " + type);
            }
        }
    }
}
