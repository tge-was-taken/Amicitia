namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AtlusLibSharp.Utilities;
    using ManagedNvTriStrip;

    public enum RwPrimitiveType
    {
        TriangleList    = 0,
        TriangleStrip   = 1,
    }

    /// <summary>
    /// Represents a RenderWare node containing info regarding splits of the geometryNode's index buffer made by material.
    /// </summary>
    public class RwMeshListNode : RwNode
    {
        private RwPrimitiveType mPrimitiveType;
        private int mPrimitiveCount;
        private RwMesh[] mMeshes;

        /// <summary>
        /// Gets the <see cref="RwPrimitiveType"/> of the material splits. This indicates how the indices in each split form faces.
        /// </summary>
        public RwPrimitiveType PrimitiveType
        {
            get { return mPrimitiveType; }
        }

        /// <summary>
        /// Gets the number of material splits.
        /// </summary>
        public int MeshCount
        {
            get { return mMeshes.Length; }
        }

        /// <summary>
        /// Gets the total number of faces (note: not indices) of all splits combined.
        /// </summary>
        public int FaceCount
        {
            get { return mPrimitiveCount; }
        }

        /// <summary>
        /// Gets the material splits in the material split data.
        /// </summary>
        public RwMesh[] MaterialMeshes
        {
            get { return mMeshes; }
        }

        /// <summary>
        /// Initialize a new <see cref="RwMeshListNode"/> using a <see cref="RwGeometryNode"/> and the primitive id for the split data.
        /// </summary>
        /// <param name="geometryNode"></param>
        /// <param name="primitiveType"></param>
        public RwMeshListNode(RwGeometryNode geometryNode, RwPrimitiveType primitiveType = RwPrimitiveType.TriangleStrip, RwNode parent = null)
            : base(RwNodeId.RwMeshListNode, parent)
        {
            // set id and prim count
            mPrimitiveType = primitiveType;
            //mPrimitiveCount = geometryNode.TriangleCount;

            // pass 1: order the triangles by ascending material id
            var sortedTriangles = geometryNode.Triangles.OrderBy(tri => tri.MatId);

            // pass 2: split the indices
            List<ushort>[] matSplitsIndices = new List<ushort>[geometryNode.MaterialCount];
            List<ushort> curMatSplitIndices = null;
            int curMatIdx = -1;
            foreach (var tri in sortedTriangles)
            {
                if (tri.MatId != curMatIdx)
                {
                    if (curMatIdx != -1)
                        matSplitsIndices[curMatIdx] = curMatSplitIndices;

                    curMatIdx = tri.MatId;
                    curMatSplitIndices = new List<ushort>();
                }

                curMatSplitIndices.Add(tri.A);
                curMatSplitIndices.Add(tri.B);
                curMatSplitIndices.Add(tri.C);
            }

            matSplitsIndices[curMatIdx] = curMatSplitIndices;

            // pass 3: create the split data
            mMeshes = new RwMesh[geometryNode.MaterialCount];
            for (int i = 0; i < mMeshes.Length; i++)
            {
                ushort[] matSplitIndices = matSplitsIndices[i].ToArray();
                int triangleCount;

                if (primitiveType == RwPrimitiveType.TriangleStrip)
                {
                    if (NvTriStripUtility.GenerateStrips(matSplitIndices, out PrimitiveGroup[] primitives ) && primitives[0].Type == ManagedNvTriStrip.PrimitiveType.TriangleStrip)
                    {
                        matSplitIndices = primitives[0].Indices;
                        geometryNode.Flags |= RwGeometryFlags.CanTriStrip;
                        triangleCount = matSplitIndices.Length - 2;
                    }
                    else
                    {
                        mPrimitiveType = RwPrimitiveType.TriangleList;
                        triangleCount = matSplitIndices.Length / 3;
                        //throw new System.Exception("Failed to generate strips.");
                    }

                    /*
                    NvTriStripDotNet.PrimitiveGroup[] primitives;
                    var tristripper = new NvTriStripDotNet.NvTriStrip();
                    if (tristripper.GenerateStrips(matSplitIndices, out primitives))
                    {
                        matSplitIndices = primitives[0].indices.Cast<ushort>().ToArray();
                    }
                    */
                }
                else
                {
                    triangleCount = matSplitIndices.Length / 3;
                }

                mPrimitiveCount += triangleCount;
                mMeshes[i] = new RwMesh(i, matSplitIndices);
            }
        }

        // init with factory node info & binary reader
        internal RwMeshListNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            mPrimitiveType = (RwPrimitiveType)reader.ReadUInt32();
            int numSplits = reader.ReadInt32();
            mPrimitiveCount = reader.ReadInt32();

            mMeshes = new RwMesh[numSplits];
            for (int i = 0; i < numSplits; i++)
            {
                mMeshes[i] = new RwMesh(reader);
            }
        }

        /// <summary>
        /// Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write((uint)mPrimitiveType);
            writer.Write(MeshCount);
            writer.Write(mPrimitiveCount);

            for (int i = 0; i < mMeshes.Length; i++)
            {
                mMeshes[i].Write(writer);
            }
        }
    }

    /// <summary>
    /// Represents a portion of the geometryNode's index buffer split by material.
    /// </summary>
    public class RwMesh
    {
        private int mMatIndex;
        private int[] mIndices;

        /// <summary>
        /// Gets the material index of the split.
        /// </summary>
        public int MaterialIndex
        {
            get { return mMatIndex; }
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
            get { return mIndices; }
        }

        // init with mat index and index array
        internal RwMesh(int matIndex, int[] indices)
        {
            mMatIndex = matIndex;
            mIndices = indices;
        }

        // init with mat index and index array
        internal RwMesh(int matIndex, ushort[] indices)
        {
            mMatIndex = matIndex;
            mIndices = new int[indices.Length];
            for (int i = 0; i < mIndices.Length; i++)
            {
                mIndices[i] = indices[i];
            }
        }

        // init from binary reader
        internal RwMesh(BinaryReader reader)
        {
            int numIndices = reader.ReadInt32();
            mMatIndex = reader.ReadInt32();
            mIndices = reader.ReadInt32Array(numIndices);
        }

        // write with binary writer
        internal void Write(BinaryWriter writer)
        {
            writer.Write(mIndices.Length);
            writer.Write(mMatIndex);
            writer.Write(mIndices);
        }
    }
}
