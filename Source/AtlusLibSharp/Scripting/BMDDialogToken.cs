using System.IO;
using System.Text;

namespace AtlusLibSharp.Scripting
{
    /*
    struct MsgOpcode
    {
	    char ident : 4; // 0xF
	    char numProceedingBytes : 4; // this includes the next byte of the opcode
	    char opType1 : 3; // index for which function table out of 8 to use
	    char opType2 : 5; // index for which function out of max. 32 to use
    }
    */

    public abstract class BmdDialogToken
    {
        public abstract BmdDialogTokenType Type { get; }

        public abstract byte[] Data { get; }

        internal abstract void InternalWrite(BinaryWriter writer);
    }

    public class BmdFunctionToken : BmdDialogToken
    {
        private byte mFuncCategory;
        private byte mFuncId;
        private byte[] mParams;

        public byte FunctionCategory
        {
            get { return mFuncCategory; }
        }

        public byte FunctionId
        {
            get { return mFuncId; }
        }

        public byte[] Parameters
        {
            get { return mParams; }
        }

        public override BmdDialogTokenType Type
        {
            get { return BmdDialogTokenType.Function; }
        }

        public override byte[] Data
        {
            get
            {
                using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
                {
                    InternalWrite(writer);
                    return (writer.BaseStream as MemoryStream).ToArray();
                }
            }
        }

        public override string ToString()
        {
            string str = $"func {mFuncCategory:X} {mFuncId:X}";
            for (int i = 0; i < mParams.Length; i++)
            {
                byte param = mParams[i];

                if (param != 0xFF)
                    param -= 1;

                str += $" {param:X}";
            }
            return str;
        }

        public BmdFunctionToken(byte funcCategory, byte funcId, params byte[] funcParams)
        {
            mFuncCategory = funcCategory;
            mFuncId = funcId;
            mParams = funcParams;
        }

        internal override void InternalWrite(BinaryWriter writer)
        {
            writer.Write(0xF << 4 | mParams.Length + 1);
            writer.Write((mFuncCategory & 0x7) << 5 | mFuncId & 0x1F);
            writer.Write(mParams);
        }
    }

    public class BmdTextToken : BmdDialogToken
    {
        private string mString;

        public override BmdDialogTokenType Type
        {
            get { return BmdDialogTokenType.Text; }
        }

        public override byte[] Data
        {
            get { return Encoding.GetEncoding("SHIFT_JIS").GetBytes(mString); }
        }

        public BmdTextToken(string value)
        {
            mString = value;
        }

        public BmdTextToken(byte[] stringBytes)
        {
            mString = Encoding.GetEncoding("SHIFT_JIS").GetString(stringBytes);
        }

        public override string ToString()
        {
            return mString;
        }

        internal override void InternalWrite(BinaryWriter writer)
        {
            writer.Write(Data);
        }
    }

    public enum BmdDialogTokenType
    {
        Default,
        Text,
        Function
    }
}
