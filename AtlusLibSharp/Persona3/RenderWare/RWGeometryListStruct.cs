using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWGeometryListStruct : RWNode
    {
        public int GeometryCount { get; set; }

        internal RWGeometryListStruct(uint size, uint version, RWNode parent, BinaryReader reader)
                : base(RWType.Struct, size, version, parent)
        {
            GeometryCount = reader.ReadInt32();
        }

        internal RWGeometryListStruct(RWGeometryList list)
            : base(RWType.Struct)
        {
            GeometryCount = list.GeometryList.Count;
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write(GeometryCount);
        }
    }
}