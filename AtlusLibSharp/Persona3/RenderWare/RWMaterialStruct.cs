using System.Drawing;
using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWMaterialStruct : RWNode
    {
        private int _isTextured;

        public int Unused1 { get; set; }
        public Color Color { get; set; }
        public int Unused2 { get; set; }

        public bool IsTextured
        {
            get { return _isTextured == 1; }
            set
            {
                if (value)
                    _isTextured = 1;
                else
                    _isTextured = 0;
            }
        }

        public float Ambient { get; set; }
        public float Specular { get; set; }
        public float Diffuse { get; set; }

        internal RWMaterialStruct(uint size, uint version, RWNode parent, BinaryReader reader)
                : base(RWType.Struct, size, version, parent)
        {
            Unused1 = reader.ReadInt32();
            Color = Color.FromArgb(reader.ReadInt32());
            Unused2 = reader.ReadInt32();
            _isTextured = reader.ReadInt32();
            Ambient = reader.ReadSingle();
            Specular = reader.ReadSingle();
            Diffuse = reader.ReadSingle();
        }

        public RWMaterialStruct(RWNode parent, bool isTextured = true, Color ?color = null, float ambient = 1.0f, float specular = 1.0f, float diffuse = 1.0f)
            : base(RWType.Struct)
        {
            Unused1 = 0;
            if (color != null)
                Color = (Color)color;
            else
                Color = Color.White;
            Unused2 = 0;
            IsTextured = isTextured;
            Ambient = ambient;
            Specular = specular;
            Diffuse = diffuse;
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write(Unused1);
            writer.Write(Color.ToArgb());
            writer.Write(Unused2);
            writer.Write(_isTextured);
            writer.Write(Ambient);
            writer.Write(Specular);
            writer.Write(Diffuse);
        }
    }
}