using System.Collections.Generic;
using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWGeometryList : RWNode
    {
        private RWGeometryListStruct _struct;
        private List<RWGeometry> _geometryList;

        public RWGeometryListStruct Struct
        {
            get { return _struct; }
            set
            {
                _struct = value;
                _struct.Parent = this;
            }
        }

        public List<RWGeometry> GeometryList
        {
            get { return _geometryList; }
            set
            {
                _geometryList = value;
                for (int i = 0; i < _geometryList.Count; i++)
                {
                    _geometryList[i].Parent = this;
                }
            }
        }

        internal RWGeometryList(uint size, uint version, RWNode parent, BinaryReader reader)
                : base(RWType.GeometryList, size, version, parent)
        {
            Struct = ReadNode(reader, this) as RWGeometryListStruct;
            GeometryList = new List<RWGeometry>(Struct.GeometryCount);
            for (int i = 0; i < Struct.GeometryCount; i++)
                GeometryList.Add(ReadNode(reader, this) as RWGeometry);
        }

        public RWGeometryList()
            : base(RWType.GeometryList)
        {
        }

        public RWGeometryList(List<RWGeometry> geoList)
            : base(RWType.GeometryList)
        {
            GeometryList = geoList;
            Struct = new RWGeometryListStruct(this);
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            Struct = new RWGeometryListStruct(this);
            Struct.Write(writer);
            for (int i = 0; i < Struct.GeometryCount; i++)
                GeometryList[i].Write(writer);
        }
    }
}
