using System.Collections.Generic;
using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWMaterialList : RWNode
    {
        private RWMaterialListStruct _struct;
        private RWMaterial[] _materials;

        public RWMaterialListStruct Struct
        {
            get { return _struct; }
            set
            {
                _struct = value;
                _struct.Parent = this;
            }
        }

        public RWMaterial[] Materials
        {
            get { return _materials; }
            set
            {
                _materials = value;
                for (int i = 0; i < _materials.Length; i++)
                {
                    _materials[i].Parent = this;
                }
            }
        }

        internal RWMaterialList(uint size, uint version, RWNode parent, BinaryReader reader)
                : base(RWType.MaterialList, size, version, parent)
        {
            Struct = ReadNode(reader, this) as RWMaterialListStruct;
            _materials = new RWMaterial[Struct.materialCount];
            for (int i = 0; i < Struct.materialCount; i++)
                _materials[i] = ReadNode(reader, this) as RWMaterial;
        }

        public RWMaterialList(string[] textureNames)
            : base(RWType.MaterialList)
        {
            _materials = new RWMaterial[textureNames.Length];
            for (int i = 0; i < textureNames.Length; i++)
                _materials[i] = new RWMaterial(textureNames[i]);

            Struct = new RWMaterialListStruct(this);
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            Struct = new RWMaterialListStruct(this);
            Struct.Write(writer);
            for (int i = 0; i < Struct.materialCount; i++)
                Materials[i].Write(writer);
        }
    }
}