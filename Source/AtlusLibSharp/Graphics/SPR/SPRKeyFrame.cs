namespace AtlusLibSharp.Graphics.SPR
{
    using System.IO;
    using AtlusLibSharp.Utilities;

    public class SPRKeyFrame
    {
        // Private Fields
        private int _unk0x00;
        private string _comment;
        private int _textureIndex;
        private int _unk0x18;   // some sort of type?
        private int _unk0x1C;
        private int _unk0x20;
        private int _unk0x24;
        private int _unk0x28;
        private int _unk0x2C;
        private int _unk0x30;
        private int _unk0x34;
        private int _unk0x38;
        private int _unk0x3C;
        private int _unk0x40;   // set in 'center' frames?
        private int _unk0x44;
        private int _unk0x48;
        private int _unk0x4C;
        private int _unk0x50;
        private int _unk0x54;   // start time??
        private int _unk0x58;   // interpolation time??
        private int _unk0x5C;   // end time??
        private int _unk0x60;   // speed?
        private int _unk0x64;   // argb color
        private int _unk0x68;   // argb color
        private int _unk0x6C;   // argb color
        private int _unk0x70;   // argb color
        private int _unk0x74;   // possibly padding
        private int _unk0x78;   // possibly padding
        private int _unk0x7C;   // possibly padding

        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        // Constructors
        internal SPRKeyFrame(BinaryReader reader)
        {
            InternalRead(reader);
        }

        // Internal Methods
        internal void InternalWrite(BinaryWriter writer)
        {
            writer.Write(_unk0x00);
            writer.WriteCString(_comment, 16);
            writer.Write(_textureIndex);
            writer.Write(_unk0x18);
            writer.Write(_unk0x1C);
            writer.Write(_unk0x20);
            writer.Write(_unk0x24);
            writer.Write(_unk0x28);
            writer.Write(_unk0x2C);
            writer.Write(_unk0x30);
            writer.Write(_unk0x34);
            writer.Write(_unk0x38);
            writer.Write(_unk0x3C);
            writer.Write(_unk0x40);
            writer.Write(_unk0x44);
            writer.Write(_unk0x48);
            writer.Write(_unk0x4C);
            writer.Write(_unk0x50);
            writer.Write(_unk0x54);
            writer.Write(_unk0x58);
            writer.Write(_unk0x5C);
            writer.Write(_unk0x60);
            writer.Write(_unk0x64);
            writer.Write(_unk0x68);
            writer.Write(_unk0x6C);
            writer.Write(_unk0x70);
            writer.Write(_unk0x74);
            writer.Write(_unk0x78);
            writer.Write(_unk0x7C);
        }

        // Private Methods
        private void InternalRead(BinaryReader reader)
        {
            _unk0x00 = reader.ReadInt32();
            _comment = reader.ReadCString(16);
            _textureIndex = reader.ReadInt32();
            _unk0x18 = reader.ReadInt32();
            _unk0x1C = reader.ReadInt32();
            _unk0x20 = reader.ReadInt32();
            _unk0x24 = reader.ReadInt32();
            _unk0x28 = reader.ReadInt32();
            _unk0x2C = reader.ReadInt32();
            _unk0x30 = reader.ReadInt32();
            _unk0x34 = reader.ReadInt32();
            _unk0x38 = reader.ReadInt32();
            _unk0x3C = reader.ReadInt32();
            _unk0x40 = reader.ReadInt32();
            _unk0x44 = reader.ReadInt32();
            _unk0x48 = reader.ReadInt32();
            _unk0x4C = reader.ReadInt32();
            _unk0x50 = reader.ReadInt32();
            _unk0x54 = reader.ReadInt32();
            _unk0x58 = reader.ReadInt32();
            _unk0x5C = reader.ReadInt32();
            _unk0x60 = reader.ReadInt32();
            _unk0x64 = reader.ReadInt32();
            _unk0x68 = reader.ReadInt32();
            _unk0x6C = reader.ReadInt32();
            _unk0x70 = reader.ReadInt32();
            _unk0x74 = reader.ReadInt32();
            _unk0x78 = reader.ReadInt32();
            _unk0x7C = reader.ReadInt32();
        }
    }
}
