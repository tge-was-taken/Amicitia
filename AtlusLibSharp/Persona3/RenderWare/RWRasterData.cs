using System;
using System.Drawing;
using System.IO;
using PS2;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWRasterData : RWNode
    {
        private PS2ImageHeader _imageHeader;
        private byte[] _indices;
        private PS2ImageHeader _paletteHeader;
        private Color[] _palette;
        private byte[] _mipMapData;

        public Bitmap Bitmap
        {
            get;
            private set;
        }

        internal RWRasterData(uint size, uint version, RWNode parent, BinaryReader reader)
            : base(RWType.Struct, size, version, parent)
        {
            long start = reader.BaseStream.Position;

            RWRasterInfo info = GetInfo();

            if ((info.Format & RWRasterFormats.HasHeaders) == RWRasterFormats.HasHeaders)
                _imageHeader = PS2ImageHeader.ReadData(reader);
            ReadIndices(reader);

            if ((info.Format & RWRasterFormats.HasHeaders) == RWRasterFormats.HasHeaders)
                _paletteHeader = PS2ImageHeader.ReadData(reader);
            ReadPalette(reader);

            long end = reader.BaseStream.Position;

            _mipMapData = reader.ReadBytes((int)((start + Size) - end));

            reader.BaseStream.Position = start + Size;

            Bitmap = BitmapHelper.Create(_palette, _indices, info.Width, info.Height);
        }

        private RWRasterInfo GetInfo()
        {
            return (Parent as RWRaster).Info;
        }

        private void ReadIndices(BinaryReader reader)
        {
            byte[] tmp;
            RWRasterInfo info = GetInfo();

            switch (info.Tex0Register.TexturePixelFormat)
            {
                case PS2PixelFormat.PSMT8H:
                case PS2PixelFormat.PSMT8:
                    PS2ImageHelper.ReadPSMT8(reader, info.Width, info.Height, out tmp);
                    PS2ImageHelper.UnSwizzle8(info.Width, info.Height, tmp, out _indices);
                    break;
                case PS2PixelFormat.PSMT4HL:
                case PS2PixelFormat.PSMT4HH:
                case PS2PixelFormat.PSMT4:
                    PS2ImageHelper.ReadPSMT4(reader, info.Width, info.Height, out tmp);
                    PS2ImageHelper.UnSwizzle8(info.Width, info.Height, tmp, out _indices);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void WriteIndices(BinaryWriter writer)
        {
            byte[] tmp;
            RWRasterInfo info = GetInfo();

            switch (info.Tex0Register.TexturePixelFormat)
            {
                case PS2PixelFormat.PSMT8H:
                case PS2PixelFormat.PSMT8:
                    PS2ImageHelper.Swizzle8(info.Width, info.Height, _indices, out tmp);
                    PS2ImageHelper.WritePSMT8(writer, tmp);
                    break;
                case PS2PixelFormat.PSMT4HL:
                case PS2PixelFormat.PSMT4HH:
                case PS2PixelFormat.PSMT4:
                    PS2ImageHelper.Swizzle8(info.Width, info.Height, _indices, out tmp);
                    PS2ImageHelper.WritePSMT4(writer, tmp);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void ReadPalette(BinaryReader reader)
        {
            Color[] tmp = null;
            int paletteWidth = 16;
            int paletteHeight = 16;
            RWRasterInfo info = GetInfo();

            if (info.Tex0Register.TexturePixelFormat == PS2PixelFormat.PSMT4 ||
                info.Tex0Register.TexturePixelFormat == PS2PixelFormat.PSMT4HH ||
                info.Tex0Register.TexturePixelFormat == PS2PixelFormat.PSMT4HL)
                paletteWidth = paletteHeight = 4;

            switch (info.Tex0Register.CLUTPixelFormat)
            {
                case PS2PixelFormat.PSMZ32:
                case PS2PixelFormat.PSMCT32:
                    PS2ImageHelper.ReadPSMCT32(reader, paletteWidth, paletteHeight, out tmp);
                    break;
                case PS2PixelFormat.PSMZ24:
                case PS2PixelFormat.PSMCT24:
                    PS2ImageHelper.ReadPSMCT24(reader, paletteWidth, paletteHeight, out tmp);
                    break;
                case PS2PixelFormat.PSMZ16:
                case PS2PixelFormat.PSMCT16:
                    PS2ImageHelper.ReadPSMCT16(reader, paletteWidth, paletteHeight, out tmp);
                    break;
                case PS2PixelFormat.PSMZ16S:
                case PS2PixelFormat.PSMCT16S:
                    PS2ImageHelper.ReadPSMCT16S(reader, paletteWidth, paletteHeight, out tmp);
                    break;
                default:
                    throw new NotImplementedException();
            }
            if (paletteWidth > 4 && paletteHeight > 4)
            {
                PS2ImageHelper.TilePalette(tmp, out _palette);
            }
            else
            {
                _palette = tmp;
            }
        }

        private void WritePalette(BinaryWriter writer)
        {
            Color[] tmp = _palette;
            int paletteWidth = 16;
            int paletteHeight = 16;
            RWRasterInfo info = GetInfo();

            if (info.Tex0Register.TexturePixelFormat == PS2PixelFormat.PSMT4 ||
                info.Tex0Register.TexturePixelFormat == PS2PixelFormat.PSMT4HH ||
                info.Tex0Register.TexturePixelFormat == PS2PixelFormat.PSMT4HL)
                paletteWidth = paletteHeight = 4;

            if (paletteWidth > 4 && paletteHeight > 4)
            {
                PS2ImageHelper.TilePalette(_palette, out tmp);
            }

            switch (info.Tex0Register.CLUTPixelFormat)
            {
                case PS2PixelFormat.PSMZ32:
                case PS2PixelFormat.PSMCT32:
                    PS2ImageHelper.WritePSMCT32(writer, tmp);
                    break;
                case PS2PixelFormat.PSMZ24:
                case PS2PixelFormat.PSMCT24:
                    PS2ImageHelper.WritePSMCT24(writer, tmp);
                    break;
                case PS2PixelFormat.PSMZ16:
                case PS2PixelFormat.PSMCT16:
                    PS2ImageHelper.WritePSMCT16(writer, tmp);
                    break;
                case PS2PixelFormat.PSMZ16S:
                case PS2PixelFormat.PSMCT16S:
                    PS2ImageHelper.WritePSMCT16S(writer, tmp);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            RWRasterInfo info = GetInfo();
            if ((info.Format & RWRasterFormats.HasHeaders) == RWRasterFormats.HasHeaders)
                _imageHeader.WriteData(writer);
            WriteIndices(writer);

            if ((info.Format & RWRasterFormats.HasHeaders) == RWRasterFormats.HasHeaders)
                _paletteHeader.WriteData(writer);
            WritePalette(writer);

            writer.Write(_mipMapData);
        }
    }
}