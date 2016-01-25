namespace AtlusLibSharp.SMT3.ChunkResources.Scripting
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using System.Text;

    public class MSGDialog
    {
        internal List<MSGDialogToken> dialogTokens;

        internal MSGDialog(BinaryReader reader)
        {
            dialogTokens = new List<MSGDialogToken>();
            Read(reader);
        }

        internal MSGDialog()
        {
            dialogTokens = new List<MSGDialogToken>();
        }

        internal XElement ConvertToXmlElement()
        {
            XElement dlgElement = new XElement("Dialog");

            foreach (MSGDialogToken dlgToken in dialogTokens)
            {
                string type = Enum.IsDefined(typeof(MSGDialogTokenType), dlgToken.Type) ? dlgToken.Type.ToString() : ((byte)dlgToken.Type).ToString("X"); 

                XElement tokenElement =
                    new XElement("Token",
                    new XAttribute("Type", type));

                if (dlgToken.Type == MSGDialogTokenType.Text)
                {
                    string str = "\n" + Encoding.GetEncoding("Shift_JIS").GetString(dlgToken.Data);
                    tokenElement.Add(new XText(str));
                }
                else
                {
                    for (int i = 0; i < dlgToken.Data.Length; i++)
                    {
                        tokenElement.Add(new XAttribute("Byte" + i, dlgToken.Data[i]));
                    }
                }
                dlgElement.Add(tokenElement);
            }

            return dlgElement;
        }

        internal void Write(BinaryWriter writer)
        {
            for (int i = 0; i < dialogTokens.Count; i++)
            {
                if (dialogTokens[i].Type != MSGDialogTokenType.Text)
                {
                    writer.Write((byte)dialogTokens[i].Type);
                }

                writer.Write(dialogTokens[i].Data);

                if (i == dialogTokens.Count - 1)
                {
                    writer.Write((byte)0);
                }
            }
        }

        private void Read(BinaryReader reader)
        {
            while (true)
            {
                MSGDialogToken token = new MSGDialogToken();

                byte b = reader.ReadByte();

                if (b == 0x00)
                {
                    break;
                }

                // Check if it is a command
                // Also check if the command isn't possibly a japanese character by
                // checking if the number of bytes isn't too high
                if ((b & 0xF0) == 0xF0 && (b & 0x0F) < 0xB)
                {
                    int numBytes = 1;
                    if ((b & 0x0F) > 1)
                    {
                        numBytes += (2 * ((b & 0x0F) - 1));
                    }

                    token = new MSGDialogToken
                    {
                        Type = (MSGDialogTokenType)b,
                        Data = reader.ReadBytes(numBytes)
                    };
                }
                else
                {
                    List<byte> textBytes = new List<byte>();
                    textBytes.Add(b);

                    while (true)
                    {
                        byte c = reader.ReadByte();

                        if ((c & 0xF0) == 0xF0 || c == 0x00)
                        {
                            reader.BaseStream.Seek(-1, SeekOrigin.Current);
                            break;
                        }
                        else
                        {
                            textBytes.Add(c);
                        }
                    }

                    token = new MSGDialogToken
                    {
                        Type = MSGDialogTokenType.Text,
                        Data = textBytes.ToArray()
                    };
                }

                dialogTokens.Add(token);

                if ((byte)token.Type == 0xF1 && token.Data[0] == 0x21)
                {
                    break;
                }
            }
        }
    }
}
