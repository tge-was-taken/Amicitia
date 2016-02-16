namespace AtlusLibSharp.SMT3.Scripting
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using System.Text;

    using Common.Utilities;
    using Common;

    public class BMDFile : BinaryFileBase
    {
        // Constants
        internal const short FLAG = 0x0007;
        internal const string TAG = "MSG1";
        internal const byte DATA_START_ADDRESS = 0x20;
        internal const int UNK_CONSTANT = 0x20000;

        // Private Fields
        private BMDMessage[] _messages;
        private string[] _actorNames;

        // Constructors
        public BMDFile(string xmlPath)
        {
            ConvertFromXml(xmlPath);
        }

        internal BMDFile(BinaryReader reader)
        {
            InternalRead(reader);
        }

        // Properties
        public int MessageCount
        {
            get { return _messages.Length; }
        }

        public BMDMessage[] Messages
        {
            get
            {
                return _messages;
            }
            private set
            {
                _messages = value;
            }
        }

        public int ActorCount
        {
            get { return _actorNames.Length; }
        }

        public string[] ActorNames
        {
            get { return _actorNames; }
        }

        // Public Static Methods
        public static BMDFile LoadFrom(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                return new BMDFile(reader);
        }

        public static BMDFile LoadFrom(Stream stream, bool leaveStreamOpen)
        {
            using (BinaryReader reader = new BinaryReader(stream, Encoding.Default, leaveStreamOpen))
                return new BMDFile(reader);
        }

        // Methods
        public void SaveXml(string path)
        {
            XDocument xDoc = new XDocument();
            XElement xRoot = new XElement(TAG);

            foreach (BMDMessage msg in Messages)
            {
                XElement msgElement = msg.ConvertToXmlElement(_actorNames);
                xRoot.Add(msgElement);
            }

            xDoc.Add(xRoot);
            xDoc.Save(path);
        }

        internal override void InternalWrite(BinaryWriter writer)
        {
            int posFileStart = (int)writer.BaseStream.Position;
            List<int> addressList = new List<int>();

            // Seek past chunk and msg header for writing later
            writer.BaseStream.Seek(DATA_START_ADDRESS, SeekOrigin.Current);

            // Write a dummy message pointer table
            for (int i = 0; i < MessageCount; i++)
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

            BMDMessageTable[] messagePointerTable = new BMDMessageTable[MessageCount];

            // Write the messages and fill in the message pointer table
            for (int i = 0; i < MessageCount; i++)
            {
                writer.AlignPosition(4);
                messagePointerTable[i].Offset = (int)writer.BaseStream.Position - DATA_START_ADDRESS - posFileStart;
                _messages[i].InternalWrite(writer, ref addressList, posFileStart);
            }

            writer.AlignPosition(4);
            int actorNamePointerTableOffset = (int)writer.BaseStream.Position - DATA_START_ADDRESS - posFileStart;

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
                    actorNamePointerTable[i] = (int)writer.BaseStream.Position - DATA_START_ADDRESS - posFileStart;
                    writer.WriteCString(_actorNames[i]);
                }

                long addresRelocPosition = writer.BaseStream.Position;

                writer.BaseStream.Seek(actorNamePointerTablePosition, SeekOrigin.Begin);
                writer.Write(actorNamePointerTable);

                writer.BaseStream.Seek(addresRelocPosition, SeekOrigin.Begin);
            }

            // Compress and write the address relocationt able
            byte[] addressRelocTable = AddressRelocationTableCompression.Compress(addressList, DATA_START_ADDRESS);
            int addressRelocTableOffset = (int)writer.BaseStream.Position - posFileStart;
            int addressRelocTableSize = addressRelocTable.Length;
            writer.Write(addressRelocTable);

            // Save the end offset for calculating length and seeking later
            long posFileEnd = writer.BaseStream.Position;
            int length = (int)(posFileEnd - posFileStart);

            // Seek back to the start
            writer.BaseStream.Seek(posFileStart, SeekOrigin.Begin);

            // Write Chunk header
            writer.Write(FLAG);
            writer.Write((short)0); // userID
            writer.Write(length);
            writer.WriteCString(TAG, 4);
            writer.Write(0);

            // Write MSG header
            writer.Write(addressRelocTableOffset);
            writer.Write(addressRelocTableSize);
            writer.Write(MessageCount);
            writer.Write(UNK_CONSTANT);

            for (int i = 0; i < MessageCount; i++)
            {
                writer.Write((int)messagePointerTable[i].Type);
                writer.Write(messagePointerTable[i].Offset);
            }

            writer.Write(actorNamePointerTableOffset);

            writer.BaseStream.Seek(posFileEnd, SeekOrigin.Begin);
        }

        private void ConvertFromXml(string path)
        {
            XDocument xDoc = XDocument.Load(path);
            XElement xRoot = xDoc.Root;

            if (xRoot.Name != TAG)
            {
                throw new InvalidDataException($"Root element name = {xRoot.Name}. Expected: {TAG}");
            }

            XElement[] messageElements = xRoot.Elements().ToArray();

            int numMessages = messageElements.Length;

            BMDMessageTable[] messagePointerTable = new BMDMessageTable[numMessages];
            _messages = new BMDMessage[numMessages];

            List<string> actorNameList = new List<string>();
            for (int i = 0; i < numMessages; i++)
            {
                // Get the type
                messagePointerTable[i].Type = (MessageType)Enum.Parse(typeof(MessageType), messageElements[i].Attribute("Type").Value);

                switch (messagePointerTable[i].Type)
                {
                    case MessageType.Regular:
                        {
                            // Get the actor name
                            string actorName = messageElements[i].Attribute("Actor").Value;

                            if (actorName == "None")
                            {
                                // If the actor name is set to None, set the index to -1
                                messageElements[i].SetAttributeValue("Actor", -1);
                            }
                            else if (!actorNameList.Contains(actorName))
                            {
                                // If the name of the actor is not in the list, add it
                                actorNameList.Add(actorName);

                                // Set the index to the index of the last added element 
                                messageElements[i].SetAttributeValue("Actor", actorNameList.Count - 1);
                            }
                            else
                            {
                                // Retrieve the index from the list
                                messageElements[i].SetAttributeValue("Actor", actorNameList.IndexOf(actorName));
                            }

                            break;
                        }


                }
            }

            _actorNames = actorNameList.ToArray();

            for (int i = 0; i < numMessages; i++)
            {
                _messages[i] = BMDMessageFactory.GetMessage(messageElements[i], messagePointerTable[i].Type);
            }
        }

        private void InternalRead(BinaryReader reader)
        {
            long posFileStart = reader.GetPosition();
            short flag = reader.ReadInt16();
            short userID = reader.ReadInt16();
            int length = reader.ReadInt32();
            string tag = reader.ReadCString(4);
            reader.AlignPosition(16);

            if (tag != TAG)
            {
                throw new InvalidDataException();
            }

            int addressRelocTableOffset = reader.ReadInt32();
            int addressRelocTableSize = reader.ReadInt32();
            int numMessages = reader.ReadInt32();
            int unk0x1C = reader.ReadInt32();

            if (unk0x1C != UNK_CONSTANT)
            {
                Console.WriteLine("_unk0x1C isn't 0x20000");
            }

            BMDMessageTable[] messagePointerTable = new BMDMessageTable[numMessages];
            for (int i = 0; i < messagePointerTable.Length; i++)
            {
                messagePointerTable[i].Type = (MessageType)reader.ReadInt32();
                messagePointerTable[i].Offset = reader.ReadInt32();
            }

            int actorNamePointerTableOffset = reader.ReadInt32();
            int numActors = reader.ReadInt32();

            reader.BaseStream.Seek(posFileStart + DATA_START_ADDRESS + actorNamePointerTableOffset, SeekOrigin.Begin);
            int[] actorNamePointerTable = reader.ReadInt32Array(numActors);

            _actorNames = new string[numActors];
            for (int i = 0; i < _actorNames.Length; i++)
            {
                reader.BaseStream.Seek(posFileStart + DATA_START_ADDRESS + actorNamePointerTable[i], SeekOrigin.Begin);
                _actorNames[i] = reader.ReadCString();
            }

            _messages = new BMDMessage[numMessages];
            for (int i = 0; i < _messages.Length; i++)
            {
                _messages[i] = BMDMessageFactory.GetMessage(reader, (int)posFileStart, messagePointerTable[i]);
            }
        }
    }

    struct BMDMessageTable
    {
        public MessageType Type;
        public int Offset;
    }

    public enum MessageType : int
    {
        Regular = 0,
        Selection = 1
    }
}
