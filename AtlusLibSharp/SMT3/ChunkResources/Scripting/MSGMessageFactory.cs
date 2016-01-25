namespace AtlusLibSharp.SMT3.ChunkResources.Scripting
{
    using System;
    using System.IO;
    using System.Xml.Linq;

    internal static class MSGMessageFactory
    {
        internal static MSGMessage GetMessage(BinaryReader reader, int filePointer, MessageTable msgTbl)
        {
            reader.BaseStream.Seek(filePointer + MSGChunk.MSG1_DATA_START_ADDRESS + msgTbl.Offset, SeekOrigin.Begin);
            switch (msgTbl.Type)
            {
                case 0:
                    return new MSGMessageType0(reader, filePointer);

                case 1:
                    return new MSGMessageType1(reader, filePointer);

                default:
                    throw new ArgumentException("Unknown message type: " + msgTbl.Type);
            }
        }

        internal static MSGMessage GetMessage(XElement xElement, int type)
        {
            switch (type)
            {
                case 0:
                    return new MSGMessageType0(xElement);

                case 1:
                    return new MSGMessageType1(xElement);

                default:
                    throw new ArgumentException("Unknown message type: " + type);
            }
        }
    }
}
