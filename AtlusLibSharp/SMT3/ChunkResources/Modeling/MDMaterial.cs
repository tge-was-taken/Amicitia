namespace AtlusLibSharp.SMT3.ChunkResources.Modeling
{
    using System;
    using System.Drawing;
    using System.IO;

    using Utilities;

    public class MDMaterial
    {
        // Private Fields
        private uint _id;
        private uint _flags;

        // bit 16
        private Color? _color1;

        // bit 17
        private Color? _color2;

        // bit 18
        private uint? _textureID;

        // bit 19
        private float[] _floatArray1;

        // bit 20
        private Color? _color3;

        // bit 21
        private ushort[] _shortArray1;

        // bit 22
        private float[] _floatArray2;

        // bit 23
        private Color? _color4;

        // bit 25
        private float? _float1;

        // bit 26
        private float[] _floatArray3;

        // Constructors
        internal MDMaterial(BinaryReader reader)
        {
            Read(reader);
        }

        // Properties
        public uint ID
        {
            get { return _id; }
        }
        
        public Color? Color1
        {
            get
            {
                return _color1;
            }

            set
            {
                if (value == null)
                {
                    BitHelper.ClearBit(ref _flags, 16);
                }
                else if (_color1 == null)
                {
                    BitHelper.SetBit(ref _flags, 16);
                }

                _color1 = value;
            }
        }
     
        public Color? Color2
        {
            get
            {
                return _color2;
            }

            set
            {
                if (value == null)
                {
                    BitHelper.ClearBit(ref _flags, 17);
                }
                else if (_color2 == null)
                {
                    BitHelper.SetBit(ref _flags, 17);
                }

                _color2 = value;
            }
        }
      
        public uint? TextureID
        {
            get
            {
                return _textureID;
            }

            set
            {
                if (_textureID == null)
                {
                    BitHelper.ClearBit(ref _flags, 18);
                }
                else if (_textureID == null)
                {
                    BitHelper.SetBit(ref _flags, 18);
                }

                _textureID = value;
            }
        }

        public float[] FloatArray1
        {
            get
            {
                return _floatArray1;
            }

            set
            {
                if (_floatArray1 == null)
                {
                    BitHelper.ClearBit(ref _flags, 19);
                }
                else if (_floatArray2 == null)
                {
                    BitHelper.SetBit(ref _flags, 19);
                }

                _floatArray1 = value;
            }
        }

        public Color? Color3
        {
            get
            {
                return _color3;
            }

            set
            {
                if (value == null)
                {
                    BitHelper.ClearBit(ref _flags, 20);
                }
                else if (_color3 == null)
                {
                    BitHelper.SetBit(ref _flags, 20);
                }

                _color3 = value;
            }
        }

        public ushort[] ShortArray1
        {
            get
            {
                return _shortArray1;
            }

            set
            {
                if (_shortArray1 == null)
                {
                    BitHelper.ClearBit(ref _flags, 21);
                }
                else if (_shortArray1 == null)
                {
                    BitHelper.SetBit(ref _flags, 21);
                }

                _shortArray1 = value;
            }
        }

        public float[] FloatArray2
        {
            get
            {
                return _floatArray2;
            }

            set
            {
                if (_floatArray2 == null)
                {
                    BitHelper.ClearBit(ref _flags, 22);
                }
                else if (_floatArray2 == null)
                {
                    BitHelper.SetBit(ref _flags, 22);
                }

                _floatArray2 = value;
            }
        }

        public Color? Color4
        {
            get
            {
                return _color4;
            }

            set
            {
                if (_color4 == null)
                {
                    BitHelper.ClearBit(ref _flags, 23);
                }
                else if (_color4 == null)
                {
                    BitHelper.SetBit(ref _flags, 23);
                }

                _color4 = value;
            }
        }

        public float? Float1
        {
            get
            {
                return _float1;
            }

            set
            {
                if (_float1 == null)
                {
                    BitHelper.ClearBit(ref _flags, 25);
                }
                else if (_float1 == null)
                {
                    BitHelper.SetBit(ref _flags, 25);
                }

                _float1 = value;
            }
        }

        public float[] FloatArray3
        {
            get
            {
                return _floatArray3;
            }

            set
            {
                if (_floatArray3 == null)
                {
                    BitHelper.ClearBit(ref _flags, 26);
                }
                else if (_floatArray3 == null)
                {
                    BitHelper.SetBit(ref _flags, 26);
                }

                _floatArray3 = value;
            }
        }

        // Internal Methods
        internal void WriteInternal(BinaryWriter writer)
        {
            WriteImpl(writer);
        }

        // Private Methods
        private void Read(BinaryReader reader)
        {
            _id = reader.ReadUInt32();
            _flags = reader.ReadUInt32();
            for (int i = 16; i < 31; i++)
            {
                if (BitHelper.IsBitSet(_flags, i))
                {
                    switch (i)
                    {
                        case 16:
                            _color1 = Color.FromArgb(reader.ReadInt32());
                            break;
                        case 17:
                            _color2 = Color.FromArgb(reader.ReadInt32());
                            break;
                        case 18:
                            _textureID = reader.ReadUInt32();
                            break;
                        case 19:
                            _floatArray1 = reader.ReadFloatArray(5);
                            break;
                        case 20:
                            _color3 = Color.FromArgb(reader.ReadInt32());
                            break;
                        case 21:
                            _shortArray1 = reader.ReadUInt16Array(2);
                            break;
                        case 22:
                            _floatArray2 = reader.ReadFloatArray(5);
                            break;
                        case 23:
                            _color4 = Color.FromArgb(reader.ReadInt32());
                            break;
                        case 25:
                            _float1 = reader.ReadSingle();
                            break;
                        case 26:
                            _floatArray3 = reader.ReadFloatArray(2);
                            break;
                        default:
                            throw new NotImplementedException("Unknown material flag bit set");
                    }
                }
            }
        }

        private void WriteImpl(BinaryWriter writer)
        {
            writer.Write(_id);
            writer.Write(_flags);
            for (int i = 16; i < 31; i++)
            {
                if (BitHelper.IsBitSet(_flags, i))
                {
                    switch (i)
                    {
                        case 16:
                            writer.Write((Color)_color1);
                            break;
                        case 17:
                            writer.Write((Color)_color2);
                            break;
                        case 18:
                            writer.Write((uint)_textureID);
                            break;
                        case 19:
                            writer.Write(_floatArray1);
                            break;
                        case 20:
                            writer.Write((Color)_color3);
                            break;
                        case 21:
                            writer.Write(_shortArray1);
                            break;
                        case 22:
                            writer.Write(_floatArray2);
                            break;
                        case 23:
                            writer.Write((Color)_color4);
                            break;
                        case 25:
                            writer.Write((float)_float1);
                            break;
                        case 26:
                            writer.Write(_floatArray3);
                            break;
                        default:
                            throw new NotImplementedException("Unknown material flag bit set");
                    }
                }
            }
        }
    }
}
