using AtlusLibSharp.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace AtlusLibSharp.Scripting
{
    internal static class BFAssembler
    {
        internal static Dictionary<string, BFInstruction> ASMKeywordToBFInstruction = BFDisassembler.BFInstructionToASMKeyword.Reverse();

        public static BFFile AssembleFromASMText(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                // keep track of various indices
                int lineIndex = -1;
                int procIndex = 0;
                int jumpIndex = 0;
                int opIndex = 0;

                // list for fixing up jump references later
                List<Tuple<string, int>> unresolvedJumps = new List<Tuple<string, int>>();
                List<Tuple<string, int>> unresolvedProcs = new List<Tuple<string, int>>();

                // code label and opcode lists
                List<BFCodeLabel> procedures = new List<BFCodeLabel>();
                List<BFCodeLabel> jumps = new List<BFCodeLabel>();
                List<BFOpcode> opcodes = new List<BFOpcode>();

                while (!reader.EndOfStream)
                {
                    lineIndex++;

                    // trim, clean, split line
                    string[] tokens = reader.ReadLine()
                        .Trim()
                        .Replace("\t", string.Empty)
                        .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    // skip if line contains no tokens or starts with a comment
                    if (tokens.Length == 0 || tokens[0] == ";")
                        continue;

                    if (tokens[0].EndsWith(":")) // we got a label
                    {
                        if (tokens[0].StartsWith("@")) // and the label is a jump label identifier
                        {
                            jumps.Add(ParseASMLabel(lineIndex, tokens[0], jumpIndex, opIndex, '@', ':'));
                            jumpIndex++;
                        }
                        else // the label is a procedure identifier
                        {
                            procedures.Add(ParseASMLabel(lineIndex, tokens[0], procIndex, opIndex, ':'));
                            procIndex++;
                        }
                    }
                    else // we got an instruction
                    {
                        // instruction without any procedures, throw an error
                        if (procedures.Count == 0)
                            throw new BFASMParserException("Instruction defined at line {0} before any procedures", lineIndex + 1);

                        BFInstruction instr;
                        BFOpcode op = null;
                        bool isDefined = ASMKeywordToBFInstruction.TryGetValue(tokens[0].ToLowerInvariant(), out instr);

                        if (!isDefined) // 'unk_XX' instruction expected
                        {
                            string opNumStr = tokens[0].Split(new string[] { "unk_" }, StringSplitOptions.RemoveEmptyEntries)[0];
                            int opNumInt;
                            long operand;

                            if (opNumStr.Length < 2 || !int.TryParse(opNumStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out opNumInt) ||
                                !long.TryParse(tokens[1], out operand))
                            {
                                throw new BFASMParserException("unk_XX", lineIndex + 1);
                            }

                            op = new BFOpcode((ushort)opNumInt, 0, operand);
                        }
                        else // regular instruction expected
                        {
                            // verify the number of arguments
                            VerifyArgCount(instr, tokens, lineIndex + 1);

                            switch (instr)
                            {
                                case BFInstruction.BeginProcedure:
                                case BFInstruction.CallNative:
                                case BFInstruction.CallProcedure:
                                case BFInstruction.Jump:
                                case BFInstruction.JumpIfFalse:
                                    string strArg = tokens[1];
                                    int intArg;

                                    if (int.TryParse(strArg, out intArg)) // reference by id
                                    {
                                        op = new BFOpcode(instr, intArg);
                                    }
                                    else // reference by name 
                                    {
                                        op = ParseInstrWithStringLiteralArgs(instr, strArg, lineIndex, opIndex, unresolvedProcs, unresolvedJumps);
                                    }
                                    break;

                                case BFInstruction.PushFloat:
                                    op = ParseInstrWithDecimalLiteralArgs(instr, tokens, lineIndex);
                                    break;

                                case BFInstruction.PushUInt16:
                                case BFInstruction.PushVariable:
                                case BFInstruction.SetVariable:
                                case BFInstruction.PushUInt32:
                                    op = ParseInstrWithNumberLiteralArgs(instr, tokens, lineIndex);
                                    break;

                                default:
                                    op = new BFOpcode(instr, null); // opcode takes no operands, eg add, subtract, return
                                    break;
                            }
                        }

                        // this shouldn't be possible
                        // but they said the same about the world being round
                        if (op == null)
                            throw new BFASMParserException(instr, lineIndex + 1);

                        opcodes.Add(op);
                        opIndex++;
                    }
                }

                // fix up unresolved jump/procedure references
                ResolveLabelReferences(unresolvedJumps, jumps, opcodes);
                ResolveLabelReferences(unresolvedProcs, procedures, opcodes);

                return new BFFile(procedures.ToArray(), jumps.ToArray(), opcodes);
            }
        }

        private static BFCodeLabel ParseASMLabel(int lineIndex, string nameToken, int jumpIndex, int opIndex, params char[] splitChars)
        {
            BFCodeLabel label = new BFCodeLabel()
            {
                Name = nameToken.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[0],
                ID = jumpIndex,
                OpcodeIndex = opIndex
            };

            if (label.Name.Length == 0)
                throw new BFASMParserException("Label", lineIndex + 1);

            return label;
        }

        private static void VerifyArgCount(BFInstruction instr, string[] tokens, int lineIndex)
        {
            switch (instr)
            {
                case BFInstruction.PushUInt32:
                case BFInstruction.PushFloat:
                case BFInstruction.BeginProcedure:
                case BFInstruction.CallNative:
                case BFInstruction.CallProcedure:
                case BFInstruction.Jump:
                case BFInstruction.JumpIfFalse:
                case BFInstruction.PushUInt16:
                case BFInstruction.PushVariable:
                case BFInstruction.SetVariable:
                    if (tokens.Length < 2)
                        throw new BFASMParserException(instr, lineIndex + 1);
                    break;
            }
        }

        private static BFOpcode ParseInstrWithNumberLiteralArgs(BFInstruction instr, string[] tokens, int lineIndex)
        {
            long value;

            if (!long.TryParse(tokens[1], out value))
                throw new BFASMParserException(instr, lineIndex + 1);

            return new BFOpcode(instr, value);
        }

        private static BFOpcode ParseInstrWithDecimalLiteralArgs(BFInstruction instr, string[] tokens, int lineIndex)
        {
            float value;

            if (!float.TryParse(tokens[1], out value))
                throw new BFASMParserException(instr, lineIndex + 1);

            return new BFOpcode(instr, value);
        }

        private static BFOpcode ParseInstrWithStringLiteralArgs(
            BFInstruction instr, string stringLiteral, int lineIndex, int opIndex,
            List<Tuple<string, int>> unresolvedProcs, List<Tuple<string, int>> unresolvedJumps)
        {
            switch (instr)
            {
                case BFInstruction.BeginProcedure:
                case BFInstruction.CallProcedure:
                    unresolvedProcs.Add(Tuple.Create(stringLiteral, opIndex));
                    break;

                case BFInstruction.CallNative:
                    BFCallNativeType callType;

                    if (!Enum.TryParse(stringLiteral, true, out callType))
                        throw new BFASMParserException(instr, lineIndex + 1);

                    return new BFOpcode(instr, (int)callType);

                case BFInstruction.Jump:
                case BFInstruction.JumpIfFalse:
                    string jumpLabelName = stringLiteral.Split(new char[] { '@', ':' }, StringSplitOptions.RemoveEmptyEntries)[0];

                    if (jumpLabelName.Length == 0)
                        throw new BFASMParserException(instr, lineIndex + 1);

                    unresolvedJumps.Add(Tuple.Create(jumpLabelName, opIndex));
                    break;
            }

            // actual operand value will be resolved later, so just set it to -1 for now
            return new BFOpcode(instr, -1);
        }

        private static void ResolveLabelReferences(List<Tuple<string, int>> unresolved, List<BFCodeLabel> labels, List<BFOpcode> opcodes)
        {
            for (int i = 0; i < unresolved.Count; i++)
            {
                int idx = labels.FindIndex(j => j.Name == unresolved[i].Item1);

                // label was not found
                if (idx == -1)
                    throw new BFASMParserException("Label {0} was referenced but was not found.", unresolved[i].Item1);

                opcodes[unresolved[i].Item2].Operand.ImmediateValue = idx;
            }
        }
    }

    [Serializable]
    public class BFASMParserException : Exception
    {
        public BFASMParserException(object instruction, object line)
            : base(string.Format("'{0}' instruction at line {1} was not in expected format", instruction, line))
        {
        }

        public BFASMParserException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }
    }
}
