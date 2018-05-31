using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmicitiaLibrary.Scripting
{
    public enum BfOperandType
    {
        Immediate = 1,
        FloatingPoint = 2,
    }

    public class BfOperand
    {
        public BfOperandType Type { get; internal set; }
        public long? ImmediateValue { get; internal set; }
        public float? FloatValue { get; internal set; }

        internal BfOperand(long immediate)
        {
            Type = BfOperandType.Immediate;
            ImmediateValue = immediate;
        }

        internal BfOperand(float floatValue)
        {
            Type = BfOperandType.FloatingPoint;
            FloatValue = floatValue;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case BfOperandType.Immediate:
                    return ImmediateValue.Value.ToString();
                case BfOperandType.FloatingPoint:
                    return FloatValue.Value.ToString();
                default:
                    return string.Empty;
            }
        }
    }

    public class BfOpcode
    {
        private ushort mInstr;
        private BfOperand mOperand;
        private int mCodeBlockIndex;

        public BfOpcode(BfInstruction instruction, IConvertible operand)
        {
            mInstr = (ushort)instruction;
            SetOperand(operand);
        }

        internal BfOpcode(ushort instruction, int codeBlockIndex, IConvertible operand)
        {
            mInstr = instruction;
            mCodeBlockIndex = codeBlockIndex;
            SetOperand(operand);
        }

        private void SetOperand(IConvertible operand)
        {
            switch (Instruction)
            {
                case BfInstruction.SetVariable:
                case BfInstruction.PushVariable:
                case BfInstruction.PushUInt16:
                case BfInstruction.JumpIfFalse:
                case BfInstruction.Jump:
                case BfInstruction.CallProcedure:
                case BfInstruction.CallNative:
                case BfInstruction.BeginProcedure:
                case BfInstruction.PushUInt32:
                    if (operand == null)
                        throw new ArgumentException("You must specify 1 operand for this instruction.");
                    mOperand = new BfOperand(Convert.ToInt64(operand));
                    break;
                case BfInstruction.PushFloat:
                    if (operand == null)
                        throw new ArgumentException("You must specify 1 operand for this instruction.");
                    mOperand = new BfOperand(Convert.ToSingle(operand));
                    break;
                default:
                    if (operand != null)
                        mOperand = new BfOperand(Convert.ToInt64(operand));
                    break;
            }
        }

        internal BfOpcode(BfInstruction instruction, int codeBlockIndex, IConvertible operand)
        {
            mInstr = (ushort)instruction;
            mCodeBlockIndex = codeBlockIndex;
            SetOperand(operand);
        }

        public BfInstruction Instruction
        {
            get { return (BfInstruction)mInstr; }
            internal set { mInstr = (ushort)value; }
        }

        public BfOperand Operand
        {
            get { return mOperand; }
            internal set { mOperand = value; }
        }

        public int CodeBlockIndex
        {
            get { return mCodeBlockIndex; }
        }

        public int Size
        {
            get
            {
                switch (Instruction)
                {
                    case BfInstruction.PushUInt32:
                    case BfInstruction.PushFloat:
                        return 8; // these occupy the next 4 bytes as well to store the data for the push
                    default:
                        return 4;
                }
            }
        }

        public override string ToString()
        {
            return Instruction.ToString() + (mOperand != null ? " " + mOperand.ToString() : string.Empty);
        }
    }
}
