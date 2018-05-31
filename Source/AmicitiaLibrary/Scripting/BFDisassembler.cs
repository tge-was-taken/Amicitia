using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AmicitiaLibrary.Scripting
{
    internal static class BfDisassembler
    {
        public static Dictionary<BfCallNativeType, string> CallNativeToHintString = new Dictionary<BfCallNativeType, string>()
        {
            { BfCallNativeType.ItfSel,         "Takes 3 arguments" },
            { BfCallNativeType.ItfYesnoAns,   "Takes no arguments" },
            { BfCallNativeType.ItfYesnoCls,   "Takes no arguments" },
            { BfCallNativeType.ItfYesnoDsp,   "Takes 1 argument" },
            { BfCallNativeType.MsgDisp,        "Takes 1 argument." },
            { BfCallNativeType.Msg,             "Takes 2 arguments where the first is the dialogue id" },
            { BfCallNativeType.MsgSel,         "Takes 3 arguments where the first is dialogue id" },
            { BfCallNativeType.MsgWndCls,     "Takes 1 argument" },
            { BfCallNativeType.MsgWndDsp,     "Takes 3 arguments" },
            { BfCallNativeType.Put,             "Takes 1 argument where the first is the variable value?" },
            { BfCallNativeType.RumbleStart,    "Takes 3 arguments" },
            { BfCallNativeType.RumbleStop,     "Takes no arguments" },
            { BfCallNativeType.ScrDecComvar,  "Takes 1 argument, the variable id" },
            { BfCallNativeType.ScrGetComvar,  "Takes 1 argument, the variable id" },
            { BfCallNativeType.ScrIncComvar,  "Takes 1 argument, the variable id" },
            { BfCallNativeType.ScrSetComvar,  "Takes 2 arguments, the variable id, and the value to set" },
            { BfCallNativeType.FlagClear,      "Takes 1 argument: the flag to clear."},
            { BfCallNativeType.FlagGet,        "Takes 1 argument: the flag to get." },
            { BfCallNativeType.FlagSet,        "Takes 1 argument: the flag to set." }
        };

        public static Dictionary<BfInstruction, string> BfInstructionToAsmKeyword = new Dictionary<BfInstruction, string>()
        {
            { BfInstruction.Add,                "add" },
            { BfInstruction.BeginProcedure,     "beginp" },
            { BfInstruction.CallNative,         "calln" },
            { BfInstruction.CallProcedure,      "callp" },
            { BfInstruction.Division,           "div" },
            { BfInstruction.Jump,               "jmp" },
            { BfInstruction.JumpIfFalse,        "jmpf" },
            { BfInstruction.Minus,              "min" },
            { BfInstruction.Multiply,           "mult" },
            { BfInstruction.Equal,              "eq"},
            { BfInstruction.Not,                "not" },
            { BfInstruction.NotEqual,           "neq" },
            { BfInstruction.NotEqualZero,       "neqz" },
            { BfInstruction.PopFloat,           "popf" },
            { BfInstruction.PopInt,             "popi" },
            { BfInstruction.PushFloat,          "pushf" },
            { BfInstruction.PushResult,         "pushr" },
            { BfInstruction.PushUInt16,         "pushs" },
            { BfInstruction.PushUInt32,         "pushi" },
            { BfInstruction.PushVariable,       "pushv" },
            { BfInstruction.Return,             "ret" },
            { BfInstruction.SetVariable,        "setv" },
            { BfInstruction.Subtract,           "sub" },
        };

        public static List<BfOpcode> ParseCodeblock(uint[] data, out bool isExtendedOpcodePresent)
        {
            isExtendedOpcodePresent = false;
            List<BfOpcode> opcodes = new List<BfOpcode>(2 << 12);

            for (int i = 0; i < data.Length; i++)
            {
                BfInstruction instruction = (BfInstruction)(data[i] & 0x0000FFFF);
                BfOpcode op = null;

                switch (instruction)
                {
                    case BfInstruction.PushUInt32:
                        {
                            if (!isExtendedOpcodePresent) // optimization, keep track of whether or not there are extended opcodes for sorting later
                                isExtendedOpcodePresent = true;

                            Debug.Assert((data[i] & 0xFFFF0000) == 0); // debug: check if the high bits are indeed zeroed out

                            op = new BfOpcode(instruction, i, data[++i]); // consume next int for op
                        }
                        break;
                    case BfInstruction.PushFloat:
                        {
                            if (!isExtendedOpcodePresent) // optimization, keep track of whether or not there are extended opcodes for sorting later
                                isExtendedOpcodePresent = true;

                            Debug.Assert((data[i] & 0xFFFF0000) == 0); // debug: check if the high bits are indeed zeroed out

                            op = new BfOpcode(instruction, i, BitConverter.ToSingle(BitConverter.GetBytes(data[++i]), 0)); // consume next int as float for op
                        }
                        break;
                    case BfInstruction.Add:
                    case BfInstruction.NotEqual:
                    case BfInstruction.PushResult:
                    case BfInstruction.Return:
                    case BfInstruction.Subtract:
                    case BfInstruction.Equal:
                        Debug.Assert((data[i] & 0xFFFF0000) == 0); // debug: check if the high bits are indeed zeroed out
                        op = new BfOpcode(instruction, i, null); // uses no operands, high bits are zeroed out
                        break;
                    default:
                        op = new BfOpcode(instruction, i, ((data[i] & 0xFFFF0000) >> 16));
                        break;     
                }

                opcodes.Add(op);
            }

            return opcodes;
        }

        public static void Disassemble(string path, List<BfOpcode> opcodes, BfCodeLabel[] procedures, BfCodeLabel[] jumpLabels)
        {
            using (StreamWriter writer = new StreamWriter(File.Create(path)))
            {
                int procIdx = 0;
                int jumpIdx = 0;

                // sorting these is faster than searching every interation
                BfCodeLabel[] sortedProcedures = procedures.OrderBy(i => i.OpcodeIndex).ToArray(); 
                BfCodeLabel[] sortedJumpLabels = jumpLabels.OrderBy(i => i.OpcodeIndex).ToArray();

                for (int i = 0; i < opcodes.Count; i++)
                {
                    BfOpcode op = opcodes[i];
                    BfCodeLabel procedure = null;
                    BfCodeLabel jumpLabel = null;

                    // first check if maybe a procedure starts at this index
                    if (procIdx != sortedProcedures.Length && sortedProcedures[procIdx].OpcodeIndex == i)
                    {
                        procedure = sortedProcedures[procIdx++];
                        writer.WriteLine("\n" + procedure.Name + ": \t; start of procedure \"{0}\"", procedure.Name);
                    }
                    // if it's not the start of a procedure then check if it's maybe a jump label
                    else if (jumpIdx != sortedJumpLabels.Length && sortedJumpLabels[jumpIdx].OpcodeIndex == i)
                    {
                        jumpLabel = sortedJumpLabels[jumpIdx++];
                        writer.WriteLine("\n@" + jumpLabel.Name + ": \t; jump label \"{0}\"", jumpLabel.Name);
                    }

                    bool isKnown = BfInstructionToAsmKeyword.ContainsKey(op.Instruction);
                    string opStr = isKnown ?
                        BfInstructionToAsmKeyword[op.Instruction] :
                        "unk_" + op.Instruction.ToString("X");

                    switch (op.Instruction)
                    {
                        case BfInstruction.CallNative:
                            BfCallNativeType callType = (BfCallNativeType)op.Operand.ImmediateValue;
                            opStr += " " + callType + (Enum.IsDefined(typeof(BfCallNativeType), callType) ? " \t; " + CallNativeToHintString[callType] : string.Empty);
                            break;
                        case BfInstruction.BeginProcedure:
                            opStr += " " + procedure.Name;
                            break;
                        case BfInstruction.CallProcedure:
                            opStr += " " + procedures[(int)op.Operand.ImmediateValue].Name;
                            break;
                        case BfInstruction.Jump:
                        case BfInstruction.JumpIfFalse:
                            opStr += " @" + jumpLabels[(int)op.Operand.ImmediateValue].Name;
                            break;
                        default:
                            if (op.Operand != null)
                                opStr += " " + op.Operand.ToString();
                            break;
                    }

                    writer.WriteLine("\t" + opStr);
                }
            }
        }
    }
}
