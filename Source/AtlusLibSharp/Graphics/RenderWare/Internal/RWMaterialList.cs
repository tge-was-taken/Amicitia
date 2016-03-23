using System.Collections.Generic;
using System.IO;

namespace AtlusLibSharp.Graphics.RenderWare
{
    internal class RWMaterialList : RWNode
    {
        private RWMaterialListStruct _struct;
        private RWMaterial[] _materials;

        public int MaterialCount
        {
            get { return _materials.Length; }
        }

        public RWMaterial[] Materials
        {
            get { return _materials; }
            set
            {
                _materials = value;

                if (_materials == null)
                    return;

                for (int i = 0; i < _materials.Length; i++)
                {
                    _materials[i].Parent = this;
                }
            }
        }

        public RWMaterialList(IList<string> textureNames)
            : base(RWNodeType.MaterialList)
        {
            _materials = new RWMaterial[textureNames.Count];

            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i] = new RWMaterial(textureNames[i], this);
            }

            _struct = new RWMaterialListStruct(this);
        }

        internal RWMaterialList(RWNodeFactory.RWNodeInfo header, BinaryReader reader)
            : base(header)
        {
            _struct = RWNodeFactory.GetNode<RWMaterialListStruct>(this, reader);
            _materials = new RWMaterial[_struct.MaterialCount];

            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i] = RWNodeFactory.GetNode<RWMaterial>(this, reader);
            }
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            _struct.InternalWrite(writer);

            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i].InternalWrite(writer);
            }
        }
    }
}