namespace AtlusLibSharp.SMT3.ChunkResources.Animation
{
    using System.Collections.Generic;
    using System.IO;
    using Utilities;

    public class MTChunk : Chunk
    {
        internal const int FLAG = 0x0008;
        internal const string TAG = "MT00";
        internal const int DATA_START_ADDRESS = 0x20;

        // Private fields
        private int _posFileStart;
        private int _unused;
        private int _addressRelocTableOffset;
        private int _addressRelocTableSize;
        private byte[] _addressRelocTable;
        private ushort _numMorph1;
        private ushort _numMorph2;
        private ushort _numMorph3;
        private ushort _unkZero;
        private ushort _numAnims;
        private ushort _numKeys;
        private int _animPointerTableOffset;
        private MTKeyInfo[] _animKeyInfo;
        private int[] _animPointers;
        private MTAnimation[] _anims;

        // Constructors
        internal MTChunk(ushort id, int length, BinaryReader reader)
            : base(FLAG, id, length, TAG)
        {
            Read(reader);
        }

        // Properties
        public ushort AnimationCount
        {
            get { return _numAnims; }
        }

        public ushort KeyCount
        {
            get { return _numKeys; }
        }

        public MTKeyInfo[] KeyInfoArray
        {
            get { return _animKeyInfo; }
        }

        public MTAnimation[] Animations
        {
            get { return _anims; }
        }

        private int GetRelativeAddress(long absAddress)
        {
            return (int)absAddress - _posFileStart - DATA_START_ADDRESS;
        }

        // Methods
        internal override void InternalWrite(BinaryWriter writer)
        {
            // Skip header
            _posFileStart = (int)writer.BaseStream.Position;
            writer.Seek(DATA_START_ADDRESS, SeekOrigin.Current);

            // Create address list for reloc table
            List<int> addressList = new List<int>();

            writer.Write(_numAnims);
            writer.Write(_numKeys);

            // Calculate size of key info table & anim pointer table offset
            int keyInfoTableSize = _numKeys * MTKeyInfo.SizeInBytes;
            _animPointerTableOffset = GetRelativeAddress(writer.GetPosition() + keyInfoTableSize);

            addressList.Add((int)writer.GetPosition() - _posFileStart);
            writer.Write(_animPointerTableOffset);

            // Write key info
            for (int i = 0; i < _numKeys; i++)
            {
                writer.Write((ushort)_animKeyInfo[i].KeyType);
                writer.Write(_animKeyInfo[i].UnkMorph);
                writer.Write(_animKeyInfo[i].BoneIndex);
            }

            // Calculate anim pointer table size
            long posAnimPointerTable = writer.GetPosition();
            int animPointerTableSize = _numAnims * sizeof(int);
            int animPointerBase = (int)posAnimPointerTable + animPointerTableSize;

            // Write anims
            _animPointers = new int[_numAnims];
            for (int i = 0; i < _numAnims; i++)
            {
                if (_anims[i] == null)
                {
                    _animPointers[i] = 0;
                }
                else
                {
                    _animPointers[i] = GetRelativeAddress(animPointerBase);

                    writer.Seek(animPointerBase, SeekOrigin.Begin);
                    _anims[i].Write(writer);
                    animPointerBase = (int)writer.GetPosition();
                }
            }

            // Write pointer table
            writer.Seek(posAnimPointerTable, SeekOrigin.Begin);
            for (int i = 0; i < _numAnims; i++)
            {
                if (_anims[i] != null)
                {
                    addressList.Add((int)writer.GetPosition() - _posFileStart);
                    writer.Write(_animPointers[i]);
                }
            }

            writer.Seek(animPointerBase, SeekOrigin.Begin);

            // Calculate address reloc table values
            _addressRelocTable = AddressRelocationTableCompression.Compress(addressList, DATA_START_ADDRESS);
            _addressRelocTableSize = _addressRelocTable.Length;
            _addressRelocTableOffset = GetRelativeAddress(writer.GetPosition());

            // Write address reloc table
            writer.Write(_addressRelocTable);

            long posFileEnd = writer.GetPosition();

            // Calculate length
            Length = (int)(posFileEnd - _posFileStart);

            // Write header
            writer.Seek(_posFileStart, SeekOrigin.Begin);
            writer.Write(FLAG);
            writer.Write(UserID);
            writer.WriteCString(TAG);
            writer.Write(Length);
            writer.Write(0);
            writer.Write(_addressRelocTableOffset);
            writer.Write(_addressRelocTableSize);
            writer.Write(_numMorph1);
            writer.Write(_numMorph2);
            writer.Write(_numMorph3);
            writer.Write(_unkZero);

            writer.BaseStream.Seek(posFileEnd, SeekOrigin.Begin);
            writer.AlignPosition(64);
        }

        private void Read(BinaryReader reader)
        {
            int fp = (int)reader.BaseStream.Position;
            _unused = reader.ReadInt32();
            _addressRelocTableOffset = reader.ReadInt32();
            _addressRelocTableSize = reader.ReadInt32();
            _numMorph1 = reader.ReadUInt16();
            _numMorph2 = reader.ReadUInt16();
            _numMorph3 = reader.ReadUInt16();
            _unkZero = reader.ReadUInt16();
            _numAnims = reader.ReadUInt16();
            _numKeys = reader.ReadUInt16();
            _animPointerTableOffset = reader.ReadInt32();

            _animKeyInfo = new MTKeyInfo[_numKeys];
            for (int i = 0; i < _numKeys; i++)
            {
                _animKeyInfo[i].KeyType = (MTKeyType)reader.ReadInt16();
                _animKeyInfo[i].UnkMorph = reader.ReadInt16();
                _animKeyInfo[i].BoneIndex = reader.ReadInt32();
            }

            reader.BaseStream.Seek(fp + DATA_START_ADDRESS + _addressRelocTableOffset, SeekOrigin.Begin);
            _addressRelocTable = reader.ReadBytes(_addressRelocTableSize);

            reader.BaseStream.Seek(fp + DATA_START_ADDRESS + _animPointerTableOffset, SeekOrigin.Begin);
            _animPointers = reader.ReadInt32Array(_numAnims);

            _anims = new MTAnimation[_numAnims];
            for (int i = 0; i < _numAnims; i++)
            {
                if (_animPointers[i] == 0)
                {
                    // Some pointers just serve as a dummy animation slot
                    continue;
                }

                reader.BaseStream.Seek(fp + DATA_START_ADDRESS + _animPointers[i], SeekOrigin.Begin);
                _anims[i] = new MTAnimation(_numKeys, reader);
            }
        }
    }
}
