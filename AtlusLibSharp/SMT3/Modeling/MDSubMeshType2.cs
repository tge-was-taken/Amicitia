namespace AtlusLibSharp.SMT3.Modeling
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using OpenTK;
    using Common.Utilities;
    using PS2.Graphics;

    public class MDSubMeshType2 : IMDSubMesh
    {
        // Private fields
        private MDNode _node;
        private MDMaterial _material;
        private ushort _batchSize;
        private ushort _materialID;
        private int _batchOffset;
        private int _numUsedNodes;
        private ushort[] _usedNodeIndices;
        private MDNode[] _usedNodes;
        private List<VIFPacket> _vifPackets;
        private List<MDSubMeshVifBatch> _batches;

        // Constructors
        internal MDSubMeshType2(MDChunk model, MDNode node, IntPtr ptr, BinaryReader reader)
        {
            _node = node;
            InternalRead(model, reader);
            _material = model.Materials[_materialID];
        }

        // Properties
        public int Type
        {
            get { return 2; }
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
            get { return _numUsedNodes; }
        }

        public ushort[] UsedNodeIndices
        {
            get { return _usedNodeIndices; }
        }

        public MDNode[] UsedNodes
        {
            get { return _usedNodes; }
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
            _numUsedNodes = reader.ReadInt32();
            _usedNodeIndices = reader.ReadUInt16Array(_numUsedNodes);
            _usedNodes = new MDNode[_numUsedNodes];
            for (int i = 0; i < _numUsedNodes; i++)
            {
                _usedNodes[i] = model.Nodes[_usedNodeIndices[i]];
            } 

            reader.BaseStream.Seek((int)model.offset + MDChunk.DATA_START_ADDRESS + _batchOffset, SeekOrigin.Begin);
            _vifPackets = VIFCodeEvaluator.EvaluateBlock(reader, _batchSize);
            _batches = MDSubMeshVifBatch.ParseBatches(_vifPackets, _numUsedNodes);

            foreach (MDSubMeshVifBatch batch in _batches)
            {
                batch.TransformedPositions = new Vector3[batch.VertexCount];
                batch.TransformedNormals = new Vector3[batch.VertexCount];
                for (int i = 0; i < batch.VertexCount; i++)
                {
                    if (batch.Weights[i] != 0)
                    {
                        batch.TransformedPositions[i] = Vector3.Transform(batch.Positions[i], UsedNodes[batch.UsedNodeArrayIndex].WorldMatrix * batch.Weights[i]);
                    }
                    else
                    {
                        batch.TransformedPositions[i] = Vector3.Zero;
                    }

                    if (batch.Weights[i] != 0)
                    {
                        batch.TransformedNormals[i] = Vector3.TransformNormal(batch.Normals[i], UsedNodes[batch.UsedNodeArrayIndex].WorldMatrix * batch.Weights[i]);
                    }
                    else
                    {
                        batch.TransformedNormals[i] = Vector3.Zero;
                    }
                }
            }

            List<MDSubMeshVifBatch> fixedBatches = new List<MDSubMeshVifBatch>();
            for (int i = 0; i < _batches.Count; i+=(int)_numUsedNodes)
            {
                Vector3[] pos = new Vector3[_batches[i].VertexCount];
                Vector3[] nrm = new Vector3[_batches[i].VertexCount];
                for (int k = 0; k < _batches[i].VertexCount; k++)
                {
                    for (int j = 0; j < _numUsedNodes; j++)
                    {
                        pos[k] += _batches[i + j].TransformedPositions[k];
                        nrm[k] += _batches[i + j].TransformedNormals[k];
                    }
                }
                for (int k = 0; k < _batches[i].VertexCount; k++)
                {
                    nrm[k].Normalize();
                }
                /*
                SubMeshVifBatch finalBatch = _batches[(int)_numUsedNodes - 1];
                finalBatch.TransformedPositions = pos;
                finalBatch.TransformedNormals = pos;
                finalBatch.NodeIndices = 
                fixedBatches.Add(finalBatch);
                */
                for (int j = 0; j < _numUsedNodes; j++)
                {
                    _batches[i + j].TransformedPositions = pos;
                    _batches[i + j].TransformedNormals = nrm;
                    _batches[i + j].Colors = _batches[(int)_numUsedNodes - 1].Colors;
                    _batches[i + j].TextureCoords = _batches[(int)_numUsedNodes - 1].TextureCoords;
                }     
            }
        }
    }
}
