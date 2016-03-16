namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using NvTriStrip;

    public enum RWPrimitiveType
    {
        TriangleList    = 0,
        TriangleStrip   = 1,
    }

    /// <summary>
    /// Represents a RenderWare node containing info regarding splits of the mesh's index buffer made by material.
    /// </summary>
    public class RWMeshMaterialSplitData : RWNode
    {
        private RWPrimitiveType _primType;
        private int _numPrimitives;
        private RWMeshMaterialSplit[] _splits;

        /// <summary>
        /// Gets the <see cref="RWPrimitiveType"/> of the material splits. This indicates how the indices in each split form faces.
        /// </summary>
        public RWPrimitiveType PrimitiveType
        {
            get { return _primType; }
        }

        /// <summary>
        /// Gets the number of material splits.
        /// </summary>
        public int MaterialSplitCount
        {
            get { return _splits.Length; }
        }

        /// <summary>
        /// Gets the total number of faces (note: not indices) of all splits combined.
        /// </summary>
        public int FaceCount
        {
            get { return _numPrimitives; }
        }

        /// <summary>
        /// Gets the material splits in the material split data.
        /// </summary>
        public RWMeshMaterialSplit[] MaterialSplits
        {
            get { return _splits; }
        }

        /// <summary>
        /// Initialize a new <see cref="RWMeshMaterialSplitData"/> using a <see cref="RWMesh"/> and the primitive type for the split data.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="primType"></param>
        public RWMeshMaterialSplitData(RWMesh mesh, RWPrimitiveType primType = RWPrimitiveType.TriangleStrip, RWNode parent = null)
            : base(RWNodeType.MeshMaterialSplitList, parent)
        {
            // set type and prim count
            _primType = primType;
            _numPrimitives = mesh.TriangleCount;

            // pass 1: order the triangles by ascending material id
            var sortedTriangles = mesh.Triangles.OrderBy(tri => tri.MatID).ToArray();

            // pass 2: split the indices
            List<ushort>[] matSplitsIndices = new List<ushort>[mesh.MaterialCount];
            List<ushort> curMatSplitIndices = null;
            int curMatIdx = -1;
            for (int i = 0; i < sortedTriangles.Length; i++)
            {
                var tri = sortedTriangles[i];

                if (tri.MatID > curMatIdx)
                {
                    if (curMatIdx != -1)
                        matSplitsIndices[curMatIdx] = curMatSplitIndices;

                    curMatIdx = tri.MatID;
                    curMatSplitIndices = new List<ushort>();
                }

                curMatSplitIndices.Add(tri.A);
                curMatSplitIndices.Add(tri.B);
                curMatSplitIndices.Add(tri.C);
            }

            matSplitsIndices[curMatIdx] = curMatSplitIndices;

            // pass 3: create the split data
            _splits = new RWMeshMaterialSplit[mesh.MaterialCount];
            for (int i = 0; i < _splits.Length; i++)
            {
                ushort[] matSplitIndices = matSplitsIndices[i].ToArray();

                if (primType == RWPrimitiveType.TriangleStrip)
                    matSplitIndices = NvTriStrip.GenerateStrips(matSplitIndices);

                _splits[i] = new RWMeshMaterialSplit(i, matSplitIndices);
            }
        }

        // init with factory node info & binary reader
        internal RWMeshMaterialSplitData(RWNodeFactory.RWNodeInfo header, BinaryReader reader)
            : base(header)
        {
            _primType = (RWPrimitiveType)reader.ReadUInt32();
            int numSplits = reader.ReadInt32();
            _numPrimitives = reader.ReadInt32();

            _splits = new RWMeshMaterialSplit[numSplits];
            for (int i = 0; i < numSplits; i++)
            {
                _splits[i] = new RWMeshMaterialSplit(reader);
            }
        }

        /// <summary>
        /// Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write((uint)_primType);
            writer.Write(MaterialSplitCount);
            writer.Write(_numPrimitives);

            for (int i = 0; i < _splits.Length; i++)
            {
                _splits[i].InternalWrite(writer);
            }
        }
    }

    /// <summary>
    /// Represents a portion of the mesh's index buffer split by material.
    /// </summary>
    public class RWMeshMaterialSplit
    {
        private int _matIndex;
        private int[] _indices;

        /// <summary>
        /// Gets the material index of the split.
        /// </summary>
        public int MaterialIndex
        {
            get { return _matIndex; }
        }

        /// <summary>
        /// Gets the number of face indices of the split.
        /// </summary>
        public int IndexCount
        {
            get { return Indices.Length; }
        }

        /// <summary>
        /// Gets the face indices of the split.
        /// </summary>
        public int[] Indices
        {
            get { return _indices; }
        }

        // init with mat index and index array
        internal RWMeshMaterialSplit(int matIndex, int[] indices)
        {
            _matIndex = matIndex;
            _indices = indices;
        }

        // init with mat index and index array
        internal RWMeshMaterialSplit(int matIndex, ushort[] indices)
        {
            _matIndex = matIndex;
            _indices = new int[indices.Length];
            for (int i = 0; i < _indices.Length; i++)
            {
                _indices[i] = indices[i];
            }
        }

        // init from binary reader
        internal RWMeshMaterialSplit(BinaryReader reader)
        {
            int numIndices = reader.ReadInt32();
            _matIndex = reader.ReadInt32();

            _indices = new int[numIndices];
            for (int i = 0; i < _indices.Length; i++)
            {
                _indices[i] = reader.ReadInt32();
            }
        }

        // write with binary writer
        internal void InternalWrite(BinaryWriter writer)
        {
            writer.Write(_indices.Length);
            writer.Write(_matIndex);

            for (int i = 0; i < _indices.Length; i++)
            {
                writer.Write(_indices[i]);
            }
        }
    }
}
