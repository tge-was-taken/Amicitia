namespace AtlusLibSharp.Scripting
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using AtlusLibSharp.Utilities;

    public class BMDSelectionMessage : BMDMessage
    {
        private ushort _field18;

        public override BMDMessageType MessageType
        {
            get { return BMDMessageType.Selection; }
        }

        internal BMDSelectionMessage(BinaryReader reader, int fp)
        {
            InternalRead(reader, fp);
        }

        internal BMDSelectionMessage() { }

        internal override void InternalWrite(BinaryWriter writer, ref List<int> addressList, int fp)
        {
            // Write header fields
            writer.WriteCString(name, NAME_LENGTH);
            writer.Write(_field18);
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
                pagePointerTable[i] = (int)writer.BaseStream.Position - BMDFile.DATA_START_ADDRESS - fp;
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
            name = reader.ReadCString(NAME_LENGTH);
            _field18 = reader.ReadUInt16();
            ushort numChoices = reader.ReadUInt16();

            if (_field18 != 0)
            {
                throw new NotImplementedException("_unk0x18 is not zero!");
            }

            // unknown zero 4 bytesz
            reader.BaseStream.Seek(4, SeekOrigin.Current);

            int[] pagePointerTable = reader.ReadInt32Array(numChoices);
            int pageDataLength = reader.ReadInt32();

            dialogs = new BMDDialog[numChoices];
            for (int i = 0; i < numChoices; i++)
            {
                reader.BaseStream.Seek(fp + BMDFile.DATA_START_ADDRESS + pagePointerTable[i], SeekOrigin.Begin);
                dialogs[i] = new BMDDialog(reader);
            }
        }
    }
}
