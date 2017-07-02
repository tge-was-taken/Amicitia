using System.Collections.Generic;

namespace AtlusLibSharp.Graphics.RenderWare
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

            /*
            var runs = DetectRuns(mUsedBoneIndices);
            mRLECount = runs.Count;

            for (int i = 0; i < runs.Count; i++)
            {
                for (int j = 0; j < runs[i].SkinBoneIndexCount; j++)
                {
                    mMeshBoneRemapIndices[runs[i].BoneIndex + j] = (byte)j;
                }
            }

            mMeshBoneRLECount = new[]
                {new SkinSplitMeshRLECount {StartIndex = 0, Count = (byte)runs.Count }};

            mMeshBoneRLE = new SkinSplitMeshBoneRLE[mRLECount];
            for (int i = 0; i < mMeshBoneRLE.Length; i++)
                mMeshBoneRLE[i] = runs[i];
            */

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

        internal void PrintInfo()
        {
            int index = -1;
            using (StreamWriter writer = File.CreateText(GetUniqueName(ref index)))
            {
                WriteField(writer, nameof(BoneCount));
                WriteField(writer, nameof(UsedBoneCount));
                WriteField(writer, nameof(MaxWeightCountPerVertex));
                WriteFieldArray(writer, nameof(UsedBoneIndices));
                WriteFieldDoubleArray(writer, nameof(VertexBoneIndices));
                WriteFieldDoubleArray(writer, nameof(VertexBoneWeights));
                WriteFieldArray(writer, nameof(SkinToBoneMatrices));
                WriteField(writer, nameof(BoneLimit));
                WriteField(writer, nameof(MeshCount));
                WriteField(writer, nameof(RleCount));
                WriteFieldArray(writer, nameof(MeshBoneRemapIndices));

                WriteField(writer, nameof(MeshBoneRleCount));
                for (int i = 0; i < MeshBoneRleCount.Length; i++)
                {
                    writer.WriteLine("{0} = {1}", "m_usedBonesStartIndex", MeshBoneRleCount[i].StartIndex);
                    writer.WriteLine("{0} = {1}", "m_numUsedBones", MeshBoneRleCount[i].Count);
                }

                WriteField(writer, nameof(MeshBoneRle));
                for (int i = 0; i < MeshBoneRle.Length; i++)
                {
                    writer.WriteLine("{0} = {1}", "m_usedBoneHierarchyIndex", MeshBoneRle[i].BoneIndex);
                    writer.WriteLine("{0} = {1}", "m_unknown", MeshBoneRle[i].SkinBoneIndexCount);
                }
            }
        }

        private void WriteField(StreamWriter writer, string fieldName)
        {
            var property = GetType().GetProperty(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            writer.WriteLine("{0} = {1}", fieldName, property.GetValue(this));
        }

        private void WriteFieldArray(StreamWriter writer, string fieldname)
        {
            WriteField(writer, fieldname);
            var property = GetType().GetProperty(fieldname, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            dynamic array = property.GetValue(this);
            for (int i = 0; i < array.Length; i++)
                writer.WriteLine(array[i]);
        }

        private void WriteFieldDoubleArray(StreamWriter writer, string fieldname)
        {
            WriteField(writer, fieldname);
            var property = GetType().GetProperty(fieldname, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            dynamic array = property.GetValue(this);
            for (int i = 0; i < array.Length; i++)
                for (int j = 0; j < array[i].Length; j++)
                    writer.WriteLine(array[i][j]);
        }

        private string GetUniqueName(ref int index)
        {
            index++;
            string str = $"skinplugin_log_{index}.txt";
            if (File.Exists(str))
                return GetUniqueName(ref index);
            else
                return str;
        }

        //public RWSkinPlugin(RWSceneNodeList rwFrameList, RWGeometry rwGeometryNode, byte[][] vertexBoneIndices, float[][] vertexBoneWeights)
        //    : base(RWNodeType.SkinNode)
        //{
        //    RWSceneNodeBoneMetadata root = rwFrameList.HierarchyAnimRoot;
        //    _numBones = (byte)root.BoneHierarchyNodeCount;

        //    List<byte> usedBoneList = new List<byte>();
        //    _numWeightPerVertex = 0;

        //    for (int i = 0; i < vertexBoneIndices.Length; i++)
        //    {
        //        double wSum = 0.0f;
        //        int wUsed = 0;
        //        for (int j = 0; j < 4; j++)
        //        {
        //            if (vertexBoneWeights[i][j] != 0.0f)
        //            {
        //                ++wUsed;
        //                wSum += vertexBoneWeights[i][j];
        //                //vertexBoneIndices[i][j] = (byte)(DFSFrameList.FindIndex(f => f.Index == vertexBoneIndices[i][j]));

        //                if (!usedBoneList.Contains(vertexBoneIndices[i][j]))
        //                    usedBoneList.Add(vertexBoneIndices[i][j]);
        //            }
        //        }
        //        if (wSum < 1.0f)
        //        {
        //            double wRemainder = 1.0f - wSum;
        //            wRemainder /= wUsed;
        //            for (int j = 0; j < wUsed; j++)
        //                vertexBoneWeights[i][j] += (float)wRemainder;
        //        }
        //        if (wUsed > MaxWeightCountPerVertex)
        //            _numWeightPerVertex = (byte)wUsed;
        //    }

        //    _numUsedBones = (byte)usedBoneList.Count;
        //    _unused = 0;
        //    _usedBoneIndices = usedBoneList.ToArray();
        //    _skinBoneIndices = vertexBoneIndices;
        //    _skinBoneWeights = vertexBoneWeights;

        //    _inverseBoneMatrices = new Matrix4[BoneCount];
        //    for (int i = 0; i < BoneCount; i++)
        //    {
        //        SkinToBoneMatrices[i] = 
        //            (rwFrameList.GetFrameByHierarchyIndex(root.Nodes[i].Index).Transform * 
        //             rwFrameList.FrameListNode[2].WorldTransform)
        //             .Inverted(); // Get the drawCall root frame world matrix
        //    }

        //    /*
        //    BoneLimit = 64;
        //    MeshCount = (uint)rwGeometryNode.MaterialListNode.StructNode.materialCount;
        //    MaterialSplitUsedBones = UsedBoneCount;
        //    */
        //}

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