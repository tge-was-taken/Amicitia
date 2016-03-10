using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlusLibSharp.Scripting
{
    public enum BFOperandType
    {
        Immediate = 1,
        FloatingPoint = 2,
    }

    public class BFOperand
    {
        public BFOperandType Type { get; internal set; }
        public long? ImmediateValue { get; internal set; }
        public float? FloatValue { get; internal set; }

        internal BFOperand(long immediate)
        {
            Type = BFOperandType.Immediate;
            ImmediateValue = immediate;
        }

        internal BFOperand(float floatValue)
        {
            Type = BFOperandType.FloatingPoint;
            FloatValue = floatValue;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case BFOperandType.Immediate:
                    return ImmediateValue.Value.ToString();
                case BFOperandType.FloatingPoint:
                    return FloatValue.Value.ToString();
                default:
                    return string.Empty;
            }
        }
    }

    public class BFOpcode
    {
        private ushort _instr;
        private BFOperand _operand;
        private int _codeBlockIndex;

        public BFOpcode(BFInstruction instruction, IConvertible operand)
        {
            _instr = (ushort)instruction;
            SetOperand(operand);
        }

        internal BFOpcode(ushort instruction, int codeBlockIndex, IConvertible operand)
        {
            _instr = instruction;
            _codeBlockIndex = codeBlockIndex;
            SetOperand(operand);
        }

        private void SetOperand(IConvertible operand)
        {
            switch (Instruction)
            {
                case BFInstruction.SetVariable:
                case BFInstruction.PushVariable:
                case BFInstruction.PushUInt16:
                case BFInstruction.JumpIfFalse:
                case BFInstruction.Jump:
                case BFInstruction.CallProcedure:
                case BFInstruction.CallNative:
                case BFInstruction.BeginProcedure:
                case BFInstruction.PushUInt32:
                    if (operand == null)
                        throw new ArgumentException("You must specify 1 operand for this instruction.");
                    _operand = new BFOperand(Convert.ToInt64(operand));
                    break;
                case BFInstruction.PushFloat:
                    if (operand == null)
                        throw new ArgumentException("You must specify 1 operand for this instruction.");
                    _operand = new BFOperand(Convert.ToSingle(operand));
                    break;
                default:
                    if (operand != null)
                        _operand = new BFOperand(Convert.ToInt64(operand));
                    break;
            }
        }

        internal BFOpcode(BFInstruction instruction, int codeBlockIndex, IConvertible operand)
        {
            _instr = (ushort)instruction;
            _codeBlockIndex = codeBlockIndex;
            SetOperand(operand);
        }

        public BFInstruction Instruction
        {
            get { return (BFInstruction)_instr; }
            internal set { _instr = (ushort)value; }
        }

        public BFOperand Operand
        {
            get { return _operand; }
            internal set { _operand = value; }
        }

        public int CodeBlockIndex
        {
            get { return _codeBlockIndex; }
        }

        public int Size
        {
            get
            {
                switch (Instruction)
                {
                    case BFInstruction.PushUInt32:
                    case BFInstruction.PushFloat:
                        return 8; // these occupy the next 4 bytes as well to store the data for the push
                    default:
                        return 4;
                }
            }
        }

        public override string ToString()
        {
            return Instruction.ToString() + (_operand != null ? " " + _operand.ToString() : string.Empty);
        }
    }
}
