using System.IO;
using PS2;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWRasterInfo : RWNode
    {
        private int _width;
        private int _height;
        private int _depth;
        private RWRasterFormats _rasterFormat;
        private PS2Tex0Register _tex0;
        private PS2Tex1Register _tex1;
        private PS2MipTBP1Register _mip1;
        private PS2MipTBP2Register _mip2;
        private uint _texelDataLength; // stream size of the mipmap data
        private uint _paletteDataLength; // stream size of the palette data (zero if no palette)
        private uint _gpuAlignedLength; // memory span of the texture mipmap data on the GS (aligned to pages/2048)
        private uint _skyMipMapValue;

        public int Width
        {
            get { return _width; }
        }

        public int Height
        {
            get { return _height; }
        }

        public int Depth
        {
            get { return _depth; }
        }

        public RWRasterFormats Format
        {
            get { return _rasterFormat; }
        }

        public PS2Tex0Register Tex0Register
        {
            get { return _tex0; }
        }

        public PS2Tex1Register Tex1Register
        {
            get { return _tex1; }
        }

        public PS2MipTBP1Register MipTBP1Register
        {
            get { return _mip1; }
        }

        public PS2MipTBP2Register MipTBP2Register
        {
            get { return _mip2; }
        }

        internal RWRasterInfo(uint size, uint version, RWNode parent, BinaryReader reader)
            : base(RWType.Struct, size, version, parent)
        {
            _width = reader.ReadInt32();
            _height = reader.ReadInt32();
            _depth = reader.ReadInt32();
            _rasterFormat = (RWRasterFormats)reader.ReadUInt32();
            _tex0 = new PS2Tex0Register(reader.ReadUInt64());
            _tex1 = new PS2Tex1Register(reader.ReadUInt64());
            _mip1 = new PS2MipTBP1Register(reader.ReadUInt64());
            _mip2 = new PS2MipTBP2Register(reader.ReadUInt64());
            _texelDataLength = reader.ReadUInt32();
            _paletteDataLength = reader.ReadUInt32();
            _gpuAlignedLength = reader.ReadUInt32();
            _skyMipMapValue = reader.ReadUInt32();
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write(_width);
            writer.Write(_height);
            writer.Write(_depth);
            writer.Write((uint)_rasterFormat);
            writer.Write(PS2Tex0Register.GetBytes(_tex0));
            writer.Write(PS2Tex1Register.GetBytes(_tex1));
            writer.Write(PS2MipTBP1Register.GetBytes(_mip1));
            writer.Write(PS2MipTBP2Register.GetBytes(_mip2));
            writer.Write(_texelDataLength);
            writer.Write(_paletteDataLength);
            writer.Write(_gpuAlignedLength);
            writer.Write(_skyMipMapValue);
        }
    }
}