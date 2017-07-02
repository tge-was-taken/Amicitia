namespace AtlusLibSharp.Scripting
{
    using System;
    using System.IO;

    internal static class BmdMessageFactory
    {
        internal static BmdMessage GetMessage(BinaryReader reader, int filePointer, BmdMessageTable msgTbl)
        {
            reader.BaseStream.Seek(filePointer + BmdFile.DataStartAddress + msgTbl.Offset, SeekOrigin.Begin);
            switch (msgTbl.Type)
            {
                case BmdMessageType.Standard:
                    return new BmdStandardMessage(reader, filePointer);

                case BmdMessageType.Selection:
                    return new BmdSelectionMessage(reader, filePointer);

                default:
                    throw new ArgumentException("SkinBoneIndexCount message id: " + msgTbl.Type);
            }
        }
    }
}
