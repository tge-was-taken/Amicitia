namespace AtlusLibSharp.SMT3.ChunkResources.Modeling
{
    using System;
    using System.IO;

    using Utilities;

    public class MDChunk : Chunk
    {
        // Internal Constants
        internal const int    MD00_FLAG = 0x0006;
        internal const string MD00_TAG = "MD00";
        internal const int    MD00_DATA_START_ADDRESS = 0x20;

        // Private fields
        internal int offset;
        private uint _unused;
        private int _addressRelocTableOffset;
        private int _addressRelocTableSize;
        private uint _unk1;
        private uint _unk2;
        private uint _nodeArrayOffset;
        private uint _materialArrayOffset;
        private uint _unk3;
        private uint _nodeNameSectionOffset;

        private uint _numNodes;
        private MDNode[] _nodes;
        private uint _numMaterials;
        private MDMaterial[] _materials;
        private NDNM _nodeNameSection;

        // Constructors
        internal MDChunk(ushort id, int length, BinaryReader reader)
            : base(MD00_FLAG, id, length, MD00_TAG)
        {
            Read(reader);
        }

        // Properties
        public uint NodeCount
        {
            get { return _numNodes; }
        }

        public MDNode[] Nodes
        {
            get { return _nodes; }
        }

        public uint MaterialCount
        {
            get { return _numMaterials; }
        }

        public MDMaterial[] Materials
        {
            get { return _materials; }
        }

        internal NDNM NodeNameSection
        {
            get { return _nodeNameSection; }
        }

        // Internal Methods
        internal override void InternalWrite(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        // Private Methods
        private void Read(BinaryReader reader)
        {
            offset = (int)reader.BaseStream.Position - CHUNK_HEADER_SIZE; 
            _unused = reader.ReadUInt32();
            _addressRelocTableOffset = reader.ReadInt32();
            _addressRelocTableSize = reader.ReadInt32();
            _unk1 = reader.ReadUInt32();
            _unk2 = reader.ReadUInt32();
            _nodeArrayOffset = reader.ReadUInt32();
            _materialArrayOffset = reader.ReadUInt32();
            _unk3 = reader.ReadUInt32();
            _nodeNameSectionOffset = reader.ReadUInt32();

            reader.BaseStream.Seek(offset + MD00_DATA_START_ADDRESS + _nodeArrayOffset, SeekOrigin.Begin);
            _numNodes = reader.ReadUInt32();
            reader.AlignPosition(16);
            long nodeArrayPos = reader.BaseStream.Position;

            reader.BaseStream.Seek(offset + MD00_DATA_START_ADDRESS + _materialArrayOffset, SeekOrigin.Begin);
            _numMaterials = reader.ReadUInt32();
            _materials = new MDMaterial[_numMaterials];
            for (int i = 0; i < _numMaterials; i++)
            {
                _materials[i] = new MDMaterial(reader);
            }

            if (_nodeNameSectionOffset != 0)
            {
                reader.BaseStream.Seek(offset + MD00_DATA_START_ADDRESS + _nodeNameSectionOffset, SeekOrigin.Begin);
                _nodeNameSection = new NDNM(_numNodes, reader);
            }

            _nodes = new MDNode[_numNodes];
            for (int i = 0; i < _numNodes; i++)
            {
                reader.BaseStream.Seek(nodeArrayPos + (i * MDNode.MD00_NODE_SIZE), SeekOrigin.Begin);
                _nodes[i] = new MDNode(this, reader);
            }

            for (int i = 0; i < _numNodes; i++)
            {
                _nodes[i].ReadMeshes(this, reader);
            }
        }
    }
}
