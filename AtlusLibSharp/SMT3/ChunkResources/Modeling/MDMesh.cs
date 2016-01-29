namespace AtlusLibSharp.SMT3.ChunkResources.Modeling
{
    using System.IO;

    public class MDMesh
    {
        // Private Fields
        private uint _subMeshPointerTableOffset;
        private ushort _numSubmeshes;
        private ushort _numUnknown;
        private uint[] _subMeshPointerArray;
        private IMDSubMesh[] _subMeshes;

        // Constructors
        internal MDMesh(MDChunk model, MDNode node, BinaryReader reader)
        {
            Read(model, node, reader);
        }

        // Properties
        public ushort SubMeshCount
        {
            get { return _numSubmeshes; }
        }

        public IMDSubMesh[] SubMeshes
        {
            get { return _subMeshes; }
        }

        // Private Methods
        private void Read(MDChunk model, MDNode node, BinaryReader reader)
        {
            _subMeshPointerTableOffset = reader.ReadUInt32();
            if (_subMeshPointerTableOffset == 0)
            {
                return;
            }

            reader.BaseStream.Seek((uint)model.offset + MDChunk.DATA_START_ADDRESS + _subMeshPointerTableOffset, SeekOrigin.Begin);
            _numSubmeshes = reader.ReadUInt16();
            _numUnknown = reader.ReadUInt16();
            _subMeshPointerArray = new uint[_numSubmeshes];

            if (_numSubmeshes == 0 && _numUnknown == 0)
            {
                return;
            }

            for (int i = 0; i < _numSubmeshes; i++)
            {
                _subMeshPointerArray[i] = reader.ReadUInt32();
            }

            _subMeshes = new IMDSubMesh[_numSubmeshes];
            for (int i = 0; i < _numSubmeshes; i++)
            {
                reader.BaseStream.Seek((uint)model.offset + MDChunk.DATA_START_ADDRESS + _subMeshPointerArray[i], SeekOrigin.Begin);
                _subMeshes[i] = MDSubMeshFactory.Get(model, node, reader);
            }
        }
    }
}