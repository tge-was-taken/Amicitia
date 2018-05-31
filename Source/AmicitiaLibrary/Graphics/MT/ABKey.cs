using AmicitiaLibrary.Utilities;

namespace AmicitiaLibrary.Graphics.MT
{
    using System.IO;
    using AmicitiaLibrary.Utilities;

    public class AbKey
    {
        // Private constants
        private const int HEADER_SIZE = 8;
        private const int FIXED_POINT = 1 << 15; 

        // Private fields
        private ushort mNumAssignedFrames;
        private ushort mType;
        private ushort[] mAssignedFrameIndices;
        private float[][] mBoneKeyData;

        // Constructors
        internal AbKey(BinaryReader reader)
        {
            InternalRead(reader);
        }

        // Properties
        public ushort AssignedFrameCount
        {
            get { return mNumAssignedFrames; }
        }

        public ushort Type
        {
            get { return mType; }
        }

        public ushort[] AssignedFrameIndices
        {
            get { return mAssignedFrameIndices; }
        }

        public float[][] BoneKeyData
        {
            get { return mBoneKeyData; }
        }

        // Methods
        internal void InternalWrite(BinaryWriter writer)
        {
            int size = GetSize();
            writer.Write(size);
            writer.Write(mNumAssignedFrames);
            writer.Write(mType);

            for (int i = 0; i < mNumAssignedFrames; i++)
            {
                writer.Write(mAssignedFrameIndices[i]);
            }

            writer.AlignPosition(4);

            for (int i = 0; i < mNumAssignedFrames; i++)
            {
                switch (mType)
                {
                    case 0x04:
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                writer.Write((ushort)(mBoneKeyData[i][j] * FIXED_POINT));
                            }
                        }

                        break;
                    case 0x08:
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                writer.Write((ushort)(mBoneKeyData[i][j] * FIXED_POINT));
                            }
                        }

                        break;
                    case 0x0C:
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                writer.Write(mBoneKeyData[i][j]);
                            }
                        }

                        break;
                    default:
                        break;
                }
            }
        }

        internal int GetSize()
        {
            int assignedFrameIndicesSize = AlignmentHelper.Align(mNumAssignedFrames * sizeof(ushort), 4);
            int boneKeySize = GetKeySize(mType);
            int boneKeyDataSize = mNumAssignedFrames * boneKeySize;
            return HEADER_SIZE + assignedFrameIndicesSize + boneKeyDataSize;
        }

        private void InternalRead(BinaryReader reader)
        {
            int size = reader.ReadInt32();
            mNumAssignedFrames = reader.ReadUInt16();
            mType = reader.ReadUInt16();

            mAssignedFrameIndices = reader.ReadUInt16Array(mNumAssignedFrames);

            // Align to 4 bytes
            reader.AlignPosition(4);

            mBoneKeyData = new float[mNumAssignedFrames][];
            for (int i = 0; i < mNumAssignedFrames; i++)
            {
                switch (mType)
                {
                    case 0x04:
                        {
                            mBoneKeyData[i] = new float[2];
                            for (int j = 0; j < 2; j++)
                            {
                                mBoneKeyData[i][j] = (float)reader.ReadInt16() / FIXED_POINT;
                            }
                        }

                        break;
                    case 0x08:
                        {
                            mBoneKeyData[i] = new float[4];
                            for (int j = 0; j < 4; j++)
                            {
                                mBoneKeyData[i][j] = (float)reader.ReadInt16() / FIXED_POINT;
                            }
                        }

                        break;
                    case 0x0C:
                        {
                            mBoneKeyData[i] = new float[3];
                            for (int j = 0; j < 3; j++)
                            {
                                mBoneKeyData[i][j] = reader.ReadSingle();
                            }
                        }

                        break;
                    default:
                        break;
                }
            }
        }

        private int GetKeySize(ushort type)
        {
            switch (type)
            {
                case 0x04:
                    return 4;
                case 0x08:
                    return 8;
                case 0x0C:
                    return 12;
                default:
                    throw new System.ArgumentException();
            }
        }
    }
}
