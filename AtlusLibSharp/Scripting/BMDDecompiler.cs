using System.Collections.Generic;
using System.IO;

namespace AtlusLibSharp.Scripting
{
    public static class BMDDecompiler
    {
        private delegate string DecompileFunc(BMDFunctionToken func);

        private static Dictionary<BMDMessageType, string> _msgTypeToKeyword = new Dictionary<BMDMessageType, string>()
        {
            { BMDMessageType.Standard, "messagebox" },
            { BMDMessageType.Selection, "selection" }
        };

        private static Dictionary<BMDMessageType, string> _msgTypeToPageKeyword = new Dictionary<BMDMessageType, string>()
        {
            { BMDMessageType.Standard, "dialog" },
            { BMDMessageType.Selection, "choice" }
        };

        private static Dictionary<uint, DecompileFunc> _msgFuncToDecDelegateP4 = new Dictionary<uint, DecompileFunc>()
        {
            { 1 << 16 | 1, DecFuncWait },
            { 4 << 16 | 2, DecFuncPname },
            { 4 << 16 | 6, DecFuncBustup },
            { 3 << 16 | 1, DecFuncVplay },
        };

        public static void Decompile(BMDFile bmd, string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (BMDMessage dlg in bmd.Messages)
                {
                    DecompileMessage(writer, dlg);
                }
            }
        }

        private static void DecompileMessage(StreamWriter writer, BMDMessage msg)
        {
            writer.WriteLine("\n{0} {1}", _msgTypeToKeyword[msg.MessageType], msg.Name);

            foreach (BMDDialog dlg in msg.Dialogs)
            {
                writer.Write("\n[{0}]\n", _msgTypeToPageKeyword[msg.MessageType]);
                DecompileDialog(writer, dlg);
                writer.Write("\n[end]\n");
            }
        }

        private static void DecompileDialog(StreamWriter writer, BMDDialog dlg)
        {
            bool lastLineWasNoNewlineFunc = false;
            bool lastLineWasText = false;

            foreach (BMDDialogToken token in dlg.Tokens)
            {
                switch (token.Type)
                {
                    case BMDDialogTokenType.Text:
                        {
                            if (!lastLineWasText)
                            {
                                lastLineWasText = true;
                            }

                            writer.Write(token.ToString());
                        }
                        break;
                    case BMDDialogTokenType.Function:
                        {
                            string funcName = GetFuncString((BMDFunctionToken)token);

                            if (lastLineWasText)
                            {
                                writer.Write("<{0}>", funcName);
                                lastLineWasText = false;
                                lastLineWasNoNewlineFunc = true;
                            }
                            else
                            {
                                if (lastLineWasNoNewlineFunc)
                                {
                                    writer.WriteLine("\n" + "<{0}>", funcName);
                                    lastLineWasNoNewlineFunc = false;
                                }
                                else
                                {
                                    writer.WriteLine("<{0}>", funcName);
                                }
                            }
                        }
                        break;

                    default:
                        throw new InvalidDataException("Unexpected page token type!");
                }
            }
        }
        
        private static string GetFuncString(BMDFunctionToken func)
        {
            /*
            DecompileFunc decFunc;
            if (!_msgFuncToDecDelegateP4.TryGetValue((uint)(func.FunctionCategory << 16 | func.FunctionID), out decFunc))
            {
                return func.ToString();
            }
            else
            {
                return decFunc(func);
            }
            */
            return func.ToString();
        }

        private static string DecFuncWait(BMDFunctionToken func)
        {
            return "wait";
        }

        private static string DecFuncPname(BMDFunctionToken func)
        {
            return "pname";
        }

        private static string DecFuncBustup(BMDFunctionToken func)
        {
            return string.Format("bup {0} {1} {2}", func.Parameters[0]-1, func.Parameters[2]-1, func.Parameters[6]-1);
        }

        private static string DecFuncVplay(BMDFunctionToken func)
        {
            return string.Format("vplay {0} {1} {2} {4}", func.Parameters[0], func.Parameters[1], func.Parameters[6], func.Parameters[7]);
        }
    }
}
