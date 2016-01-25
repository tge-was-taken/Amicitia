using System.Drawing;
using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWTextureNative : RWNode
    {
        private RWTextureNativeStruct _struct;
        private RWString _name;
        private RWString _maskName;
        private RWRaster _raster;
        private RWExtension _extension;

        public RWTextureNativeStruct Struct
        {
            get { return _struct; }
            set
            {
                _struct = value;
                _struct.Parent = this;
            }
        }

        public string Name
        {
            get { return _name.Value; }
            set
            {
                _name = new RWString(value);
                _name.Parent = this;
            }
        }

        public string MaskName
        {
            get { return _maskName.Value; }
            set
            {
                _maskName = new RWString(value);
                _maskName.Parent = this;
            }
        }

        public RWRaster Raster
        {
            get { return _raster; }
            set
            {
                _raster = value;
                _raster.Parent = this;
            }
        }

        public RWExtension Extension
        {
            get { return _extension; }
            set
            {
                _extension = value;
                _extension.Parent = this;
            }
        }

        public Bitmap Bitmap
        {
            get { return Raster.Data.Bitmap; }
        }

        internal RWTextureNative(uint size, uint version, RWNode parent, BinaryReader reader)
            : base(RWType.TextureNative, size, version, parent)
        {
            _struct = ReadNode(reader, this) as RWTextureNativeStruct;
            _name = ReadNode(reader, this) as RWString;
            _maskName = ReadNode(reader, this) as RWString;
            _raster = ReadNode(reader, this) as RWRaster;
            _extension = ReadNode(reader, this) as RWExtension;
            Bitmap.Save(Name + ".png");
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            _struct.Write(writer);
            _name.Write(writer);
            _maskName.Write(writer);
            _raster.Write(writer);
            _extension.Write(writer);
        }
    }
}