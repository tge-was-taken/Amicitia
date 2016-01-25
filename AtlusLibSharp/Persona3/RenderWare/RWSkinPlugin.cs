using OpenTK;
using System.Collections.Generic;
using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWSkinPlugin : RWNode
    {
        protected RWGeometry Geometry { get; private set; }
        public byte BoneCount { get; private set; }
        public byte UsedBoneCount { get; private set; }
        public byte MaxWeightPerVertex { get; private set; }
        public byte Unused { get; private set; }
        public byte[] UsedBones { get; private set; }
        public byte[][] SkinBoneIndices { get; private set; }
        public float[][] SkinBoneWeights { get; private set; }
        public Matrix4[] InverseMatrices { get; private set; }
        public uint BoneLimit { get; private set; }
        public uint MaterialSplitCount { get; private set; }
        public uint MaterialSplitTotalUsedBones { get; private set; }
        public byte[] BoneRemapIndices { get; private set; }
        public MaterialSplitSkinInfo[] MaterialSplitSkinInfo { get; private set; }
        public MaterialSplitUsedBoneInfo[] MaterialSplitUsedBoneInfo { get; private set; }

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

        public RWSkinPlugin(uint size, uint version, RWNode parent, BinaryReader reader, RWGeometry rwGeometry)
            : base(RWType.SkinPlugin, size, version, parent)
        {
            // Header

            Geometry = rwGeometry;
            BoneCount = reader.ReadByte();
            UsedBoneCount = reader.ReadByte();
            MaxWeightPerVertex = reader.ReadByte();
            Unused = reader.ReadByte();

            // Used bones

            UsedBones = new byte[UsedBoneCount];
            for (int i = 0; i < UsedBoneCount; i++)
                UsedBones[i] = reader.ReadByte();

            // Skin Bone Indices

            SkinBoneIndices = new byte[rwGeometry.Struct.VertexCount][];
            for (int i = 0; i < rwGeometry.Struct.VertexCount; i++)
            {
                SkinBoneIndices[i] = reader.ReadBytes(4);
            }

            // Skin Bone Weights

            SkinBoneWeights = new float[rwGeometry.Struct.VertexCount][];
            for (int i = 0; i < rwGeometry.Struct.VertexCount; i++)
            {
                SkinBoneWeights[i] = new float[4];
                for (int j = 0; j < 4; j++)
                    SkinBoneWeights[i][j] = reader.ReadSingle();
            }

            // Inverse Matrices

            InverseMatrices = new Matrix4[BoneCount];
            for (int i = 0; i < BoneCount; i++)
            {
                Matrix4 mtx = Matrix4.Identity;
                mtx.M11 = reader.ReadSingle(); mtx.M12 = reader.ReadSingle(); mtx.M13 = reader.ReadSingle();
                reader.BaseStream.Position += 4;
                mtx.M21 = reader.ReadSingle(); mtx.M22 = reader.ReadSingle(); mtx.M23 = reader.ReadSingle();
                reader.BaseStream.Position += 4;
                mtx.M31 = reader.ReadSingle(); mtx.M32 = reader.ReadSingle(); mtx.M33 = reader.ReadSingle();
                reader.BaseStream.Position += 4;
                mtx.M41 = reader.ReadSingle(); mtx.M42 = reader.ReadSingle(); mtx.M43 = reader.ReadSingle();
                reader.BaseStream.Position += 4;
                InverseMatrices[i] = mtx;
            }

            // PS2 Native skin data

            BoneLimit = reader.ReadUInt32();
            MaterialSplitCount = reader.ReadUInt32();
            MaterialSplitTotalUsedBones = reader.ReadUInt32();

            if (MaterialSplitCount < 1)
                return;

            // Bone remap indices

            BoneRemapIndices = new byte[BoneCount];
            for (int i = 0; i < BoneCount; i++)
                BoneRemapIndices[i] = reader.ReadByte();

            // Bone RLE counts

            MaterialSplitSkinInfo = new MaterialSplitSkinInfo[MaterialSplitCount];
            for (int i = 0; i < MaterialSplitCount; i++)
                MaterialSplitSkinInfo[i] = new MaterialSplitSkinInfo { UsedBonesStartIndex = reader.ReadByte(), NumUsedBones = reader.ReadByte() };

            // Bone RLE

            MaterialSplitUsedBoneInfo = new MaterialSplitUsedBoneInfo[MaterialSplitTotalUsedBones];
            for (int i = 0; i < MaterialSplitTotalUsedBones; i++)
                MaterialSplitUsedBoneInfo[i] = new MaterialSplitUsedBoneInfo { UsedBoneHierarchyIndex = reader.ReadByte(), Unknown = reader.ReadByte() };
        }

        public RWSkinPlugin(RWFrameList rwFrameList, RWGeometry rwGeometry, byte[][] skinBoneIndices, float[][] skinBoneWeights)
            : base(RWType.SkinPlugin)
        {
            Geometry = rwGeometry;

            RWHierarchyAnimPlugin root = RWFrameList.GetRoot(rwFrameList);
            BoneCount = (byte)root.NodeCount;

            List<byte> usedBoneList = new List<byte>();
            MaxWeightPerVertex = 0;

            for (int i = 0; i < skinBoneIndices.Length; i++)
            {
                double wSum = 0.0f;
                int wUsed = 0;
                for (int j = 0; j < 4; j++)
                {
                    if (skinBoneWeights[i][j] != 0.0f)
                    {
                        ++wUsed;
                        wSum += skinBoneWeights[i][j];
                        //skinBoneIndices[i][j] = (byte)(DFSFrameList.FindIndex(f => f.Index == skinBoneIndices[i][j]));

                        if (!usedBoneList.Contains(skinBoneIndices[i][j]))
                            usedBoneList.Add(skinBoneIndices[i][j]);
                    }
                }
                if (wSum < 1.0f)
                {
                    double wRemainder = 1.0f - wSum;
                    wRemainder /= wUsed;
                    for (int j = 0; j < wUsed; j++)
                        skinBoneWeights[i][j] += (float)wRemainder;
                }
                if (wUsed > MaxWeightPerVertex)
                    MaxWeightPerVertex = (byte)wUsed;
            }

            UsedBoneCount = (byte)usedBoneList.Count;
            Unused = 0;
            UsedBones = usedBoneList.ToArray();
            SkinBoneIndices = skinBoneIndices;
            SkinBoneWeights = skinBoneWeights;

            InverseMatrices = new Matrix4[BoneCount];
            for (int i = 0; i < BoneCount; i++)
            {
                InverseMatrices[i] = (rwFrameList.GetFrameByHierarchyIndex(root.NodeList[i].HierarchyIndex).LocalMatrix * rwFrameList.Struct.Frames[2].WorldMatrix).Inverted(); // Get the atomic root frame world matrix
            }

            /*
            BoneLimit = 64;
            MaterialSplitCount = (uint)rwGeometry.MaterialList.Struct.materialCount;
            MaterialSplitUsedBones = UsedBoneCount;
            */
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write(BoneCount);
            writer.Write(UsedBoneCount);
            writer.Write(MaxWeightPerVertex);
            writer.Write(Unused);
            writer.Write(UsedBones);
            for (int i = 0; i < Geometry.Struct.VertexCount; i++)
                writer.Write(SkinBoneIndices[i]);
            for (int i = 0; i < Geometry.Struct.VertexCount; i++)
                for (int j = 0; j < 4; j++)
                    writer.Write(SkinBoneWeights[i][j]);
            for (int i = 0; i < BoneCount; i++)
            {
                writer.Write(InverseMatrices[i].M11); writer.Write(InverseMatrices[i].M12); writer.Write(InverseMatrices[i].M13);
                writer.Write(0.0f);
                writer.Write(InverseMatrices[i].M21); writer.Write(InverseMatrices[i].M22); writer.Write(InverseMatrices[i].M23);
                writer.Write(0.0f);
                writer.Write(InverseMatrices[i].M31); writer.Write(InverseMatrices[i].M32); writer.Write(InverseMatrices[i].M33);
                writer.Write(0.0f);
                writer.Write(InverseMatrices[i].M41); writer.Write(InverseMatrices[i].M42); writer.Write(InverseMatrices[i].M43);
                writer.Write(0.0f);
            }
            writer.Write(BoneLimit);
            writer.Write(MaterialSplitCount);
            writer.Write(MaterialSplitTotalUsedBones);
            if (MaterialSplitCount < 1)
                return;
            writer.Write(BoneRemapIndices);
            for (int i = 0; i < MaterialSplitCount; i++)
            {
                writer.Write(MaterialSplitSkinInfo[i].UsedBonesStartIndex);
                writer.Write(MaterialSplitSkinInfo[i].NumUsedBones);
            }
            for (int i = 0; i < MaterialSplitTotalUsedBones; i++)
            {
                writer.Write(MaterialSplitUsedBoneInfo[i].UsedBoneHierarchyIndex);
                writer.Write(MaterialSplitUsedBoneInfo[i].Unknown);
            }
        }
    }

    public struct MaterialSplitSkinInfo
    {
        public byte UsedBonesStartIndex { get; set; }
        public byte NumUsedBones { get; set; }
    }

    public struct MaterialSplitUsedBoneInfo
    {
        public byte UsedBoneHierarchyIndex { get; set; }
        public byte Unknown { get; set; } // something related to num of influences?
    }
}