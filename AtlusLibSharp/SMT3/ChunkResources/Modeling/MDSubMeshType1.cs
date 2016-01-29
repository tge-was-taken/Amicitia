namespace AtlusLibSharp.SMT3.ChunkResources.Modeling
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using OpenTK;
    using PS2.Graphics;

    public class MDSubMeshType1 : IMDSubMesh
    {
        // Private fields
        private MDNode _node;
        private MDMaterial _material;
        private ushort _batchSize;
        private ushort _materialID;
        private int _batchOffset;
        private List<VIFPacket> _vifPackets;
        private List<MDSubMeshVifBatch> _batches;

        // Constructors
        internal MDSubMeshType1(MDChunk model, MDNode node, IntPtr ptr, BinaryReader reader)
        {
            _node = node;
            InternalRead(model, reader);
            _material = model.Materials[_materialID];
        }

        // Properties
        public int Type
        {
            get { return 1; }
        }

        public ushort MaterialID
        {
            get { return _materialID; }
        }

        public MDMaterial Material
        {
            get { return _material; }
        }

        public int UsedNodeCount
        {
            get { return 1; }
        }

        public ushort[] UsedNodeIndices
        {
            get { return new ushort[] { (ushort)_node.Index }; }
        }

        public MDNode[] UsedNodes
        {
            get { return new MDNode[] { _node }; }
        }

        public List<MDSubMeshVifBatch> Batches
        {
            get { return _batches; }
        }

        // Methods
        internal void InternalRead(MDChunk model, BinaryReader reader)
        {
            _batchSize = reader.ReadUInt16();
            _materialID = reader.ReadUInt16();
            _batchOffset = reader.ReadInt32();

            reader.BaseStream.Seek((uint)model.offset + MDChunk.DATA_START_ADDRESS + _batchOffset, SeekOrigin.Begin);
            _vifPackets = VIFCodeEvaluator.EvaluateBlock(reader, _batchSize);
            _batches = MDSubMeshVifBatch.ParseBatches(_vifPackets, UsedNodeCount);

            foreach (MDSubMeshVifBatch batch in _batches)
            {
                batch.TransformedPositions = new Vector3[batch.VertexCount];
                batch.TransformedNormals = new Vector3[batch.VertexCount];
                for (int i = 0; i < batch.VertexCount; i++)
                {
                    batch.TransformedPositions[i] = Vector3.Transform(batch.Positions[i], UsedNodes[batch.UsedNodeArrayIndex].WorldMatrix);
                    batch.TransformedNormals[i] = Vector3.TransformNormal(batch.Normals[i], UsedNodes[batch.UsedNodeArrayIndex].WorldMatrix);
                }
            }
        }
    }
}
