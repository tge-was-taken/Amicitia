using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    /// <summary>
    /// Holds internal data for an <see cref="RWMaterialList"/> instance.
    /// </summary>
    internal class RWMaterialListStruct : RWNode
    {
        private int _matCount;
        private int[] _matReferences;

        public int MaterialCount
        {
            get { return _matCount; }
        }

        public int[] MaterialReferences
        {
            get { return _matReferences; }
        }

        internal RWMaterialListStruct(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
                : base(header)
        {
            _matCount = reader.ReadInt32();
            _matReferences = new int[_matCount];
            for (int i = 0; i < _matCount; i++)
                _matReferences[i] = reader.ReadInt32();
        }

        internal RWMaterialListStruct(RWMaterialList list)
            : base(RWType.Struct, list)
        {
            _matCount = list.Materials.Length;
            _matReferences = new int[_matCount];
            for (int i = 0; i < _matCount; i++)
            {
                _matReferences[i] = -1;
            }
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(_matCount);
            for (int i = 0; i < _matCount; i++)
                writer.Write(_matReferences[i]);
        }
    }
}