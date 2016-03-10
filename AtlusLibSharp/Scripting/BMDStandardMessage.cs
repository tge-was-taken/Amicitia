namespace AtlusLibSharp.Scripting
{
    using System.Collections.Generic;
    using System.IO;
    using Utilities;

    public class BMDStandardMessage : BMDMessage
    {
        private short _actorIndex;

        public short ActorID
        {
            get { return _actorIndex; }
            internal set { _actorIndex = value; }
        }

        public override BMDMessageType MessageType
        {
            get { return BMDMessageType.Standard; }
        }

        internal BMDStandardMessage(BinaryReader reader, int fp)
        {
            InternalRead(reader, fp);
        }

        internal BMDStandardMessage() { }

        internal override void InternalWrite(BinaryWriter writer, ref List<int> addressList, int fp)
        {
            // Write header fields
            writer.WriteCString(name, NAME_LENGTH);
            writer.Write((ushort)DialogCount);
            writer.Write(_actorIndex);

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
            ushort numDialogs = reader.ReadUInt16();
            _actorIndex = (reader.ReadInt16());

            if (_actorIndex != -1)
            {
                // High bit is a special flag?
                _actorIndex = (short)(_actorIndex & 0x7FFF);
            }

            int[] pagePointerTable = reader.ReadInt32Array(numDialogs);
            int pageDataLength = reader.ReadInt32();

            dialogs = new BMDDialog[numDialogs];
            for (int i = 0; i < numDialogs; i++)
            {
                reader.BaseStream.Seek(fp + BMDFile.DATA_START_ADDRESS + pagePointerTable[i], SeekOrigin.Begin);
                dialogs[i] = new BMDDialog(reader);
            }
        }
    }
}
