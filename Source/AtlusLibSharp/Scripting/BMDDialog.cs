namespace AtlusLibSharp.Scripting
{
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;

    public class BMDDialog
    {
        private List<BMDDialogToken> _dialogTokens;

        public List<BMDDialogToken> Tokens
        {
            get { return _dialogTokens; }
        }

        public BMDDialog(IList<BMDDialogToken> tokens)
        {
            _dialogTokens = tokens.ToList();
        }

        internal BMDDialog(BinaryReader reader)
        {
            _dialogTokens = new List<BMDDialogToken>();
            InternalRead(reader);
        }

        internal BMDDialog()
        {
            _dialogTokens = new List<BMDDialogToken>();
        }

        internal void InternalWrite(BinaryWriter writer)
        {
            for (int i = 0; i < _dialogTokens.Count; i++)
            {
                _dialogTokens[i].InternalWrite(writer);
            }

            writer.Write((byte)0);
        }

        private void InternalRead(BinaryReader reader)
        {
            while (true)
            {
                BMDDialogToken token;

                byte b = reader.ReadByte();

                if (b == 0x00)
                {
                    break;
                }

                // Check if it is a command
                if ((b & 0xF0) == 0xF0)
                {
                    int numParams = ((b & 0x0F) - 1) << 1;
                    byte b2 = reader.ReadByte();
                    byte funcCategory = (byte)((b2 & 0xE0) >> 5);
                    byte funcID = (byte)(b2 & 0x1F);

                    token = new BMDFunctionToken(funcCategory, funcID, reader.ReadBytes(numParams));
                }
                else
                {
                    List<byte> textBytes = new List<byte>();

                    while (true)
                    {
                        if ((b & 0xF0) == 0xF0 || b == 0x00)
                        {
                            reader.BaseStream.Seek(-1, SeekOrigin.Current);
                            break;
                        }
                        else if ((b & 0x80) == 0x80) // japanese extended encoding
                        {
                            textBytes.Add(b);
                            textBytes.Add(reader.ReadByte());
                        }
                        else
                        {
                            textBytes.Add(b);
                        }

                        b = reader.ReadByte();
                    }

                    token = new BMDTextToken(textBytes.ToArray());
                }

                _dialogTokens.Add(token);
            }
        }
    }
}
