namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;

    internal class RWGeometryListStruct : RWNode
    {
        private int _geometryCount;

        public int GeometryCount
        {
            get { return _geometryCount; }
            internal set { _geometryCount = value; }
        }

        internal RWGeometryListStruct(RWNodeFactory.RWNodeInfo header, BinaryReader reader)
                : base(header)
        {
            _geometryCount = reader.ReadInt32();
        }

        internal RWGeometryListStruct(RWMeshList list)
            : base(RWNodeType.Struct, list)
        {
            _geometryCount = list.Meshes.Count;
        }

        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(_geometryCount);
        }
    }
}