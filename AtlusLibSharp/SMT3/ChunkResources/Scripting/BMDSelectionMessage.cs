namespace AtlusLibSharp.SMT3.ChunkResources.Scripting
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    using Utilities;

    public class BMDSelectionMessage : BMDMessage
    {
        private ushort _unk0x18;
        private ushort _numChoices;

        internal BMDSelectionMessage(BinaryReader reader, int fp)
        {
            Read(reader, fp);
        }

        internal BMDSelectionMessage(XElement xElement)
        {
            ParseXmlElement(xElement);
        }

        public override MessageType MessageType
        {
            get { return MessageType.Selection; }
        }

        protected internal override XElement ConvertToXmlElement(params object[] param)
        {
            XElement msgElement =
                new XElement("Message",
                new XAttribute("Type", "Selection"),
                new XAttribute("Name", _name));


            foreach (BMDDialog dlg in Dialogs)
            {
                XElement dlgElement = dlg.ConvertToXmlElement();
                msgElement.Add(dlgElement);
            }

            return msgElement;
        }

        protected internal override void InternalWrite(BinaryWriter writer, ref List<int> addressList, int fp)
        {
            // Write header fields
            writer.WriteCString(_name, MSG_MESSAGE_NAME_LENGTH);
            writer.Write(_unk0x18);
            writer.Write(_numChoices);
            
            // unknown zero
            writer.Write(0);

            // Save the pointer table offset for writing later
            long dialogPointerTableOffset = writer.BaseStream.Position;

            // dummy dialog pointer table
            for (int i = 0; i < _numChoices; i++)
            {
                addressList.Add((int)writer.BaseStream.Position - fp);
                writer.Write(0);
            }

            // dummy length
            writer.Write(0);

            _dialogPointerTable = new int[_numChoices];

            // Write the dialogs
            long dialogDataStart = writer.BaseStream.Position;
            for (int i = 0; i < _numChoices; i++)
            {
                _dialogPointerTable[i] = (int)writer.BaseStream.Position - BMDFile.DATA_START_ADDRESS - fp;
                _dialogs[i].Write(writer);
            }

            long dialogDataEnd = writer.BaseStream.Position;

            // Calculate the dialog data length
            _dialogDataLength = (int)(dialogDataEnd - dialogDataStart);

            // Seek to the pointer table and write it
            writer.BaseStream.Seek(dialogPointerTableOffset, SeekOrigin.Begin);
            writer.Write(_dialogPointerTable);
            writer.Write(_dialogDataLength);

            // Seek back to the end of the data
            writer.BaseStream.Seek(dialogDataEnd, SeekOrigin.Begin);
        }

        private void ParseXmlElement(XElement xElement)
        {
            XElement[] dialogElements = xElement.Elements().ToArray();

            _name = xElement.Attribute("Name").Value;
            _unk0x18 = 0;
            _numChoices = (ushort)dialogElements.Length;
            _dialogs = new BMDDialog[_numChoices];

            for (int i = 0; i < _numChoices; i++)
            {
                XElement[] tokenElements = dialogElements[i].Elements().ToArray();
                _dialogs[i] = new BMDDialog();

                for (int j = 0; j < tokenElements.Length; j++)
                {
                    BMDDialogTokenType type;
                    bool hasNamedType = Enum.TryParse(tokenElements[j].Attribute("Type").Value, out type);

                    if (!hasNamedType)
                    {
                        type = (BMDDialogTokenType)int.Parse(tokenElements[j].Attribute("Type").Value, System.Globalization.NumberStyles.HexNumber);
                    }

                    BMDDialogToken token =
                        new BMDDialogToken
                        {
                            Type = type
                        };

                    if (token.Type == BMDDialogTokenType.Text)
                    {
                        if (tokenElements[j].FirstNode != null)
                        {
                            string str = (tokenElements[j].FirstNode as XText).Value.TrimStart('\n').Trim('\0');
                            token.Data = Encoding.ASCII.GetBytes(str);
                        }
                        else
                        {
                            token.Data = new byte[0];
                        }
                    }
                    else
                    {
                        XAttribute[] attribs = tokenElements[j].Attributes().ToArray();

                        token.Data = new byte[attribs.Length - 1];
                        for (int k = 1; k < attribs.Length; k++)
                        {
                            token.Data[k - 1] = byte.Parse(attribs[k].Value);
                        }
                    }

                    _dialogs[i].dialogTokens.Add(token);
                }
            }
        }

        private void Read(BinaryReader reader, int fp)
        {
            _name = reader.ReadCString(MSG_MESSAGE_NAME_LENGTH);
            _unk0x18 = reader.ReadUInt16();
            _numChoices = reader.ReadUInt16();

            if (_unk0x18 != 0)
            {
                throw new NotImplementedException("MSGChoiceMessage.Read(): _unk0x18 is not zero!");
            }

            // unknown zero 4 bytesz
            reader.BaseStream.Seek(4, SeekOrigin.Current);

            _dialogPointerTable = reader.ReadInt32Array(_numChoices);
            _dialogDataLength = reader.ReadInt32();

            _dialogs = new BMDDialog[_numChoices];
            for (int i = 0; i < _numChoices; i++)
            {
                reader.BaseStream.Seek(fp + BMDFile.DATA_START_ADDRESS + _dialogPointerTable[i], SeekOrigin.Begin);
                _dialogs[i] = new BMDDialog(reader);
            }
        }
    }
}
