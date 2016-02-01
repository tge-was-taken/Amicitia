namespace AtlusLibSharp.SMT3.ChunkResources.Animation
{
    using System.IO;
    using Utilities;

    public class ABKey
    {
        // Private constants
        private const int HEADER_SIZE = 8;
        private const int FIXED_POINT = 1 << 15; 

        // Private fields
        private ushort _numAssignedFrames;
        private ushort _type;
        private ushort[] _assignedFrameIndices;
        private float[][] _boneKeyData;

        // Constructors
        internal ABKey(BinaryReader reader)
        {
            InternalRead(reader);
        }

        // Properties
        public ushort AssignedFrameCount
        {
            get { return _numAssignedFrames; }
        }

        public ushort Type
        {
            get { return _type; }
        }

        public ushort[] AssignedFrameIndices
        {
            get { return _assignedFrameIndices; }
        }

        public float[][] BoneKeyData
        {
            get { return _boneKeyData; }
        }

        // Methods
        internal void InternalWrite(BinaryWriter writer)
        {
            int size = GetSize();
            writer.Write(size);
            writer.Write(_numAssignedFrames);
            writer.Write(_type);

            for (int i = 0; i < _numAssignedFrames; i++)
            {
                writer.Write(_assignedFrameIndices[i]);
            }

            writer.AlignPosition(4);

            for (int i = 0; i < _numAssignedFrames; i++)
            {
                switch (_type)
                {
                    case 0x04:
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                writer.Write((ushort)(_boneKeyData[i][j] * FIXED_POINT));
                            }
                        }

                        break;
                    case 0x08:
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                writer.Write((ushort)(_boneKeyData[i][j] * FIXED_POINT));
                            }
                        }

                        break;
                    case 0x0C:
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                writer.Write(_boneKeyData[i][j]);
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
            int assignedFrameIndicesSize = AlignmentHelper.Align(_numAssignedFrames * sizeof(ushort), 4);
            int boneKeySize = GetKeySize(_type);
            int boneKeyDataSize = _numAssignedFrames * boneKeySize;
            return HEADER_SIZE + assignedFrameIndicesSize + boneKeyDataSize;
        }

        private void InternalRead(BinaryReader reader)
        {
            int size = reader.ReadInt32();
            _numAssignedFrames = reader.ReadUInt16();
            _type = reader.ReadUInt16();

            _assignedFrameIndices = reader.ReadUInt16Array(_numAssignedFrames);

            // Align to 4 bytes
            reader.AlignPosition(4);

            _boneKeyData = new float[_numAssignedFrames][];
            for (int i = 0; i < _numAssignedFrames; i++)
            {
                switch (_type)
                {
                    case 0x04:
                        {
                            _boneKeyData[i] = new float[2];
                            for (int j = 0; j < 2; j++)
                            {
                                _boneKeyData[i][j] = (float)reader.ReadInt16() / FIXED_POINT;
                            }
                        }

                        break;
                    case 0x08:
                        {
                            _boneKeyData[i] = new float[4];
                            for (int j = 0; j < 4; j++)
                            {
                                _boneKeyData[i][j] = (float)reader.ReadInt16() / FIXED_POINT;
                            }
                        }

                        break;
                    case 0x0C:
                        {
                            _boneKeyData[i] = new float[3];
                            for (int j = 0; j < 3; j++)
                            {
                                _boneKeyData[i][j] = reader.ReadSingle();
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
