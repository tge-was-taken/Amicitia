namespace AtlusLibSharp.Scripting
{
    using System;
    using System.IO;

    internal static class BMDMessageFactory
    {
        internal static BMDMessage GetMessage(BinaryReader reader, int filePointer, BMDMessageTable msgTbl)
        {
            reader.BaseStream.Seek(filePointer + BMDFile.DATA_START_ADDRESS + msgTbl.Offset, SeekOrigin.Begin);
            switch (msgTbl.Type)
            {
                case BMDMessageType.Standard:
                    return new BMDStandardMessage(reader, filePointer);

                case BMDMessageType.Selection:
                    return new BMDSelectionMessage(reader, filePointer);

                default:
                    throw new ArgumentException("Unknown message type: " + msgTbl.Type);
            }
        }
    }
}
