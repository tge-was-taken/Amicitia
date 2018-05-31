namespace AmicitiaLibrary.Scripting
{
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;

    public class BmdDialog
    {
        private List<BmdDialogToken> mDialogTokens;

        public List<BmdDialogToken> Tokens
        {
            get { return mDialogTokens; }
        }

        public BmdDialog(IList<BmdDialogToken> tokens)
        {
            mDialogTokens = tokens.ToList();
        }

        internal BmdDialog(BinaryReader reader)
        {
            mDialogTokens = new List<BmdDialogToken>();
            InternalRead(reader);
        }

        internal BmdDialog()
        {
            mDialogTokens = new List<BmdDialogToken>();
        }

        internal void InternalWrite(BinaryWriter writer)
        {
            for (int i = 0; i < mDialogTokens.Count; i++)
            {
                mDialogTokens[i].InternalWrite(writer);
            }

            writer.Write((byte)0);
        }

        private void InternalRead(BinaryReader reader)
        {
            while (true)
            {
                BmdDialogToken token;

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
                    byte funcId = (byte)(b2 & 0x1F);

                    token = new BmdFunctionToken(funcCategory, funcId, reader.ReadBytes(numParams));
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

                    token = new BmdTextToken(textBytes.ToArray());
                }

                mDialogTokens.Add(token);
            }
        }
    }
}
