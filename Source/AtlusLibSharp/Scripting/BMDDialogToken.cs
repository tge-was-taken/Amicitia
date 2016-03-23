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

    public abstract class BMDDialogToken
    {
        public abstract BMDDialogTokenType Type { get; }

        public abstract byte[] Data { get; }

        internal abstract void InternalWrite(BinaryWriter writer);
    }

    public class BMDFunctionToken : BMDDialogToken
    {
        private byte _funcCategory;
        private byte _funcID;
        private byte[] _params;

        public byte FunctionCategory
        {
            get { return _funcCategory; }
        }

        public byte FunctionID
        {
            get { return _funcID; }
        }

        public byte[] Parameters
        {
            get { return _params; }
        }

        public override BMDDialogTokenType Type
        {
            get { return BMDDialogTokenType.Function; }
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
            string str = string.Format("func {0:X} {1:X}", _funcCategory, _funcID);
            for (int i = 0; i < _params.Length; i++)
            {
                byte param = _params[i];

                if (param != 0xFF)
                    param -= 1;

                str += string.Format(" {0:X}", param);
            }
            return str;
        }

        public BMDFunctionToken(byte funcCategory, byte funcID, params byte[] funcParams)
        {
            _funcCategory = funcCategory;
            _funcID = funcID;
            _params = funcParams;
        }

        internal override void InternalWrite(BinaryWriter writer)
        {
            writer.Write(0xF << 4 | _params.Length + 1);
            writer.Write((_funcCategory & 0x7) << 5 | _funcID & 0x1F);
            writer.Write(_params);
        }
    }

    public class BMDTextToken : BMDDialogToken
    {
        private string _string;

        public override BMDDialogTokenType Type
        {
            get { return BMDDialogTokenType.Text; }
        }

        public override byte[] Data
        {
            get { return Encoding.GetEncoding("SHIFT_JIS").GetBytes(_string); }
        }

        public BMDTextToken(string value)
        {
            _string = value;
        }

        public BMDTextToken(byte[] stringBytes)
        {
            _string = Encoding.GetEncoding("SHIFT_JIS").GetString(stringBytes);
        }

        public override string ToString()
        {
            return _string;
        }

        internal override void InternalWrite(BinaryWriter writer)
        {
            writer.Write(Data);
        }
    }

    public enum BMDDialogTokenType
    {
        Default,
        Text,
        Function
    }
}
