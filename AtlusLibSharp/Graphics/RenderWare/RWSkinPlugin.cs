namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;
    using System.Numerics;
    using Utilities;
    using System.Reflection;
    using System;

    // This class needs some major refactoring
    public class RWSkinPlugin : RWNode
    {
        private byte m_numBones;
        private byte m_numUsedBones;
        private byte m_numWeightPerVertex;
        private byte m_unused;
        private byte[] m_usedBoneIndices;
        private byte[][] m_skinBoneIndices;
        private float[][] m_skinBoneWeights;
        private Matrix4x4[] m_inverseBoneMatrices;
        private int m_boneLimit;
        private int m_numMaterialSplit;
        private int m_materialSplitNumUsedBones;
        private byte[] m_boneRemapIndices;
        private MaterialSplitSkinInfo[] m_materialSplitSkinInfo;
        private MaterialSplitUsedBoneInfo[] m_materialSplitUsedBoneInfo;

        public byte BoneCount
        {
            get { return m_numBones; }
        }

        public byte UsedBoneCount
        {
            get { return m_numUsedBones; }
        }

        public byte MaxWeightCountPerVertex
        {
            get { return m_numWeightPerVertex; }
        }

        public byte[] UsedBoneIndices
        {
            get { return m_usedBoneIndices; }
        }

        public byte[][] SkinBoneIndices
        {
            get { return m_skinBoneIndices; }
        }

        public float[][] SkinBoneWeights
        {
            get { return m_skinBoneWeights; }
        }

        public Matrix4x4[] InverseBoneMatrices
        {
            get { return m_inverseBoneMatrices; }
        }

        public int BoneLimit
        {
            get { return m_boneLimit; }
        }

        public int MaterialSplitCount
        {
            get { return m_numMaterialSplit; }
        }
        public int MaterialSplitTotalUsedBones
        {
            get { return m_materialSplitNumUsedBones; }
        }

        public byte[] BoneRemapIndices
        {
            get { return m_boneRemapIndices; }
        }

        public MaterialSplitSkinInfo[] MaterialSplitSkinInfo
        {
            get { return m_materialSplitSkinInfo; }
        }

        public MaterialSplitUsedBoneInfo[] MaterialSplitUsedBoneInfo
        {
            get { return m_materialSplitUsedBoneInfo; }
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

        internal RWSkinPlugin(RWNodeFactory.RWNodeInfo header, BinaryReader reader, RWMesh rwGeometry)
            : base(header)
        {
            int numVertices = rwGeometry.VertexCount;

            m_numBones = reader.ReadByte();
            m_numUsedBones = reader.ReadByte();
            m_numWeightPerVertex = reader.ReadByte();
            m_unused = reader.ReadByte();

            m_usedBoneIndices = reader.ReadBytes(m_numUsedBones);

            m_skinBoneIndices = new byte[numVertices][];
            for (int i = 0; i < numVertices; i++)
            {
                m_skinBoneIndices[i] = reader.ReadBytes(4);
            }

            m_skinBoneWeights = new float[numVertices][];
            for (int i = 0; i < numVertices; i++)
            {
                m_skinBoneWeights[i] = reader.ReadFloatArray(4);
            }

            m_inverseBoneMatrices = new Matrix4x4[m_numBones];
            for (int i = 0; i < BoneCount; i++)
            {
                Matrix4x4 mtx = Matrix4x4.Identity;

                mtx.M11 = reader.ReadSingle(); mtx.M12 = reader.ReadSingle(); mtx.M13 = reader.ReadSingle(); reader.BaseStream.Position += 4;
                mtx.M21 = reader.ReadSingle(); mtx.M22 = reader.ReadSingle(); mtx.M23 = reader.ReadSingle(); reader.BaseStream.Position += 4;
                mtx.M31 = reader.ReadSingle(); mtx.M32 = reader.ReadSingle(); mtx.M33 = reader.ReadSingle(); reader.BaseStream.Position += 4;
                mtx.M41 = reader.ReadSingle(); mtx.M42 = reader.ReadSingle(); mtx.M43 = reader.ReadSingle(); reader.BaseStream.Position += 4;

                m_inverseBoneMatrices[i] = mtx;
            }

            m_boneLimit = reader.ReadInt32();
            m_numMaterialSplit = reader.ReadInt32();
            m_materialSplitNumUsedBones = reader.ReadInt32();

            if (m_numMaterialSplit < 1)
                return;

            m_boneRemapIndices = reader.ReadBytes(m_numBones);

            m_materialSplitSkinInfo = new MaterialSplitSkinInfo[m_numMaterialSplit];
            for (int i = 0; i < m_numMaterialSplit; i++)
                m_materialSplitSkinInfo[i] = new MaterialSplitSkinInfo { UsedBonesStartIndex = reader.ReadByte(), NumUsedBones = reader.ReadByte() };

            m_materialSplitUsedBoneInfo = new MaterialSplitUsedBoneInfo[m_materialSplitNumUsedBones];
            for (int i = 0; i < m_materialSplitNumUsedBones; i++)
                m_materialSplitUsedBoneInfo[i] = new MaterialSplitUsedBoneInfo { UsedBoneHierarchyIndex = reader.ReadByte(), Unknown = reader.ReadByte() };

            PrintInfo();
        }

        internal void PrintInfo()
        {
            int index = -1;
            using (StreamWriter writer = File.CreateText(GetUniqueName(ref index)))
            {
                WriteField(writer, nameof(m_numBones));
                WriteField(writer, nameof(m_numUsedBones));
                WriteField(writer, nameof(m_numWeightPerVertex));
                WriteField(writer, nameof(m_unused));
                WriteFieldArray(writer, nameof(m_usedBoneIndices));
                WriteFieldDoubleArray(writer, nameof(m_skinBoneIndices));
                WriteFieldDoubleArray(writer, nameof(m_skinBoneWeights));
                WriteFieldArray(writer, nameof(m_inverseBoneMatrices));
                WriteField(writer, nameof(m_boneLimit));
                WriteField(writer, nameof(m_numMaterialSplit));
                WriteField(writer, nameof(m_materialSplitNumUsedBones));
                WriteFieldArray(writer, nameof(m_boneRemapIndices));

                WriteField(writer, nameof(m_materialSplitSkinInfo));
                for (int i = 0; i < m_materialSplitSkinInfo.Length; i++)
                {
                    writer.WriteLine("{0} = {1}", "m_usedBonesStartIndex", m_materialSplitSkinInfo[i].UsedBonesStartIndex);
                    writer.WriteLine("{0} = {1}", "m_numUsedBones", m_materialSplitSkinInfo[i].NumUsedBones);
                }

                WriteField(writer, nameof(m_materialSplitUsedBoneInfo));
                for (int i = 0; i < m_materialSplitUsedBoneInfo.Length; i++)
                {
                    writer.WriteLine("{0} = {1}", "m_usedBoneHierarchyIndex", m_materialSplitUsedBoneInfo[i].UsedBoneHierarchyIndex);
                    writer.WriteLine("{0} = {1}", "m_unknown", m_materialSplitUsedBoneInfo[i].Unknown);
                }
            }
        }

        private void WriteField(StreamWriter writer, string fieldName)
        {
            var field = GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            writer.WriteLine("{0} = {1}", fieldName, field.GetValue(this));
        }

        private void WriteFieldArray(StreamWriter writer, string fieldname)
        {
            WriteField(writer, fieldname);
            var field = GetType().GetField(fieldname, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            dynamic array = field.GetValue(this);
            for (int i = 0; i < array.Length; i++)
                writer.WriteLine(array[i]);
        }

        private void WriteFieldDoubleArray(StreamWriter writer, string fieldname)
        {
            WriteField(writer, fieldname);
            var field = GetType().GetField(fieldname, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            dynamic array = field.GetValue(this);
            for (int i = 0; i < array.Length; i++)
                for (int j = 0; j < array[i].Length; j++)
                    writer.WriteLine(array[i][j]);
        }

        private string GetUniqueName(ref int index)
        {
            index++;
            string str = string.Format("skinplugin_log_{0}.txt", index);
            if (File.Exists(str))
                return GetUniqueName(ref index);
            else
                return str;
        }

        //public RWSkinPlugin(RWSceneNodeList rwFrameList, RWMesh rwGeometry, byte[][] skinBoneIndices, float[][] skinBoneWeights)
        //    : base(RWNodeType.SkinPlugin)
        //{
        //    RWSceneNodeBoneMetadata root = rwFrameList.HierarchyAnimRoot;
        //    _numBones = (byte)root.BoneHierarchyNodeCount;

        //    List<byte> usedBoneList = new List<byte>();
        //    _numWeightPerVertex = 0;

        //    for (int i = 0; i < skinBoneIndices.Length; i++)
        //    {
        //        double wSum = 0.0f;
        //        int wUsed = 0;
        //        for (int j = 0; j < 4; j++)
        //        {
        //            if (skinBoneWeights[i][j] != 0.0f)
        //            {
        //                ++wUsed;
        //                wSum += skinBoneWeights[i][j];
        //                //skinBoneIndices[i][j] = (byte)(DFSFrameList.FindIndex(f => f.Index == skinBoneIndices[i][j]));

        //                if (!usedBoneList.Contains(skinBoneIndices[i][j]))
        //                    usedBoneList.Add(skinBoneIndices[i][j]);
        //            }
        //        }
        //        if (wSum < 1.0f)
        //        {
        //            double wRemainder = 1.0f - wSum;
        //            wRemainder /= wUsed;
        //            for (int j = 0; j < wUsed; j++)
        //                skinBoneWeights[i][j] += (float)wRemainder;
        //        }
        //        if (wUsed > MaxWeightCountPerVertex)
        //            _numWeightPerVertex = (byte)wUsed;
        //    }

        //    _numUsedBones = (byte)usedBoneList.Count;
        //    _unused = 0;
        //    _usedBoneIndices = usedBoneList.ToArray();
        //    _skinBoneIndices = skinBoneIndices;
        //    _skinBoneWeights = skinBoneWeights;

        //    _inverseBoneMatrices = new Matrix4[BoneCount];
        //    for (int i = 0; i < BoneCount; i++)
        //    {
        //        InverseBoneMatrices[i] = 
        //            (rwFrameList.GetFrameByHierarchyIndex(root.Nodes[i].HierarchyIndex).Transform * 
        //             rwFrameList.SceneNodes[2].WorldTransform)
        //             .Inverted(); // Get the drawCall root frame world matrix
        //    }

        //    /*
        //    BoneLimit = 64;
        //    MaterialSplitCount = (uint)rwGeometry.MaterialList.Struct.materialCount;
        //    MaterialSplitUsedBones = UsedBoneCount;
        //    */
        //}

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(m_numBones);
            writer.Write(m_numUsedBones);
            writer.Write(m_numWeightPerVertex);
            writer.Write(m_unused);
            writer.Write(m_usedBoneIndices);

            for (int i = 0; i < SkinBoneIndices.Length; i++)
            {
                writer.Write(SkinBoneIndices[i]);
            }

            for (int i = 0; i < SkinBoneWeights.Length; i++)
            {
                writer.Write(m_skinBoneWeights[i]);
            }

            for (int i = 0; i < BoneCount; i++)
            {
                writer.Write(InverseBoneMatrices[i].M11); writer.Write(InverseBoneMatrices[i].M12); writer.Write(InverseBoneMatrices[i].M13); writer.Write(0);
                writer.Write(InverseBoneMatrices[i].M21); writer.Write(InverseBoneMatrices[i].M22); writer.Write(InverseBoneMatrices[i].M23); writer.Write(0);
                writer.Write(InverseBoneMatrices[i].M31); writer.Write(InverseBoneMatrices[i].M32); writer.Write(InverseBoneMatrices[i].M33); writer.Write(0);
                writer.Write(InverseBoneMatrices[i].M41); writer.Write(InverseBoneMatrices[i].M42); writer.Write(InverseBoneMatrices[i].M43); writer.Write(0);
            }

            writer.Write(m_boneLimit);
            writer.Write(m_numMaterialSplit);
            writer.Write(m_materialSplitNumUsedBones);

            if (m_numMaterialSplit < 1)
            {
                return;
            }

            writer.Write(m_boneRemapIndices);

            for (int i = 0; i < m_numMaterialSplit; i++)
            {
                writer.Write(MaterialSplitSkinInfo[i].UsedBonesStartIndex);
                writer.Write(MaterialSplitSkinInfo[i].NumUsedBones);
            }

            for (int i = 0; i < m_materialSplitNumUsedBones; i++)
            {
                writer.Write(MaterialSplitUsedBoneInfo[i].UsedBoneHierarchyIndex);
                writer.Write(MaterialSplitUsedBoneInfo[i].Unknown);
            }
        }
    }

    public class MaterialSplitSkinInfo
    {
        public byte UsedBonesStartIndex { get; set; }
        public byte NumUsedBones { get; set; }
    }

    public class MaterialSplitUsedBoneInfo
    {
        public byte UsedBoneHierarchyIndex { get; set; }
        public byte Unknown { get; set; } // something related to num of influences?
    }
}