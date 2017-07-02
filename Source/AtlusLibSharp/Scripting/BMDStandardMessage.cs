namespace AtlusLibSharp.Scripting
{
    using System.Collections.Generic;
    using System.IO;
    using Utilities;

    public class BmdStandardMessage : BmdMessage
    {
        private short mActorIndex;

        public short ActorId
        {
            get { return mActorIndex; }
            internal set { mActorIndex = value; }
        }

        public override BmdMessageType MessageType
        {
            get { return BmdMessageType.Standard; }
        }

        internal BmdStandardMessage(BinaryReader reader, int fp)
        {
            InternalRead(reader, fp);
        }

        internal BmdStandardMessage() { }

        internal override void InternalWrite(BinaryWriter writer, ref List<int> addressList, int fp)
        {
            // Write header fields
            writer.WriteCString(name, NameLength);
            writer.Write((ushort)DialogCount);
            writer.Write(mActorIndex);

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
            ushort numDialogs = reader.ReadUInt16();
            mActorIndex = (reader.ReadInt16());

            if (mActorIndex != -1)
            {
                // High bit is a special flag?
                mActorIndex = (short)(mActorIndex & 0x7FFF);
            }

            int[] pagePointerTable = reader.ReadInt32Array(numDialogs);
            int pageDataLength = reader.ReadInt32();

            dialogs = new BmdDialog[numDialogs];
            for (int i = 0; i < numDialogs; i++)
            {
                reader.BaseStream.Seek(fp + BmdFile.DataStartAddress + pagePointerTable[i], SeekOrigin.Begin);
                dialogs[i] = new BmdDialog(reader);
            }
        }
    }
}
