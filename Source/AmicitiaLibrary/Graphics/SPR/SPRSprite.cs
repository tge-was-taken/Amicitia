using System.Text;

namespace AmicitiaLibrary.Graphics.SPR
{
    using System.IO;
    using AmicitiaLibrary.Utilities;
    using System.Drawing;

    public class SprSprite
    {
        // Private Fields
        private int _unk0x00;
        private string mComment;
        private int mTextureIndex;
        private int _unk0x18;   // some sort of id?
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
        private int mOffsetX;
        private int mOffsetY;
        private int _unk0x4C;
        private int _unk0x50;
        private int mCoordX1;
        private int mCoordY1;
        private int mCoordX2;
        private int mCoordY2;
        private int _unk0x64;   // argb color
        private int _unk0x68;   // argb color
        private int _unk0x6C;   // argb color
        private int _unk0x70;   // argb color
        private int _unk0x74;   // possibly padding
        private int _unk0x78;   // possibly padding
        private int _unk0x7C;   // possibly padding

        public string Comment
        {
            get { return mComment; }
            set { mComment = value; }
        }

        public int TextureIndex
        {
            get { return mTextureIndex; }
            set { mTextureIndex = value; }
        }

        public int OffsetX
        {
            get { return mOffsetX; }
            set { mOffsetX = value;  }
        }

        public int OffsetY
        {
            get { return mOffsetY; }
            set { mOffsetY = value; }
        }

        public Rectangle Coordinates
        {
            get { return new Rectangle(mCoordX1, mCoordY1, mCoordX2 - mCoordX1, mCoordY2 - mCoordY1); }
            set { mCoordX1 = value.X; mCoordY1 = value.Y; mCoordX2 = value.Right; mCoordY2 = value.Bottom; }
        }

        public SprSprite(string path)
        {
            using (var reader = new BinaryReader(File.OpenRead(path)))
                Read(reader);
        }

        public SprSprite( Stream stream, bool leaveOpen )
        {
            using ( var reader = new BinaryReader( stream, Encoding.Default, leaveOpen ) )
                Read( reader );
        }

        // Constructors
        internal SprSprite(BinaryReader reader)
        {
            Read(reader);
        }

        public void Save(string path)
        {
            using (var writer = new BinaryWriter(File.Create(path)))
                Write(writer);
        }

        // Internal Methods
        internal void Write(BinaryWriter writer)
        {
            writer.Write(_unk0x00);
            writer.WriteCString(mComment, 16);
            writer.Write(mTextureIndex);
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
            writer.Write(mOffsetX);
            writer.Write(mOffsetY);
            writer.Write(_unk0x4C);
            writer.Write(_unk0x50);
            writer.Write(mCoordX1);
            writer.Write(mCoordY1);
            writer.Write(mCoordX2);
            writer.Write(mCoordY2);
            writer.Write(_unk0x64);
            writer.Write(_unk0x68);
            writer.Write(_unk0x6C);
            writer.Write(_unk0x70);
            writer.Write(_unk0x74);
            writer.Write(_unk0x78);
            writer.Write(_unk0x7C);
        }

        // Private Methods
        private void Read(BinaryReader reader)
        {
            _unk0x00 = reader.ReadInt32();
            mComment = reader.ReadCString(16);
            mTextureIndex = reader.ReadInt32();
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
            mOffsetX = reader.ReadInt32();
            mOffsetY = reader.ReadInt32();
            _unk0x4C = reader.ReadInt32();
            _unk0x50 = reader.ReadInt32();
            mCoordX1 = reader.ReadInt32();
            mCoordY1 = reader.ReadInt32();
            mCoordX2 = reader.ReadInt32();
            mCoordY2 = reader.ReadInt32();
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
