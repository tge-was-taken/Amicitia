namespace AmicitiaLibrary.Scripting
{
    using System.IO;
    using System.Collections.Generic;
    using System.Text;

    using AmicitiaLibrary.Utilities;
    using IO;
    using Compression;

    public class BmdFile : BinaryBase
    {
        internal const short Flag = 0x0007;
        internal const string Tag = "MSG1";
        internal const byte DataStartAddress = 0x20;
        internal const int UnkConstant = 0x20000;

        private BmdMessage[] mMessages;
        private string[] mActorNames;

        #region Properties

        public int DialogCount
        {
            get { return mMessages.Length; }
        }

        public BmdMessage[] Messages
        {
            get { return mMessages; }
            internal set { mMessages = value; }
        }

        public int ActorCount
        {
            get { return mActorNames.Length; }
        }

        public string[] ActorNames
        {
            get { return mActorNames; }
            internal set { mActorNames = value; }
        }

        #endregion

        public BmdFile(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                InternalRead(reader);
        }

        public BmdFile(Stream stream, bool leaveStreamOpen)
        {
            using (BinaryReader reader = new BinaryReader(stream, Encoding.Default, leaveStreamOpen))
                InternalRead(reader);
        }

        internal BmdFile(BinaryReader reader)
        {
            InternalRead(reader);
        }

        internal BmdFile()
        {
        }

        internal override void Write(BinaryWriter writer)
        {
            int posFileStart = (int)writer.BaseStream.Position;
            List<int> addressList = new List<int>();

            // Seek past chunk and msg header for writing later
            writer.BaseStream.Seek(DataStartAddress, SeekOrigin.Current);

            // Write a dummy message pointer table
            for (int i = 0; i < DialogCount; i++)
            {
                writer.Write(0);
                addressList.Add((int)writer.BaseStream.Position - posFileStart);
                writer.Write(0);
            }

            // Write a dummy offset for writing later
            addressList.Add((int)writer.BaseStream.Position - posFileStart);
            writer.Write(0);
            writer.Write(ActorCount);

            // These are always here for some reason
            writer.Write(0);
            writer.Write(0);

            writer.AlignPosition(4);

            BmdMessageTable[] messagePointerTable = new BmdMessageTable[DialogCount];

            // Write the messages and fill in the message pointer table
            for (int i = 0; i < DialogCount; i++)
            {
                writer.AlignPosition(4);
                messagePointerTable[i].Offset = (int)writer.BaseStream.Position - DataStartAddress - posFileStart;
                mMessages[i].InternalWrite(writer, ref addressList, posFileStart);
            }

            writer.AlignPosition(4);
            int actorNamePointerTableOffset = (int)writer.BaseStream.Position - DataStartAddress - posFileStart;

            // Write dummy actor name pointer table if there are actors present
            if (ActorCount > 0)
            {
                long actorNamePointerTablePosition = writer.BaseStream.Position;
                for (int i = 0; i < ActorCount; i++)
                {
                    addressList.Add((int)writer.BaseStream.Position - posFileStart);
                    writer.Write(0);
                }

                int[] actorNamePointerTable = new int[ActorCount];
                for (int i = 0; i < actorNamePointerTable.Length; i++)
                {
                    actorNamePointerTable[i] = (int)writer.BaseStream.Position - DataStartAddress - posFileStart;
                    writer.WriteCString(mActorNames[i]);
                }

                long addresRelocPosition = writer.BaseStream.Position;

                writer.BaseStream.Seek(actorNamePointerTablePosition, SeekOrigin.Begin);
                writer.Write(actorNamePointerTable);

                writer.BaseStream.Seek(addresRelocPosition, SeekOrigin.Begin);
            }

            // Compress and write the address relocationt able
            byte[] addressRelocTable = PointerRelocationTableCompression.Compress(addressList, DataStartAddress);
            int addressRelocTableOffset = (int)writer.BaseStream.Position - posFileStart;
            int addressRelocTableSize = addressRelocTable.Length;
            writer.Write(addressRelocTable);

            // Save the end offset for calculating length and seeking later
            long posFileEnd = writer.BaseStream.Position;
            int length = (int)(posFileEnd - posFileStart);

            // Seek back to the start
            writer.BaseStream.Seek(posFileStart, SeekOrigin.Begin);

            // Write Chunk header
            writer.Write(Flag);
            writer.Write((short)0); // userID
            writer.Write(length);
            writer.WriteCString(Tag, 4);
            writer.Write(0);

            // Write MSG header
            writer.Write(addressRelocTableOffset);
            writer.Write(addressRelocTableSize);
            writer.Write(DialogCount);
            writer.Write(UnkConstant);

            for (int i = 0; i < DialogCount; i++)
            {
                writer.Write((int)messagePointerTable[i].Type);
                writer.Write(messagePointerTable[i].Offset);
            }

            writer.Write(actorNamePointerTableOffset);

            writer.BaseStream.Seek(posFileEnd, SeekOrigin.Begin);
        }

        private void InternalRead(BinaryReader reader)
        {
            long posFileStart = reader.GetPosition();
            short flag = reader.ReadInt16();
            short userId = reader.ReadInt16();
            int length = reader.ReadInt32();
            string tag = reader.ReadCString(4);
            reader.AlignPosition(16);

            if (tag != Tag)
            {
                throw new InvalidDataException();
            }

            int addressRelocTableOffset = reader.ReadInt32();
            int addressRelocTableSize = reader.ReadInt32();
            int numMessages = reader.ReadInt32();
            short isRelocated = reader.ReadInt16(); // actually a byte but not very important
            short unk0X1E = reader.ReadInt16();

            /*
            if (unk0x1C != UNK_CONSTANT)
            {
                Console.WriteLine("_unk0x1C isn't 0x20000");
            }
            */

            BmdMessageTable[] messagePointerTable = new BmdMessageTable[numMessages];
            for (int i = 0; i < messagePointerTable.Length; i++)
            {
                messagePointerTable[i].Type = (BmdMessageType)reader.ReadInt32();
                messagePointerTable[i].Offset = reader.ReadInt32();
            }

            int actorNamePointerTableOffset = reader.ReadInt32();
            int numActors = reader.ReadInt32();

            reader.BaseStream.Seek(posFileStart + DataStartAddress + actorNamePointerTableOffset, SeekOrigin.Begin);
            int[] actorNamePointerTable = reader.ReadInt32Array(numActors);

            mActorNames = new string[numActors];
            for (int i = 0; i < mActorNames.Length; i++)
            {
                reader.BaseStream.Seek(posFileStart + DataStartAddress + actorNamePointerTable[i], SeekOrigin.Begin);
                mActorNames[i] = reader.ReadCString();
            }

            mMessages = new BmdMessage[numMessages];
            for (int i = 0; i < mMessages.Length; i++)
            {
                mMessages[i] = BmdMessageFactory.GetMessage(reader, (int)posFileStart, messagePointerTable[i]);
            }
        }
    }

    struct BmdMessageTable
    {
        public BmdMessageType Type;
        public int Offset;
    }

    public enum BmdMessageType : int
    {
        Standard = 0,
        Selection = 1
    }
}
