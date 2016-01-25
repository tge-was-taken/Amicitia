using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWRaster : RWNode
    {
        private RWRasterInfo _rasterInfo;
        private RWRasterData _rasterData;

        public RWRasterInfo Info
        {
            get { return _rasterInfo; }
            set
            {
                _rasterInfo = value;
                _rasterInfo.Parent = this;
            }
        }

        public RWRasterData Data
        {
            get { return _rasterData; }
            set
            {
                _rasterData = value;
                _rasterData.Parent = this;
            }
        }

        internal RWRaster(uint size, uint version, RWNode parent, BinaryReader reader)
            : base(RWType.Struct, size, version, parent)
        {
            _rasterInfo = ReadNode(reader, this) as RWRasterInfo;
            _rasterData = ReadNode(reader, this) as RWRasterData;
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            _rasterInfo.Write(writer);
            _rasterData.Write(writer);
        }
    }
}