using System.IO;

namespace AtlusLibSharp.Graphics.RenderWare
{
    internal class RWGeometryListStruct : RWNode
    {
        private int _geometryCount;

        public int GeometryCount
        {
            get { return _geometryCount; }
            internal set { _geometryCount = value; }
        }

        internal RWGeometryListStruct(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
                : base(header)
        {
            _geometryCount = reader.ReadInt32();
        }

        internal RWGeometryListStruct(RWGeometryList list)
            : base(RWType.Struct, list)
        {
            _geometryCount = list.GeometryList.Count;
        }

        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(_geometryCount);
        }
    }
}