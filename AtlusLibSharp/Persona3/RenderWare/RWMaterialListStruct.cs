using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWMaterialListStruct : RWNode
    {
        public int materialCount;
        public int[] materialReferences;

        internal RWMaterialListStruct(uint size, uint version, RWNode parent, BinaryReader reader)
                : base(RWType.Struct, size, version, parent)
        {
            materialCount = reader.ReadInt32();
            materialReferences = new int[materialCount];
            for (int i = 0; i < materialCount; i++)
                materialReferences[i] = reader.ReadInt32();
        }

        internal RWMaterialListStruct(RWMaterialList list)
            : base(RWType.Struct)
        {
            materialCount = list.Materials.Length;
            materialReferences = new int[materialCount];
            for (int i = 0; i < materialCount; i++)
            {
                materialReferences[i] = -1;
            }
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write(materialCount);
            for (int i = 0; i < materialCount; i++)
                writer.Write(materialReferences[i]);
        }
    }
}