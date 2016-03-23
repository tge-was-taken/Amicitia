namespace AtlusLibSharp.Graphics.RenderWare
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
            : base(RWNodeType.Struct, parent)
        {
            _numUVAnims = uvAnimationCount;
        }

        internal RWUVAnimationDictionaryStruct(RWNodeFactory.RWNodeInfo header, BinaryReader reader)
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
