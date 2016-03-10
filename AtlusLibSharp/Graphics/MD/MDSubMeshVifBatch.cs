namespace AtlusLibSharp.SMT3.Modeling
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    using Common.Utilities;
    using OpenTK;
    using PS2.Graphics;

    public class MDSubMeshVifBatch
    {
        // Private fields
        private ushort _numFaces;
        private ushort _numVertices;
        private uint _flags;

        private byte[][] _triangleIndices;
        private Vector3[] _positions;
        private Vector3[] _normals;
        private Vector2[] _textureCoords;
        private Color[] _colors;
        private float[] _weights;
        private ushort _usedNodeArrayIndex;

        private ushort[] _nodeIndices;
        private Vector3[] _transformedPositions;
        private Vector3[] _transformedNormals;

        internal MDSubMeshVifBatch(ref int index, ushort nodeIndex, bool isLast, List<VIFPacket> packets)
        {
            SetHeader(ref index, nodeIndex, packets);

            for (int i = 0; i < 31; i++)
            {
                if (BitHelper.IsBitSet(_flags, i))
                {
                    switch (i)
                    {
                        case 21: // Triangle indices
                            SetTriangleIndices(ref index, packets);
                            break;

                        case 22: // Vertex Positions
                            SetVertexPositionsAndWeights(ref index, packets);
                            break;

                        case 23: // Vertex Normals
                            SetVertexNormals(ref index, packets);
                            break;

                        case 24: // Texture Coordinates
                            if (isLast)
                            {
                                SetTextureCoordinates(ref index, packets);
                            }
                            break;

                        case 25: // Vertex Colors
                            if (isLast)
                            {
                                SetVertexColors(ref index, packets);
                            }
                            break;
                    }
                }
            }

            // Some meshes don't store normals, so generate average normals in case we want to export it.
            if (_normals == null)
            {
                _normals = MeshUtilities.CalculateAverageNormals(_triangleIndices, _positions);
            }
        }

        public ushort FaceCount
        {
            get { return _numFaces; }
        }

        public ushort VertexCount
        {
            get { return _numVertices; }
        }

        public byte[][] FaceIndices
        {
            get { return _triangleIndices; }
        }

        public Vector3[] Positions
        {
            get { return _positions; }
        }

        public Vector3[] Normals
        {
            get { return _normals; }
        }

        public Vector2[] TextureCoords
        {
            get { return _textureCoords; }
            internal set { _textureCoords = value; }
        }

        public Color[] Colors
        {
            get { return _colors; }
            internal set { _colors = value; }
        }

        public float[] Weights
        {
            get { return _weights; }
            internal set { _weights = value; }
        }

        public ushort[] NodeIndices
        {
            get { return _nodeIndices; }
            internal set { _nodeIndices = value; }
        }

        public ushort UsedNodeArrayIndex
        {
            get { return _usedNodeArrayIndex; }
        }

        public Vector3[] TransformedPositions
        {
            get { return _transformedPositions; }
            internal set { _transformedPositions = value; }
        }

        public Vector3[] TransformedNormals
        {
            get { return _transformedNormals; }
            internal set { _transformedNormals = value; }
        }

        internal static List<MDSubMeshVifBatch> ParseBatches(List<VIFPacket> packets, int numUsedNodes)
        {
            List<MDSubMeshVifBatch> batches = new List<MDSubMeshVifBatch>();
            int idx = 0;
            while (idx < packets.Count - 1)
            {
                for (ushort i = 0; i < numUsedNodes; i++)
                {
                    if (idx == packets.Count)
                        break;
                    if (packets[idx].Command == VIFCommand.NoOperation)
                    {
                        idx++;
                        continue;
                    }

                    batches.Add(new MDSubMeshVifBatch(ref idx, i, i == (numUsedNodes-1), packets));
                    while (true)
                    {
                        VIFPacket packet = packets[idx++];
                        if (packet.Command == VIFCommand.FlushEnd || packet.Command == VIFCommand.Flush ||
                            packet.Command == VIFCommand.FlushAll || packet.Command == VIFCommand.ActMicro ||
                            packet.Command == VIFCommand.ActMicroF || packet.Command == VIFCommand.CntMicro)
                        {
                            break;
                        }
                    }
                }
            }

            return batches;
        }

        private void SetHeader(ref int index, ushort nodeIndex, List<VIFPacket> packets)
        {
            VIFUnpack headerUnpack = GetUnpack(ref index, packets);
            _numFaces = (ushort)headerUnpack.Elements[0][0];
            _numVertices = (ushort)headerUnpack.Elements[0][1];
            _flags = (uint)((ushort)headerUnpack.Elements[0][2] | (ushort)headerUnpack.Elements[0][3] << 16);
            _usedNodeArrayIndex = nodeIndex;
        }

        private void SetTriangleIndices(ref int index, List<VIFPacket> packets)
        {
            VIFUnpack unpack = GetUnpack(ref index, packets);
            _triangleIndices = ConvertArray<byte>(unpack.Elements);
        }

        private void SetVertexPositionsAndWeights(ref int index, List<VIFPacket> packets)
        {
            VIFUnpack unpack = GetUnpack(ref index, packets);
            float[][] floatArray = ConvertArray<float>(unpack.Elements);
            if (BitHelper.IsBitSet(_flags, 27))
            {
                _weights = new float[_numVertices];
                for (int j = 0; j < _numVertices; j++)
                {
                    _weights[j] = floatArray[j][3];
                }
            }

            _positions = new Vector3[_numVertices];
            for (int j = 0; j < _numVertices; j++)
            {
                _positions[j] = new Vector3(floatArray[j][0], floatArray[j][1], floatArray[j][2]);
            }
        }

        private void SetVertexNormals(ref int index, List<VIFPacket> packets)
        {
            VIFUnpack unpack = GetUnpack(ref index, packets);
            float[][] floatArray = ConvertArray<float>(unpack.Elements);
            _normals = new Vector3[_numVertices];
            for (int j = 0; j < _numVertices; j++)
            {
                _normals[j] = new Vector3(floatArray[j][0], floatArray[j][1], floatArray[j][2]);
            }
        }

        private void SetTextureCoordinates(ref int index, List<VIFPacket> packets)
        {
            VIFUnpack unpack = GetUnpack(ref index, packets);
            float[][] floatArray = ConvertArray<float>(unpack.Elements);
            _textureCoords = new Vector2[_numVertices];
            for (int j = 0; j < _numVertices; j++)
            {
                _textureCoords[j] = new Vector2(floatArray[j][0], floatArray[j][1]);
            }
        }

        private void SetVertexColors(ref int index, List<VIFPacket> packets)
        {
            VIFUnpack unpack = GetUnpack(ref index, packets);
            byte[][] byteArray = ConvertArray<byte>(unpack.Elements);
            _colors = new Color[_numVertices];
            for (int j = 0; j < _numVertices; j++)
            {
                _colors[j] = Color.FromArgb(byteArray[j][3], byteArray[j][0], byteArray[j][1], byteArray[j][2]);
            }
        }

        private T[][] ConvertArray<T>(object[][] array)
        {
            return Array.ConvertAll(array, item => Array.ConvertAll(item, subItem => (T)subItem));
        }

        private VIFUnpack GetUnpack(ref int index, List<VIFPacket> packets)
        {
            return packets[index++] as VIFUnpack;
        }
    }
}
