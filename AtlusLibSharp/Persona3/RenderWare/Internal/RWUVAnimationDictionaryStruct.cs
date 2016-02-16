namespace AtlusLibSharp.Persona3.RenderWare
{
    using System.IO;

    internal class RWUVAnimationDictionaryStruct : RWNode
    {
        private int _numUVAnims;

        public int UVAnimationCount
        {
            get { return _numUVAnims; }
            internal set { _numUVAnims = value; }
        }

        public RWUVAnimationDictionaryStruct(int uvAnimationCount, RWNode parent = null)
            : base(RWType.Struct, parent)
        {
            _numUVAnims = uvAnimationCount;
        }

        internal RWUVAnimationDictionaryStruct(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            _numUVAnims = reader.ReadInt32();
        }

        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(_numUVAnims);
        }
    }
}
