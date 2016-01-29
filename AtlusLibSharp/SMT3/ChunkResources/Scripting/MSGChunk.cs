namespace AtlusLibSharp.SMT3.ChunkResources.Scripting
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using System.Text;

    using Utilities;

    public class MSGChunk : Chunk
    {
        // Constants
        internal const int FLAG = 0x0007;
        internal const string TAG = "MSG1";
        internal const int DATA_START_ADDRESS = 0x20;

        // Private Fields
        private int _addressRelocTableOffset;
        private int _addressRelocTableSize;
        private int _numMessages;
        private int _unk0x1C = 0x20000;
        private MessageTable[] _messagePointerTable;
        private MSGMessage[] _messages;
        private int _actorNamePointerTableOffset;
        private int _numActors;
        private int[] _actorNamePointerTable;
        private string[] _actorNames;

        // Constructors
        public MSGChunk(string xmlPath)
            : base(FLAG, 0, 0, TAG)
        {
            ConvertFromXml(xmlPath);
        }

        internal MSGChunk(ushort id, int length, BinaryReader reader)
            : base(FLAG, id, length, TAG)
        {
            Read(reader);
        }

        // Properties
        public int MessageCount
        {
            get { return _numMessages; }
        }

        public MSGMessage[] Messages
        {
            get
            {
                return _messages;
            }
            private set
            {
                _messages = value;
                _numMessages = _messages.Length;
            }
        }

        // Methods
        public void SaveXml(string path)
        {
            XDocument xDoc = new XDocument();
            XElement xRoot = new XElement(TAG);

            foreach (MSGMessage msg in Messages)
            {
                XElement msgElement = msg.ConvertToXmlElement(_actorNames);
                xRoot.Add(msgElement);
            }

            xDoc.Add(xRoot);
            xDoc.Save(path);
        }

        internal override void InternalWrite(BinaryWriter writer)
        {
            int fp = (int)writer.BaseStream.Position;
            List<int> addressList = new List<int>();

            // Seek past chunk and msg header for writing later
            writer.BaseStream.Seek(DATA_START_ADDRESS, SeekOrigin.Current);

            // Write a dummy message pointer table
            for (int i = 0; i < _numMessages; i++)
            {
                writer.Write(0);
                addressList.Add((int)writer.BaseStream.Position - fp);
                writer.Write(0);
            }

            // Write a dummy offset for writing later
            addressList.Add((int)writer.BaseStream.Position - fp);
            writer.Write(0);
            writer.Write(_numActors);

            // These are always here for some reason
            writer.Write(0);
            writer.Write(0);

            writer.AlignPosition(4);

            // Write the messages and fill in the message pointer table
            for (int i = 0; i < _numMessages; i++)
            {
                writer.AlignPosition(4);
                _messagePointerTable[i].Offset = (int)writer.BaseStream.Position - DATA_START_ADDRESS - fp;
                _messages[i].Write(writer, ref addressList, fp);
            }

            writer.AlignPosition(4);
            _actorNamePointerTableOffset = (int)writer.BaseStream.Position - DATA_START_ADDRESS - fp;

            // Write dummy actor name pointer table if there are actors present
            if (_numActors > 0)
            {
                long actorNamePointerTablePosition = writer.BaseStream.Position;
                for (int i = 0; i < _numActors; i++)
                {
                    addressList.Add((int)writer.BaseStream.Position - fp);
                    writer.Write(0);
                }

                _actorNamePointerTable = new int[_numActors];
                for (int i = 0; i < _numActors; i++)
                {
                    _actorNamePointerTable[i] = (int)writer.BaseStream.Position - DATA_START_ADDRESS - fp;
                    writer.WriteCString(_actorNames[i]);
                }

                long addresRelocPosition = writer.BaseStream.Position;

                writer.BaseStream.Seek(actorNamePointerTablePosition, SeekOrigin.Begin);
                writer.Write(_actorNamePointerTable);

                writer.BaseStream.Seek(addresRelocPosition, SeekOrigin.Begin);
            }

            // Compress and write the address relocationt able
            byte[] addressRelocTable = AddressRelocationTableCompression.Compress(addressList, DATA_START_ADDRESS);
            _addressRelocTableOffset = (int)writer.BaseStream.Position - fp;
            _addressRelocTableSize = addressRelocTable.Length;
            writer.Write(addressRelocTable);

            // Save the end offset for calculating length and seeking later
            long endOffset = writer.BaseStream.Position;
            Length = (int)(endOffset - fp);

            // Seek back to the start
            writer.BaseStream.Seek(fp, SeekOrigin.Begin);

            // Write Chunk header
            writer.Write(Flags);
            writer.Write(UserID);
            writer.Write(Length);
            writer.WriteCString(Tag, 4);
            writer.Write(0);

            // Write MSG header
            writer.Write(_addressRelocTableOffset);
            writer.Write(_addressRelocTableSize);
            writer.Write(_numMessages);
            writer.Write(_unk0x1C);

            for (int i = 0; i < _numMessages; i++)
            {
                writer.Write(_messagePointerTable[i].Type);
                writer.Write(_messagePointerTable[i].Offset);
            }

            writer.Write(_actorNamePointerTableOffset);

            writer.BaseStream.Seek(endOffset, SeekOrigin.Begin);
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

            _numMessages = messageElements.Length;

            _messagePointerTable = new MessageTable[_numMessages];
            _messages = new MSGMessage[_numMessages];

            List<string> actorNameList = new List<string>();
            for (int i = 0; i < _numMessages; i++)
            {
                // Get the type
                _messagePointerTable[i].Type = int.Parse(messageElements[i].Attribute("Type").Value);

                switch (_messagePointerTable[i].Type)
                {
                    case 0:
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
            _numActors = _actorNames.Length;

            for (int i = 0; i < _numMessages; i++)
            {
                _messages[i] = MSGMessageFactory.GetMessage(messageElements[i], _messagePointerTable[i].Type);
            }
        }

        private void Read(BinaryReader reader)
        {
            int fp = (int)reader.BaseStream.Position - HEADER_SIZE;
            reader.AlignPosition(16);
            _addressRelocTableOffset = reader.ReadInt32();
            _addressRelocTableSize = reader.ReadInt32();
            _numMessages = reader.ReadInt32();
            _unk0x1C = reader.ReadInt32();

            if (_unk0x1C != 0x20000)
            {
                Console.WriteLine("_unk0x1C isn't 0x20000");
            }

            _messagePointerTable = new MessageTable[_numMessages];
            for (int i = 0; i < _numMessages; i++)
            {
                _messagePointerTable[i].Type = reader.ReadInt32();
                _messagePointerTable[i].Offset = reader.ReadInt32();
            }

            _actorNamePointerTableOffset = reader.ReadInt32();
            _numActors = reader.ReadInt32();

            reader.BaseStream.Seek(fp + DATA_START_ADDRESS + _actorNamePointerTableOffset, SeekOrigin.Begin);
            _actorNamePointerTable = reader.ReadInt32Array(_numActors);

            _actorNames = new string[_numActors];
            for (int i = 0; i < _numActors; i++)
            {
                reader.BaseStream.Seek(fp + DATA_START_ADDRESS + _actorNamePointerTable[i], SeekOrigin.Begin);
                _actorNames[i] = reader.ReadCString();
            }

            _messages = new MSGMessage[_numMessages];
            for (int i = 0; i < _numMessages; i++)
            {
                _messages[i] = MSGMessageFactory.GetMessage(reader, fp, _messagePointerTable[i]);
            }
        }
    }

    struct MessageTable
    {
        public int Type;
        public int Offset;
    }
}
