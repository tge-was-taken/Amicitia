namespace AmicitiaLibrary.Scripting
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using AmicitiaLibrary.Utilities;

    public class BmdSelectionMessage : BmdMessage
    {
        private ushort mField18;

        public override BmdMessageType MessageType
        {
            get { return BmdMessageType.Selection; }
        }

        internal BmdSelectionMessage(BinaryReader reader, int fp)
        {
            InternalRead(reader, fp);
        }

        internal BmdSelectionMessage() { }

        internal override void InternalWrite(BinaryWriter writer, ref List<int> addressList, int fp)
        {
            // Write header fields
            writer.WriteCString(name, NameLength);
            writer.Write(mField18);
            writer.Write((ushort)DialogCount);
            
            // unknown zero
            writer.Write(0);

            // Save the pointer table offset for writing later
            long pagePointerTableOffset = writer.BaseStream.Position;

            // dummy page pointer table
            for (int i = 0; i < DialogCount; i++)
            {
                addressList.Add((int)writer.BaseStream.Position - fp);
                writer.Write(0);
            }

            // dummy length
            writer.Write(0);

            int[] pagePointerTable = new int[DialogCount];

            // Write the pages
            long pageDataStart = writer.BaseStream.Position;
            for (int i = 0; i < DialogCount; i++)
            {
                pagePointerTable[i] = (int)writer.BaseStream.Position - BmdFile.DataStartAddress - fp;
                dialogs[i].InternalWrite(writer);
            }

            long dialogDataEnd = writer.BaseStream.Position;

            // Calculate the page data length
            int pageDataLength = (int)(dialogDataEnd - pageDataStart);

            // Seek to the pointer table and write it
            writer.BaseStream.Seek(pagePointerTableOffset, SeekOrigin.Begin);
            writer.Write(pagePointerTable);
            writer.Write(pageDataLength);

            // Seek back to the end of the data
            writer.BaseStream.Seek(dialogDataEnd, SeekOrigin.Begin);
        }

        private void InternalRead(BinaryReader reader, int fp)
        {
            name = reader.ReadCString(NameLength);
            mField18 = reader.ReadUInt16();
            ushort numChoices = reader.ReadUInt16();

            if (mField18 != 0)
            {
                throw new NotImplementedException("_unk0x18 is not zero!");
            }

            // unknown zero 4 bytesz
            reader.BaseStream.Seek(4, SeekOrigin.Current);

            int[] pagePointerTable = reader.ReadInt32Array(numChoices);
            int pageDataLength = reader.ReadInt32();

            dialogs = new BmdDialog[numChoices];
            for (int i = 0; i < numChoices; i++)
            {
                reader.BaseStream.Seek(fp + BmdFile.DataStartAddress + pagePointerTable[i], SeekOrigin.Begin);
                dialogs[i] = new BmdDialog(reader);
            }
        }
    }
}
