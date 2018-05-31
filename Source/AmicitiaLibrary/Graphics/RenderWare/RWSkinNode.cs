using System.Collections.Generic;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.IO;
    using System.Numerics;
    using Utilities;
    using System.Reflection;
    using System;
    using System.Linq;

    // This class needs some major refactoring
    public class RwSkinNode : RwNode
    {
        public byte BoneCount { get; }

        public byte UsedBoneCount { get; }

        public byte MaxWeightCountPerVertex { get; }

        public byte[] UsedBoneIndices { get; }

        public byte[][] VertexBoneIndices { get; }

        public float[][] VertexBoneWeights { get; }

        public Matrix4x4[] SkinToBoneMatrices { get; }

        public int BoneLimit { get; }

        public int MeshCount { get; }

        public int RleCount { get; }

        public byte[] MeshBoneRemapIndices { get; }

        public SkinSplitMeshRleCount[] MeshBoneRleCount { get; }

        public SkinSplitMeshBoneRle[] MeshBoneRle { get; }

        // TODO:
        //
        // 1. Figure out how the bone remap indices work
        // They seem to be tied to the used bone indices somehow
        // Maybe building up an hierarchy as the same indices appear multiple times
        // Indices that aren't in the used bone indices are set to 0xFF
        //
        // 2. Figure out the BoneRLE
        // First byte is the hierarchy index of the affected bone
        // Second byte seems to be something like how many bones it shares weights with on the same material split?
        // 
        // 3. Figure out the inverse matrices
        // I can currently just copy and paste the one from the original file
        // But knowing how to calculate it would be a lot easier

        internal RwSkinNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader, RwGeometryNode rwGeometryNode)
            : base(header)
        {
            int numVertices = rwGeometryNode.VertexCount;

            BoneCount = reader.ReadByte();
            UsedBoneCount = reader.ReadByte();
            MaxWeightCountPerVertex = reader.ReadByte();
            reader.Seek(1, SeekOrigin.Current);

            UsedBoneIndices = reader.ReadBytes(UsedBoneCount);

            VertexBoneIndices = new byte[numVertices][];
            for (int i = 0; i < numVertices; i++)
            {
                VertexBoneIndices[i] = reader.ReadBytes(4);
            }

            VertexBoneWeights = new float[numVertices][];
            for (int i = 0; i < numVertices; i++)
            {
                VertexBoneWeights[i] = reader.ReadFloatArray(4);
            }

            SkinToBoneMatrices = new Matrix4x4[BoneCount];
            for (int i = 0; i < BoneCount; i++)
            {
                Matrix4x4 mtx = Matrix4x4.Identity;

                mtx.M11 = reader.ReadSingle(); mtx.M12 = reader.ReadSingle(); mtx.M13 = reader.ReadSingle(); reader.BaseStream.Position += 4;
                mtx.M21 = reader.ReadSingle(); mtx.M22 = reader.ReadSingle(); mtx.M23 = reader.ReadSingle(); reader.BaseStream.Position += 4;
                mtx.M31 = reader.ReadSingle(); mtx.M32 = reader.ReadSingle(); mtx.M33 = reader.ReadSingle(); reader.BaseStream.Position += 4;
                mtx.M41 = reader.ReadSingle(); mtx.M42 = reader.ReadSingle(); mtx.M43 = reader.ReadSingle(); reader.BaseStream.Position += 4;

                SkinToBoneMatrices[i] = mtx;
            }

            BoneLimit = reader.ReadInt32();
            MeshCount = reader.ReadInt32();
            RleCount = reader.ReadInt32();

            if (MeshCount < 1)
                return;

            MeshBoneRemapIndices = reader.ReadBytes(BoneCount);

            MeshBoneRleCount = new SkinSplitMeshRleCount[MeshCount];
            for (int i = 0; i < MeshCount; i++)
                MeshBoneRleCount[i] = new SkinSplitMeshRleCount { StartIndex = reader.ReadByte(), Count = reader.ReadByte() };

            MeshBoneRle = new SkinSplitMeshBoneRle[RleCount];
            for (int i = 0; i < RleCount; i++)
                MeshBoneRle[i] = new SkinSplitMeshBoneRle { BoneIndex = reader.ReadByte(), SkinBoneIndexCount = reader.ReadByte() };

            //PrintInfo();
        }

        public RwSkinNode(byte[][] vertexBoneIndices, float[][] vertexBoneWeights, Matrix4x4[] skinToBoneMatrices)
            : base(RwNodeId.RwSkinNode)
        {
            BoneCount = (byte)skinToBoneMatrices.Length;

            var uniqueSkinBoneIndices = new List<byte>();
            MaxWeightCountPerVertex = 0;

            for (int i = 0; i < vertexBoneIndices.Length; i++)
            {
                for (int j = 0; j < vertexBoneIndices[i].Length; j++)
                {
                    if (vertexBoneWeights[i][j] != 0.0f)
                    {
                        if (!uniqueSkinBoneIndices.Contains(vertexBoneIndices[i][j]))
                            uniqueSkinBoneIndices.Add(vertexBoneIndices[i][j]);

                        if ((j + 1) > MaxWeightCountPerVertex)
                            MaxWeightCountPerVertex = (byte)(j + 1);
                    }
                }   
            }

            UsedBoneCount = (byte)uniqueSkinBoneIndices.Count;
            UsedBoneIndices = uniqueSkinBoneIndices.ToArray();
            VertexBoneIndices = vertexBoneIndices;
            VertexBoneWeights = vertexBoneWeights;
            SkinToBoneMatrices = skinToBoneMatrices;

            BoneLimit = 64;
            MeshCount = 1;

            // these are offsets relative to the BoneIndex
            MeshBoneRemapIndices = new byte[BoneCount];
            for (int i = 0; i < MeshBoneRemapIndices.Length; i++)
                MeshBoneRemapIndices[i] = 0xFF;

            RleCount = UsedBoneCount;

            for (int i = 0; i < UsedBoneIndices.Length; i++)
                MeshBoneRemapIndices[UsedBoneIndices[i]] = (byte)i;

            MeshBoneRleCount = new[] { new SkinSplitMeshRleCount { StartIndex = 0, Count = UsedBoneCount } };

            MeshBoneRle = new SkinSplitMeshBoneRle[UsedBoneCount];
            for (int i = 0; i < MeshBoneRle.Length; i++)
            {
                MeshBoneRle[i] = new SkinSplitMeshBoneRle
                {
                    BoneIndex = UsedBoneIndices[i],
                    SkinBoneIndexCount = 1
                };
            }
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write(BoneCount);
            writer.Write(UsedBoneCount);
            writer.Write(MaxWeightCountPerVertex);
            writer.Write((byte)0);
            writer.Write(UsedBoneIndices);

            for (int i = 0; i < VertexBoneIndices.Length; i++)
            {
                writer.Write(VertexBoneIndices[i]);
            }

            for (int i = 0; i < VertexBoneWeights.Length; i++)
            {
                writer.Write(VertexBoneWeights[i]);
            }

            for (int i = 0; i < BoneCount; i++)
            {
                writer.Write(SkinToBoneMatrices[i].M11); writer.Write(SkinToBoneMatrices[i].M12); writer.Write(SkinToBoneMatrices[i].M13); writer.Write(0);
                writer.Write(SkinToBoneMatrices[i].M21); writer.Write(SkinToBoneMatrices[i].M22); writer.Write(SkinToBoneMatrices[i].M23); writer.Write(0);
                writer.Write(SkinToBoneMatrices[i].M31); writer.Write(SkinToBoneMatrices[i].M32); writer.Write(SkinToBoneMatrices[i].M33); writer.Write(0);
                writer.Write(SkinToBoneMatrices[i].M41); writer.Write(SkinToBoneMatrices[i].M42); writer.Write(SkinToBoneMatrices[i].M43); writer.Write(0);
            }

            writer.Write(BoneLimit);
            writer.Write(MeshCount);
            writer.Write(RleCount);

            if (MeshCount < 1)
            {
                return;
            }

            writer.Write(MeshBoneRemapIndices);

            for (int i = 0; i < MeshCount; i++)
            {
                writer.Write(MeshBoneRleCount[i].StartIndex);
                writer.Write(MeshBoneRleCount[i].Count);
            }

            for (int i = 0; i < RleCount; i++)
            {
                writer.Write(MeshBoneRle[i].BoneIndex);
                writer.Write(MeshBoneRle[i].SkinBoneIndexCount);
            }
        }
    }

    public class SkinSplitMeshRleCount
    {
        public byte StartIndex { get; set; }

        public byte Count { get; set; }
    }

    public class SkinSplitMeshBoneRle
    {
        public byte BoneIndex { get; set; }

        public byte SkinBoneIndexCount { get; set; } // something related to num of influences?
    }
}