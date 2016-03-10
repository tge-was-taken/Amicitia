namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Drawing;
    using System.IO;
    using AtlusLibSharp.Utilities;

    /// <summary>
    /// Holds internal data for an <see cref="RWMaterial"/> instance.
    /// </summary>
    internal class RWMaterialStruct : RWNode
    {
        private Color _color;
        private int _isTextured;
        private float _ambient;
        private float _specular;
        private float _diffuse;

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public bool IsTextured
        {
            get { return _isTextured == 1; }
            set
            {
                _isTextured = 0;

                if (value)
                {
                    _isTextured = 1;
                }
            }
        }

        public float Ambient
        {
            get { return _ambient; }
            set { _ambient = value; }
        }

        public float Specular
        {
            get { return _specular; }
            set { _specular = value; }
        }

        public float Diffuse
        {
            get { return _diffuse; }
            set { _diffuse = value; }
        }

        /// <summary>
        /// Initialize RenderWare material data with default properties.
        /// </summary>
        public RWMaterialStruct(RWNode parent = null)
            : base(RWType.Struct, parent)
        {
            Color = Color.White;
            IsTextured = true;
            Ambient = 1.0f;
            Specular = 1.0f;
            Diffuse = 1.0f;
        }

        internal RWMaterialStruct(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
                : base(header)
        {
            int unused1 = reader.ReadInt32();
            _color = Color.FromArgb(reader.ReadInt32());
            int unused2 = reader.ReadInt32();
            _isTextured = reader.ReadInt32();
            _ambient = reader.ReadSingle();
            _specular = reader.ReadSingle();
            _diffuse = reader.ReadSingle();
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(0); // unused
            writer.Write(_color);
            writer.Write(0); // unused
            writer.Write(_isTextured);
            writer.Write(Ambient);
            writer.Write(Specular);
            writer.Write(Diffuse);
        }
    }
}