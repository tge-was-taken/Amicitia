using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AtlusLibSharp.Graphics.RenderWare
{
    internal class RWMeshList : RWNode
    {
        private RWGeometryListStruct _struct;
        private List<RWMesh> _geometryList;

        public int GeometryCount
        {
            get { return _geometryList.Count; }
        }

        public List<RWMesh> Meshes
        {
            get { return _geometryList; }
            set
            {
                _geometryList = value;

                if (_geometryList == null)
                    return;

                for (int i = 0; i < _geometryList.Count; i++)
                {
                    _geometryList[i].Parent = this;
                }
            }
        }

        public RWMeshList(IList<RWMesh> geoList, RWNode parent = null)
            : base(RWNodeType.GeometryList, parent)
        {
            Meshes = geoList.ToList();
            _struct = new RWGeometryListStruct(this);
        }

        internal RWMeshList(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
                : base(header)
        {
            _struct = RWNodeFactory.GetNode<RWGeometryListStruct>(this, reader);
            _geometryList = new List<RWMesh>(_struct.GeometryCount);

            for (int i = 0; i < _struct.GeometryCount; i++)
            {
                _geometryList.Add(RWNodeFactory.GetNode<RWMesh>(this, reader));
            }
        }

        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            // Update the number of geometries
            _struct.GeometryCount = GeometryCount;

            // Write struct
            _struct.InternalWrite(writer);

            // Write geometries
            for (int i = 0; i < _geometryList.Count; i++)
            { 
                _geometryList[i].InternalWrite(writer);
            }
        }
    }
}
