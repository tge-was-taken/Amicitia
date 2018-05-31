namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.Drawing;
    using System.IO;
    using AmicitiaLibrary.Utilities;

    /// <summary>
    /// Holds internal data for an <see cref="RwMaterial"/> instance.
    /// </summary>
    internal class RwMaterialStructNode : RwNode
    {
        public int Field00 { get; set; }

        public Color Color { get; set; }

        public int Field08 { get; set; }

        public bool IsTextured { get; set; }

        public float Ambient { get; set; }

        public float Specular { get; set; }

        public float Diffuse { get; set; }

        /// <summary>
        /// Initialize RenderWare material data with default properties.
        /// </summary>
        public RwMaterialStructNode(RwNode parent = null)
            : base(RwNodeId.RwStructNode, parent)
        {
            Color = Color.White;
            IsTextured = true;
            Ambient = 1.0f;
            Specular = 1.0f;
            Diffuse = 1.0f;
        }

        internal RwMaterialStructNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
                : base(header)
        {
            Field00 = reader.ReadInt32();
            Color = Color.FromArgb(reader.ReadInt32());
            Field08 = reader.ReadInt32();
            IsTextured = reader.ReadInt32() == 1;
            Ambient = reader.ReadSingle();
            Specular = reader.ReadSingle();
            Diffuse = reader.ReadSingle();
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write( Field00 );
            writer.Write(Color);
            writer.Write( Field08 );
            writer.Write(IsTextured ? (int)1 : (int)0);
            writer.Write(Ambient);
            writer.Write(Specular);
            writer.Write(Diffuse);
        }
    }
}