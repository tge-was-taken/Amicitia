using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace AtlusLibSharp.Scripting
{
    public static class BMDScriptParser
    {
        internal static int curLineIndex = -1;
        internal static string curLine;
        internal static long prevLineOffset;
        internal static long curLineOffset;
        internal static List<string> curStringBuffer = new List<string>();
        internal static StreamReader reader;
        internal static List<string> actorNameList = new List<string>();

        public static BMDFile CompileScript(string path)
        {
            BMDFile bmd = new BMDFile();

            List<BMDMessage> msgs = new List<BMDMessage>();
            using (reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    ReadLine();

                    string[] tokens = curLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (tokens.Length == 0)
                        continue;

                    if (tokens[0] == "messagebox" || tokens[0] == "selection")
                        msgs.Add(ParseMsg());
                }
            }

            bmd.ActorNames = actorNameList.ToArray();
            bmd.Messages = msgs.ToArray();

            return bmd;
        }

        private static void ReadLine()
        {
            if (reader.Peek() == -1)
                throw new BMDScriptParserException("read past end of file");

            prevLineOffset = reader.BaseStream.Position;
            curLine = reader.ReadLine();
            curLineOffset = reader.BaseStream.Position;
            curLineIndex++;
        }

        private static void GotoPrevLine()
        {
            if (prevLineOffset == -1)
                throw new BMDScriptParserException("can't go back multiple lines");

            reader.BaseStream.Position = prevLineOffset;
            curLineOffset = prevLineOffset;
            prevLineOffset = -1;
            curLine = reader.ReadLine();
        }

        private static void ReadLines(string terminator)
        {
            curStringBuffer.Clear();

            while (true)
            {
                ReadLine();

                if (curLine.Contains(terminator))
                    break;

                curStringBuffer.Add(curLine);

                if (reader.Peek() != -1)
                {
                    ReadLine();
                }
                else
                {
                    throw new BMDScriptParserException("cant read line");
                }
            }
        }

        private static BMDMessage ParseMsg()
        {
            string[] tokens = curLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 0)
                throw new BMDScriptParserException("messagebox or selection keyword");

            if (tokens.Length == 1)
                throw new BMDScriptParserException("name");

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
                throw new BMDScriptParserException("messagebox or selection keyword", tokens[0]);
            }
        }

        private static BMDSelectionMessage ParseSelMsg(string name)
        {
            List<BMDDialog> dlgList = new List<BMDDialog>();

            while (true)
            {
                ReadLine();
                string[] tokens = curLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

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
                    throw new BMDScriptParserException("keyword");
                }
            }

            BMDSelectionMessage msg = new BMDSelectionMessage()
            {
                Name = name,
                Dialogs = dlgList.ToArray()
            };

            return msg;
        }

        private static BMDStandardMessage ParseStdMsg(string name)
        {
            List<BMDDialog> dlgList = new List<BMDDialog>();
            string actorName = null;

            while (true)
            {
                ReadLine();
                string[] tokens = curLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

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
                        throw new BMDScriptParserException("actor name");

                    actorName = tokens[1];

                    if (!actorNameList.Contains(actorName))
                        actorNameList.Add(actorName);
                }
                else if (tokens[0] == "[dialog]")
                {
                    dlgList.Add(ParseDlg());
                }
                else
                {
                    throw new BMDScriptParserException("keyword");
                }
            }

            BMDStandardMessage msg = new BMDStandardMessage()
            {
                Name = name,
                ActorID = (short)actorNameList.IndexOf(actorName),
                Dialogs = dlgList.ToArray()
            };

            return msg;
        }

        private static BMDDialog ParseDlg()
        {
            BMDDialog dlg = new BMDDialog();
            ReadLines("[end]");

            StringBuilder strBuilder = new StringBuilder();
            bool textWasRead = false;

            for (int i = 0; i < curStringBuffer.Count; i++)
            {
                for (int j = 0; j < curStringBuffer[i].Length; j++)
                {
                    if (curStringBuffer[i][j] == '<')
                    {
                        if (textWasRead)
                        {
                            dlg.Tokens.Add(new BMDTextToken(strBuilder.ToString()));
                            strBuilder.Clear();
                            textWasRead = false;
                        }

                        int funcOpenIdx = j;
                        int funcCloseIdx = -1;
                        for (int k = j; k < curStringBuffer[i].Length-j; k++)
                        {
                            if (curStringBuffer[i][k] == '>')
                            {
                                funcCloseIdx = k;
                                j += (k - j);
                                break;
                            }
                        }

                        if (funcCloseIdx != -1)
                        {
                            string funcString = curStringBuffer[i].Substring(funcOpenIdx + 1, funcCloseIdx - funcOpenIdx - 1);
                            dlg.Tokens.Add(ParseFunction(funcString));
                            j = funcCloseIdx;
                        }
                    }
                    else
                    {
                        if (!textWasRead)
                            textWasRead = true;

                        strBuilder.Append(curStringBuffer[i][j]);
                    }
                }
            }

            return dlg;
        }

        private static BMDFunctionToken ParseFunction(string funcString)
        {
            string[] tokens = funcString.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 0)
                throw new BMDScriptParserException("function");

            string funcName = tokens[0];
            byte funcCategory;
            byte funcID;
            byte[] funcParams;

            if (funcName == "func")
            {
                if (tokens.Length < 3)
                    throw new BMDScriptParserException("function category & id");

                if (!byte.TryParse(tokens[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out funcCategory))
                    throw new BMDScriptParserException("function category");

                if (!byte.TryParse(tokens[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out funcID))
                    throw new BMDScriptParserException("function id");

                int numParams = tokens.Length - 3;
                funcParams = new byte[numParams];

                for (int i = 0; i < numParams; i++)
                {
                    byte param;

                    if (!byte.TryParse(tokens[3 + i], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out param))
                        throw new BMDScriptParserException("function argument");

                    funcParams[i] = param;
                }
            }
            else
            {
                throw new BMDScriptParserException("function");
            }

            return new BMDFunctionToken(funcCategory, funcID, funcParams);
        }
    }

    [Serializable]
    public class BMDScriptParserException : Exception
    {
        public BMDScriptParserException(object expected, object got)
            : base(string.Format("Error at line {0}. Expected {1}, got {2}", BMDScriptParser.curLineIndex, expected, got))
        {
        }

        public BMDScriptParserException(object expected)
            : base(string.Format("Error at line {0}. Expected {1}", BMDScriptParser.curLineIndex, expected))
        {
        }

        public BMDScriptParserException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }
    }
}
