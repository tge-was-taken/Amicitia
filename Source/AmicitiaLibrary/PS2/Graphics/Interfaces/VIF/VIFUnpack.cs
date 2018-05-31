using AmicitiaLibrary.Utilities;

namespace AmicitiaLibrary.PS2.Graphics.Interfaces.VIF
{
    using System.IO;
    using AmicitiaLibrary.Utilities;

    public class VifUnpack : VifPacket
    {
        // Constructors
        public VifUnpack(VifTag vt, BinaryReader reader)
            : base(vt)
        {
            ReadElements(reader);
        }

        public enum VifUnpackElementFormat
        {
            Float = 0x0,
            Short = 0x1,
            Byte = 0x2,
            RGBA5A1 = 0x3
        }

        // Properties
        public override PS2VifCommand Command
        {
            get { return (PS2VifCommand)(_command & 0xF0); }
        }

        public ushort Address
        {
            get
            {
                return (ushort)(_immediate & 0x1FF);
            }

            set
            {
                unchecked
                {
                    _immediate &= (ushort)~0x1FF;
                }

                _immediate |= (ushort)(value & 0x1FF);
            }
        }

        public bool Sign
        {
            get
            {
                return BitHelper.IsBitSet(_immediate, 14);
            }

            set
            {
                if (value == true)
                {
                    BitHelper.SetBit(ref _immediate, 14);
                }
                else
                {
                    BitHelper.ClearBit(ref _immediate, 14);
                }
            }
        }

        public bool Flag
        {
            get
            {
                return BitHelper.IsBitSet(_immediate, 14);
            }

            set
            {
                if (value == true)
                {
                    BitHelper.SetBit(ref _immediate, 15);
                }
                else
                {
                    BitHelper.ClearBit(ref _immediate, 15);
                }
            }
        }

        public byte ElementCount
        {
            get
            {
                return (byte)(((_command & (0x3 << 2)) >> 2) + 1);
            }

            set
            {
                unchecked
                {
                    _command &= (byte)~(0x3 << 2);
                }

                _command |= (byte)((value - 1 & 0x3) << 2);
            }
        }

        public VifUnpackElementFormat ElementFormat
        {
            get
            {
                return (VifUnpackElementFormat)(_command & 0x3);
            }

            set
            {
                unchecked
                {
                    _command &= (byte)~0x3;
                }

                _command |= (byte)((byte)value & 0x3);
            }
        }

        public object[][] Elements { get; set; }

        // Methods
        public override string ToString()
        {
            return $"{Command} addr:{Address} count:{DataCount} numElements:{ElementCount} format:{ElementFormat}";
        }

        private void ReadElements(BinaryReader reader)
        {
            Elements = new object[DataCount][];
            switch (ElementFormat)
            {
                case VifUnpackElementFormat.Float:
                    for (int i = 0; i < DataCount; i++)
                    {
                        Elements[i] = new object[ElementCount];
                        for (int j = 0; j < ElementCount; j++)
                        {
                            Elements[i][j] = reader.ReadSingle();
                        }
                    }

                    break;
                case VifUnpackElementFormat.Short:
                    for (int i = 0; i < DataCount; i++)
                    {
                        Elements[i] = new object[ElementCount];
                        for (int j = 0; j < ElementCount; j++)
                        {
                            Elements[i][j] = reader.ReadUInt16();
                        }
                    }

                    break;
                case VifUnpackElementFormat.Byte:
                    for (int i = 0; i < DataCount; i++)
                    {
                        Elements[i] = new object[ElementCount];
                        for (int j = 0; j < ElementCount; j++)
                        {
                            Elements[i][j] = reader.ReadByte();
                        }
                    }

                    break;
                case VifUnpackElementFormat.RGBA5A1:
                    for (int i = 0; i < DataCount; i++)
                    {
                        Elements[i] = new object[ElementCount];
                        for (int j = 0; j < ElementCount; j++)
                        {
                            Elements[i][j] = reader.ReadUInt16();
                        }
                    }

                    break;
            }
        }
    }
}
