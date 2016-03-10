using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AtlusLibSharp.Scripting
{
    internal static class BFDisassembler
    {
        public static Dictionary<BFCallNativeType, string> CallNativeToHintString = new Dictionary<BFCallNativeType, string>()
        {
            { BFCallNativeType.ITF_SEL,         "Takes 3 arguments" },
            { BFCallNativeType.ITF_YESNO_ANS,   "Takes no arguments" },
            { BFCallNativeType.ITF_YESNO_CLS,   "Takes no arguments" },
            { BFCallNativeType.ITF_YESNO_DSP,   "Takes 1 argument" },
            { BFCallNativeType.MSG_DISP,        "Takes 1 argument." },
            { BFCallNativeType.MSG,             "Takes 2 arguments where the first is the dialogue id" },
            { BFCallNativeType.MSG_SEL,         "Takes 3 arguments where the first is dialogue id" },
            { BFCallNativeType.MSG_WND_CLS,     "Takes 1 argument" },
            { BFCallNativeType.MSG_WND_DSP,     "Takes 3 arguments" },
            { BFCallNativeType.PUT,             "Takes 1 argument where the first is the variable value?" },
            { BFCallNativeType.RUMBLE_START,    "Takes 3 arguments" },
            { BFCallNativeType.RUMBLE_STOP,     "Takes no arguments" },
            { BFCallNativeType.SCR_DEC_COMVAR,  "Takes 1 argument, the variable id" },
            { BFCallNativeType.SCR_GET_COMVAR,  "Takes 1 argument, the variable id" },
            { BFCallNativeType.SCR_INC_COMVAR,  "Takes 1 argument, the variable id" },
            { BFCallNativeType.SCR_SET_COMVAR,  "Takes 2 arguments, the variable id, and the value to set" },
            { BFCallNativeType.FLAG_CLEAR,      "Takes 1 argument: the flag to clear."},
            { BFCallNativeType.FLAG_GET,        "Takes 1 argument: the flag to get." },
            { BFCallNativeType.FLAG_SET,        "Takes 1 argument: the flag to set." }
        };

        public static Dictionary<BFInstruction, string> BFInstructionToASMKeyword = new Dictionary<BFInstruction, string>()
        {
            { BFInstruction.Add,                "add" },
            { BFInstruction.BeginProcedure,     "beginp" },
            { BFInstruction.CallNative,         "calln" },
            { BFInstruction.CallProcedure,      "callp" },
            { BFInstruction.Division,           "div" },
            { BFInstruction.Jump,               "jmp" },
            { BFInstruction.JumpIfFalse,        "jmpf" },
            { BFInstruction.Minus,              "min" },
            { BFInstruction.Multiply,           "mult" },
            { BFInstruction.Equal,              "eq"},
            { BFInstruction.Not,                "not" },
            { BFInstruction.NotEqual,           "neq" },
            { BFInstruction.NotEqualZero,       "neqz" },
            { BFInstruction.PopFloat,           "popf" },
            { BFInstruction.PopInt,             "popi" },
            { BFInstruction.PushFloat,          "pushf" },
            { BFInstruction.PushResult,         "pushr" },
            { BFInstruction.PushUInt16,         "pushs" },
            { BFInstruction.PushUInt32,         "pushi" },
            { BFInstruction.PushVariable,       "pushv" },
            { BFInstruction.Return,             "ret" },
            { BFInstruction.SetVariable,        "setv" },
            { BFInstruction.Subtract,           "sub" },
        };

        public static List<BFOpcode> ParseCodeblock(uint[] data, out bool isExtendedOpcodePresent)
        {
            isExtendedOpcodePresent = false;
            List<BFOpcode> opcodes = new List<BFOpcode>(2 << 12);

            for (int i = 0; i < data.Length; i++)
            {
                BFInstruction instruction = (BFInstruction)(data[i] & 0x0000FFFF);
                BFOpcode op = null;

                switch (instruction)
                {
                    case BFInstruction.PushUInt32:
                        {
                            if (!isExtendedOpcodePresent) // optimization, keep track of whether or not there are extended opcodes for sorting later
                                isExtendedOpcodePresent = true;

                            Debug.Assert((data[i] & 0xFFFF0000) == 0); // debug: check if the high bits are indeed zeroed out

                            op = new BFOpcode(instruction, i, data[++i]); // consume next int for op
                        }
                        break;
                    case BFInstruction.PushFloat:
                        {
                            if (!isExtendedOpcodePresent) // optimization, keep track of whether or not there are extended opcodes for sorting later
                                isExtendedOpcodePresent = true;

                            Debug.Assert((data[i] & 0xFFFF0000) == 0); // debug: check if the high bits are indeed zeroed out

                            op = new BFOpcode(instruction, i, BitConverter.ToSingle(BitConverter.GetBytes(data[++i]), 0)); // consume next int as float for op
                        }
                        break;
                    case BFInstruction.Add:
                    case BFInstruction.NotEqual:
                    case BFInstruction.PushResult:
                    case BFInstruction.Return:
                    case BFInstruction.Subtract:
                    case BFInstruction.Equal:
                        Debug.Assert((data[i] & 0xFFFF0000) == 0); // debug: check if the high bits are indeed zeroed out
                        op = new BFOpcode(instruction, i, null); // uses no operands, high bits are zeroed out
                        break;
                    default:
                        op = new BFOpcode(instruction, i, ((data[i] & 0xFFFF0000) >> 16));
                        break;     
                }

                opcodes.Add(op);
            }

            return opcodes;
        }

        public static void Disassemble(string path, List<BFOpcode> opcodes, BFCodeLabel[] procedures, BFCodeLabel[] jumpLabels)
        {
            using (StreamWriter writer = new StreamWriter(File.Create(path)))
            {
                int procIdx = 0;
                int jumpIdx = 0;

                // sorting these is faster than searching every interation
                BFCodeLabel[] sortedProcedures = procedures.OrderBy(i => i.OpcodeIndex).ToArray(); 
                BFCodeLabel[] sortedJumpLabels = jumpLabels.OrderBy(i => i.OpcodeIndex).ToArray();

                for (int i = 0; i < opcodes.Count; i++)
                {
                    BFOpcode op = opcodes[i];
                    BFCodeLabel procedure = null;
                    BFCodeLabel jumpLabel = null;

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

                    bool isKnown = BFInstructionToASMKeyword.ContainsKey(op.Instruction);
                    string opStr = isKnown ?
                        BFInstructionToASMKeyword[op.Instruction] :
                        "unk_" + op.Instruction.ToString("X");

                    switch (op.Instruction)
                    {
                        case BFInstruction.CallNative:
                            BFCallNativeType callType = (BFCallNativeType)op.Operand.ImmediateValue;
                            opStr += " " + callType + (Enum.IsDefined(typeof(BFCallNativeType), callType) ? " \t; " + CallNativeToHintString[callType] : string.Empty);
                            break;
                        case BFInstruction.BeginProcedure:
                            opStr += " " + procedure.Name;
                            break;
                        case BFInstruction.CallProcedure:
                            opStr += " " + procedures[(int)op.Operand.ImmediateValue].Name;
                            break;
                        case BFInstruction.Jump:
                        case BFInstruction.JumpIfFalse:
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
