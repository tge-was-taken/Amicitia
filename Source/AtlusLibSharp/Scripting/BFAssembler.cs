using AtlusLibSharp.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace AtlusLibSharp.Scripting
{
    internal static class BfAssembler
    {
        internal static Dictionary<string, BfInstruction> AsmKeywordToBfInstruction = BfDisassembler.BfInstructionToAsmKeyword.Reverse();

        public static BfFile AssembleFromAsmText(string path)
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
                List<BfCodeLabel> procedures = new List<BfCodeLabel>();
                List<BfCodeLabel> jumps = new List<BfCodeLabel>();
                List<BfOpcode> opcodes = new List<BfOpcode>();

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
                            jumps.Add(ParseAsmLabel(lineIndex, tokens[0], jumpIndex, opIndex, '@', ':'));
                            jumpIndex++;
                        }
                        else // the label is a procedure identifier
                        {
                            procedures.Add(ParseAsmLabel(lineIndex, tokens[0], procIndex, opIndex, ':'));
                            procIndex++;
                        }
                    }
                    else // we got an instruction
                    {
                        // instruction without any procedures, throw an error
                        if (procedures.Count == 0)
                            throw new BfasmParserException("Instruction defined at line {0} before any procedures", lineIndex + 1);

                        BfInstruction instr;
                        BfOpcode op = null;
                        bool isDefined = AsmKeywordToBfInstruction.TryGetValue(tokens[0].ToLowerInvariant(), out instr);

                        if (!isDefined) // 'unk_XX' instruction expected
                        {
                            string opNumStr = tokens[0].Split(new string[] { "unk_" }, StringSplitOptions.RemoveEmptyEntries)[0];
                            int opNumInt;
                            long operand;

                            if (opNumStr.Length < 2 || !int.TryParse(opNumStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out opNumInt) ||
                                !long.TryParse(tokens[1], out operand))
                            {
                                throw new BfasmParserException("unk_XX", lineIndex + 1);
                            }

                            op = new BfOpcode((ushort)opNumInt, 0, operand);
                        }
                        else // regular instruction expected
                        {
                            // verify the number of arguments
                            VerifyArgCount(instr, tokens, lineIndex + 1);

                            switch (instr)
                            {
                                case BfInstruction.BeginProcedure:
                                case BfInstruction.CallNative:
                                case BfInstruction.CallProcedure:
                                case BfInstruction.Jump:
                                case BfInstruction.JumpIfFalse:
                                    string strArg = tokens[1];
                                    int intArg;

                                    if (int.TryParse(strArg, out intArg)) // reference by id
                                    {
                                        op = new BfOpcode(instr, intArg);
                                    }
                                    else // reference by name 
                                    {
                                        op = ParseInstrWithStringLiteralArgs(instr, strArg, lineIndex, opIndex, unresolvedProcs, unresolvedJumps);
                                    }
                                    break;

                                case BfInstruction.PushFloat:
                                    op = ParseInstrWithDecimalLiteralArgs(instr, tokens, lineIndex);
                                    break;

                                case BfInstruction.PushUInt16:
                                case BfInstruction.PushVariable:
                                case BfInstruction.SetVariable:
                                case BfInstruction.PushUInt32:
                                    op = ParseInstrWithNumberLiteralArgs(instr, tokens, lineIndex);
                                    break;

                                default:
                                    op = new BfOpcode(instr, null); // opcode takes no operands, eg add, subtract, return
                                    break;
                            }
                        }

                        // this shouldn't be possible
                        // but they said the same about the world being round
                        if (op == null)
                            throw new BfasmParserException(instr, lineIndex + 1);

                        opcodes.Add(op);
                        opIndex++;
                    }
                }

                // fix up unresolved jump/procedure references
                ResolveLabelReferences(unresolvedJumps, jumps, opcodes);
                ResolveLabelReferences(unresolvedProcs, procedures, opcodes);

                return new BfFile(procedures.ToArray(), jumps.ToArray(), opcodes);
            }
        }

        private static BfCodeLabel ParseAsmLabel(int lineIndex, string nameToken, int jumpIndex, int opIndex, params char[] splitChars)
        {
            BfCodeLabel label = new BfCodeLabel()
            {
                Name = nameToken.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[0],
                Id = jumpIndex,
                OpcodeIndex = opIndex
            };

            if (label.Name.Length == 0)
                throw new BfasmParserException("Label", lineIndex + 1);

            return label;
        }

        private static void VerifyArgCount(BfInstruction instr, string[] tokens, int lineIndex)
        {
            switch (instr)
            {
                case BfInstruction.PushUInt32:
                case BfInstruction.PushFloat:
                case BfInstruction.BeginProcedure:
                case BfInstruction.CallNative:
                case BfInstruction.CallProcedure:
                case BfInstruction.Jump:
                case BfInstruction.JumpIfFalse:
                case BfInstruction.PushUInt16:
                case BfInstruction.PushVariable:
                case BfInstruction.SetVariable:
                    if (tokens.Length < 2)
                        throw new BfasmParserException(instr, lineIndex + 1);
                    break;
            }
        }

        private static BfOpcode ParseInstrWithNumberLiteralArgs(BfInstruction instr, string[] tokens, int lineIndex)
        {
            long value;

            if (!long.TryParse(tokens[1], out value))
                throw new BfasmParserException(instr, lineIndex + 1);

            return new BfOpcode(instr, value);
        }

        private static BfOpcode ParseInstrWithDecimalLiteralArgs(BfInstruction instr, string[] tokens, int lineIndex)
        {
            float value;

            if (!float.TryParse(tokens[1], out value))
                throw new BfasmParserException(instr, lineIndex + 1);

            return new BfOpcode(instr, value);
        }

        private static BfOpcode ParseInstrWithStringLiteralArgs(
            BfInstruction instr, string stringLiteral, int lineIndex, int opIndex,
            List<Tuple<string, int>> unresolvedProcs, List<Tuple<string, int>> unresolvedJumps)
        {
            switch (instr)
            {
                case BfInstruction.BeginProcedure:
                case BfInstruction.CallProcedure:
                    unresolvedProcs.Add(Tuple.Create(stringLiteral, opIndex));
                    break;

                case BfInstruction.CallNative:
                    BfCallNativeType callType;

                    if (!Enum.TryParse(stringLiteral, true, out callType))
                        throw new BfasmParserException(instr, lineIndex + 1);

                    return new BfOpcode(instr, (int)callType);

                case BfInstruction.Jump:
                case BfInstruction.JumpIfFalse:
                    string jumpLabelName = stringLiteral.Split(new char[] { '@', ':' }, StringSplitOptions.RemoveEmptyEntries)[0];

                    if (jumpLabelName.Length == 0)
                        throw new BfasmParserException(instr, lineIndex + 1);

                    unresolvedJumps.Add(Tuple.Create(jumpLabelName, opIndex));
                    break;
            }

            // actual operand value will be resolved later, so just set it to -1 for now
            return new BfOpcode(instr, -1);
        }

        private static void ResolveLabelReferences(List<Tuple<string, int>> unresolved, List<BfCodeLabel> labels, List<BfOpcode> opcodes)
        {
            for (int i = 0; i < unresolved.Count; i++)
            {
                int idx = labels.FindIndex(j => j.Name == unresolved[i].Item1);

                // label was not found
                if (idx == -1)
                    throw new BfasmParserException("Label {0} was referenced but was not found.", unresolved[i].Item1);

                opcodes[unresolved[i].Item2].Operand.ImmediateValue = idx;
            }
        }
    }

    [Serializable]
    public class BfasmParserException : Exception
    {
        public BfasmParserException(object instruction, object line)
            : base($"'{instruction}' instruction at line {line} was not in expected format")
        {
        }

        public BfasmParserException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }
    }
}
