using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace AtlusLibSharp.Scripting
{
    public static class BmdScriptParser
    {
        internal static int CurLineIndex = -1;
        internal static string CurLine;
        internal static long PrevLineOffset;
        internal static long CurLineOffset;
        internal static List<string> CurStringBuffer = new List<string>();
        internal static StreamReader Reader;
        internal static List<string> ActorNameList = new List<string>();

        public static BmdFile CompileScript(string path)
        {
            BmdFile bmd = new BmdFile();

            List<BmdMessage> msgs = new List<BmdMessage>();
            using (Reader = new StreamReader(path))
            {
                while (!Reader.EndOfStream)
                {
                    ReadLine();

                    string[] tokens = CurLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (tokens.Length == 0)
                        continue;

                    if (tokens[0] == "messagebox" || tokens[0] == "selection")
                        msgs.Add(ParseMsg());
                }
            }

            bmd.ActorNames = ActorNameList.ToArray();
            bmd.Messages = msgs.ToArray();

            return bmd;
        }

        private static void ReadLine()
        {
            if (Reader.Peek() == -1)
                throw new BmdScriptParserException("read past end of file");

            PrevLineOffset = Reader.BaseStream.Position;
            CurLine = Reader.ReadLine();
            CurLineOffset = Reader.BaseStream.Position;
            CurLineIndex++;
        }

        private static void GotoPrevLine()
        {
            if (PrevLineOffset == -1)
                throw new BmdScriptParserException("can't go back multiple lines");

            Reader.BaseStream.Position = PrevLineOffset;
            CurLineOffset = PrevLineOffset;
            PrevLineOffset = -1;
            CurLine = Reader.ReadLine();
        }

        private static void ReadLines(string terminator)
        {
            CurStringBuffer.Clear();

            while (true)
            {
                ReadLine();

                if (CurLine.Contains(terminator))
                    break;

                CurStringBuffer.Add(CurLine);

                if (Reader.Peek() != -1)
                {
                    ReadLine();
                }
                else
                {
                    throw new BmdScriptParserException("cant read line");
                }
            }
        }

        private static BmdMessage ParseMsg()
        {
            string[] tokens = CurLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 0)
                throw new BmdScriptParserException("messagebox or selection keyword");

            if (tokens.Length == 1)
                throw new BmdScriptParserException("name");

            if (tokens[0] == "messagebox")
            {
                return ParseStdMsg(tokens[1]);
            }
            else if (tokens[0] == "selection")
            {
                return ParseSelMsg(tokens[1]);
            }
            else
            {
                throw new BmdScriptParserException("messagebox or selection keyword", tokens[0]);
            }
        }

        private static BmdSelectionMessage ParseSelMsg(string name)
        {
            List<BmdDialog> dlgList = new List<BmdDialog>();

            while (true)
            {
                ReadLine();
                string[] tokens = CurLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length == 0)
                    continue;

                if (tokens[0] == "messagebox" || tokens[0] == "selection")
                {
                    GotoPrevLine();
                    break;
                }
                else if (tokens[0] == "[choice]")
                {
                    dlgList.Add(ParseDlg());
                }
                else
                {
                    throw new BmdScriptParserException("keyword");
                }
            }

            BmdSelectionMessage msg = new BmdSelectionMessage()
            {
                Name = name,
                Dialogs = dlgList.ToArray()
            };

            return msg;
        }

        private static BmdStandardMessage ParseStdMsg(string name)
        {
            List<BmdDialog> dlgList = new List<BmdDialog>();
            string actorName = null;

            while (true)
            {
                ReadLine();
                string[] tokens = CurLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length == 0)
                    continue;

                if (tokens[0] == "messagebox" || tokens[0] == "selection")
                {
                    GotoPrevLine();
                    break;
                }
                else if (tokens[0] == "actor")
                {
                    if (tokens.Length != 2)
                        throw new BmdScriptParserException("actor name");

                    actorName = tokens[1];

                    if (!ActorNameList.Contains(actorName))
                        ActorNameList.Add(actorName);
                }
                else if (tokens[0] == "[dialog]")
                {
                    dlgList.Add(ParseDlg());
                }
                else
                {
                    throw new BmdScriptParserException("keyword");
                }
            }

            BmdStandardMessage msg = new BmdStandardMessage()
            {
                Name = name,
                ActorId = (short)ActorNameList.IndexOf(actorName),
                Dialogs = dlgList.ToArray()
            };

            return msg;
        }

        private static BmdDialog ParseDlg()
        {
            BmdDialog dlg = new BmdDialog();
            ReadLines("[end]");

            StringBuilder strBuilder = new StringBuilder();
            bool textWasRead = false;

            for (int i = 0; i < CurStringBuffer.Count; i++)
            {
                for (int j = 0; j < CurStringBuffer[i].Length; j++)
                {
                    if (CurStringBuffer[i][j] == '<')
                    {
                        if (textWasRead)
                        {
                            dlg.Tokens.Add(new BmdTextToken(strBuilder.ToString()));
                            strBuilder.Clear();
                            textWasRead = false;
                        }

                        int funcOpenIdx = j;
                        int funcCloseIdx = -1;
                        for (int k = j; k < CurStringBuffer[i].Length-j; k++)
                        {
                            if (CurStringBuffer[i][k] == '>')
                            {
                                funcCloseIdx = k;
                                j += (k - j);
                                break;
                            }
                        }

                        if (funcCloseIdx != -1)
                        {
                            string funcString = CurStringBuffer[i].Substring(funcOpenIdx + 1, funcCloseIdx - funcOpenIdx - 1);
                            dlg.Tokens.Add(ParseFunction(funcString));
                            j = funcCloseIdx;
                        }
                    }
                    else
                    {
                        if (!textWasRead)
                            textWasRead = true;

                        strBuilder.Append(CurStringBuffer[i][j]);
                    }
                }
            }

            return dlg;
        }

        private static BmdFunctionToken ParseFunction(string funcString)
        {
            string[] tokens = funcString.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 0)
                throw new BmdScriptParserException("function");

            string funcName = tokens[0];
            byte funcCategory;
            byte funcId;
            byte[] funcParams;

            if (funcName == "func")
            {
                if (tokens.Length < 3)
                    throw new BmdScriptParserException("function category & id");

                if (!byte.TryParse(tokens[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out funcCategory))
                    throw new BmdScriptParserException("function category");

                if (!byte.TryParse(tokens[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out funcId))
                    throw new BmdScriptParserException("function id");

                int numParams = tokens.Length - 3;
                funcParams = new byte[numParams];

                for (int i = 0; i < numParams; i++)
                {
                    byte param;

                    if (!byte.TryParse(tokens[3 + i], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out param))
                        throw new BmdScriptParserException("function argument");

                    funcParams[i] = param;
                }
            }
            else
            {
                throw new BmdScriptParserException("function");
            }

            return new BmdFunctionToken(funcCategory, funcId, funcParams);
        }
    }

    [Serializable]
    public class BmdScriptParserException : Exception
    {
        public BmdScriptParserException(object expected, object got)
            : base($"Error at line {BmdScriptParser.CurLineIndex}. Expected {expected}, got {got}")
        {
        }

        public BmdScriptParserException(object expected)
            : base($"Error at line {BmdScriptParser.CurLineIndex}. Expected {expected}")
        {
        }

        public BmdScriptParserException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }
    }
}
