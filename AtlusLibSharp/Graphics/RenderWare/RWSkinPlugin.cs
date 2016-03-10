using OpenTK;
using System.Collections.Generic;
using System.IO;

namespace AtlusLibSharp.Graphics.RenderWare
{
    using AtlusLibSharp.Utilities;

    // This class needs some major refactoring
    public class RWSkinPlugin : RWNode
    {
        private byte _numBones;
        private byte _numUsedBones;
        private byte _numWeightPerVertex;
        private byte _unused;
        private byte[] _usedBoneIndices;
        private byte[][] _skinBoneIndices;
        private float[][] _skinBoneWeights;
        private Matrix4[] _inverseBoneMatrices;
        private int _boneLimit;
        private int _numMaterialSplit;
        private int _materialSplitNumUsedBones;
        private byte[] _boneRemapIndices;
        private MaterialSplitSkinInfo[] _materialSplitSkinInfo;
        private MaterialSplitUsedBoneInfo[] _materialSplitUsedBoneInfo;

        public byte BoneCount
        {
            get { return _numBones; }
        }

        public byte UsedBoneCount
        {
            get { return _numUsedBones; }
        }

        public byte MaxWeightCountPerVertex
        {
            get { return _numWeightPerVertex; }
        }

        public byte[] UsedBoneIndices
        {
            get { return _usedBoneIndices; }
        }

        public byte[][] SkinBoneIndices
        {
            get { return _skinBoneIndices; }
        }

        public float[][] SkinBoneWeights
        {
            get { return _skinBoneWeights; }
        }

        public Matrix4[] InverseBoneMatrices
        {
            get { return _inverseBoneMatrices; }
        }

        public int BoneLimit
        {
            get { return _boneLimit; }
        }

        public int MaterialSplitCount
        {
            get { return _numMaterialSplit; }
        }
        public int MaterialSplitTotalUsedBones
        {
            get { return _materialSplitNumUsedBones; }
        }

        public byte[] BoneRemapIndices
        {
            get { return _boneRemapIndices; }
        }

        public MaterialSplitSkinInfo[] MaterialSplitSkinInfo
        {
            get { return _materialSplitSkinInfo; }
        }

        public MaterialSplitUsedBoneInfo[] MaterialSplitUsedBoneInfo
        {
            get { return _materialSplitUsedBoneInfo; }
        }

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

        internal RWSkinPlugin(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader, RWGeometry rwGeometry)
            : base(header)
        {
            int numVertices = rwGeometry.VertexCount;

            _numBones = reader.ReadByte();
            _numUsedBones = reader.ReadByte();
            _numWeightPerVertex = reader.ReadByte();
            _unused = reader.ReadByte();

            _usedBoneIndices = reader.ReadBytes(_numUsedBones);

            _skinBoneIndices = new byte[numVertices][];
            for (int i = 0; i < numVertices; i++)
            {
                _skinBoneIndices[i] = reader.ReadBytes(4);
            }

            _skinBoneWeights = new float[numVertices][];
            for (int i = 0; i < numVertices; i++)
            {
                _skinBoneWeights[i] = reader.ReadFloatArray(4);
            }

            _inverseBoneMatrices = new Matrix4[_numBones];
            for (int i = 0; i < BoneCount; i++)
            {
                Matrix4 mtx = Matrix4.Identity;

                mtx.M11 = reader.ReadSingle(); mtx.M12 = reader.ReadSingle(); mtx.M13 = reader.ReadSingle(); reader.BaseStream.Position += 4;
                mtx.M21 = reader.ReadSingle(); mtx.M22 = reader.ReadSingle(); mtx.M23 = reader.ReadSingle(); reader.BaseStream.Position += 4;
                mtx.M31 = reader.ReadSingle(); mtx.M32 = reader.ReadSingle(); mtx.M33 = reader.ReadSingle(); reader.BaseStream.Position += 4;
                mtx.M41 = reader.ReadSingle(); mtx.M42 = reader.ReadSingle(); mtx.M43 = reader.ReadSingle(); reader.BaseStream.Position += 4;

                _inverseBoneMatrices[i] = mtx;
            }

            _boneLimit = reader.ReadInt32();
            _numMaterialSplit = reader.ReadInt32();
            _materialSplitNumUsedBones = reader.ReadInt32();

            if (_numMaterialSplit < 1)
                return;

            _boneRemapIndices = reader.ReadBytes(_numBones);

            _materialSplitSkinInfo = new MaterialSplitSkinInfo[_numMaterialSplit];
            for (int i = 0; i < _numMaterialSplit; i++)
                _materialSplitSkinInfo[i] = new MaterialSplitSkinInfo { UsedBonesStartIndex = reader.ReadByte(), NumUsedBones = reader.ReadByte() };

            _materialSplitUsedBoneInfo = new MaterialSplitUsedBoneInfo[_materialSplitNumUsedBones];
            for (int i = 0; i < _materialSplitNumUsedBones; i++)
                _materialSplitUsedBoneInfo[i] = new MaterialSplitUsedBoneInfo { UsedBoneHierarchyIndex = reader.ReadByte(), Unknown = reader.ReadByte() };
        }

        public RWSkinPlugin(RWFrameList rwFrameList, RWGeometry rwGeometry, byte[][] skinBoneIndices, float[][] skinBoneWeights)
            : base(RWType.SkinPlugin)
        {
            RWHierarchyAnimPlugin root = rwFrameList.HierarchyAnimRoot;
            _numBones = (byte)root.NodeCount;

            List<byte> usedBoneList = new List<byte>();
            _numWeightPerVertex = 0;

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
                if (wUsed > MaxWeightCountPerVertex)
                    _numWeightPerVertex = (byte)wUsed;
            }

            _numUsedBones = (byte)usedBoneList.Count;
            _unused = 0;
            _usedBoneIndices = usedBoneList.ToArray();
            _skinBoneIndices = skinBoneIndices;
            _skinBoneWeights = skinBoneWeights;

            _inverseBoneMatrices = new Matrix4[BoneCount];
            for (int i = 0; i < BoneCount; i++)
            {
                InverseBoneMatrices[i] = 
                    (rwFrameList.GetFrameByHierarchyIndex(root.Nodes[i].HierarchyIndex).LocalMatrix * 
                     rwFrameList.Frames[2].WorldMatrix)
                     .Inverted(); // Get the atomic root frame world matrix
            }

            /*
            BoneLimit = 64;
            MaterialSplitCount = (uint)rwGeometry.MaterialList.Struct.materialCount;
            MaterialSplitUsedBones = UsedBoneCount;
            */
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(_numBones);
            writer.Write(_numUsedBones);
            writer.Write(_numWeightPerVertex);
            writer.Write(_unused);
            writer.Write(_usedBoneIndices);

            for (int i = 0; i < SkinBoneIndices.Length; i++)
            {
                writer.Write(SkinBoneIndices[i]);
            }

            for (int i = 0; i < SkinBoneWeights.Length; i++)
            {
                writer.Write(_skinBoneWeights[i]);
            }

            for (int i = 0; i < BoneCount; i++)
            {
                writer.Write(InverseBoneMatrices[i].M11); writer.Write(InverseBoneMatrices[i].M12); writer.Write(InverseBoneMatrices[i].M13); writer.Write(0);
                writer.Write(InverseBoneMatrices[i].M21); writer.Write(InverseBoneMatrices[i].M22); writer.Write(InverseBoneMatrices[i].M23); writer.Write(0);
                writer.Write(InverseBoneMatrices[i].M31); writer.Write(InverseBoneMatrices[i].M32); writer.Write(InverseBoneMatrices[i].M33); writer.Write(0);
                writer.Write(InverseBoneMatrices[i].M41); writer.Write(InverseBoneMatrices[i].M42); writer.Write(InverseBoneMatrices[i].M43); writer.Write(0);
            }

            writer.Write(_boneLimit);
            writer.Write(_numMaterialSplit);
            writer.Write(_materialSplitNumUsedBones);

            if (_numMaterialSplit < 1)
            {
                return;
            }

            writer.Write(_boneRemapIndices);

            for (int i = 0; i < _numMaterialSplit; i++)
            {
                writer.Write(MaterialSplitSkinInfo[i].UsedBonesStartIndex);
                writer.Write(MaterialSplitSkinInfo[i].NumUsedBones);
            }

            for (int i = 0; i < _materialSplitNumUsedBones; i++)
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