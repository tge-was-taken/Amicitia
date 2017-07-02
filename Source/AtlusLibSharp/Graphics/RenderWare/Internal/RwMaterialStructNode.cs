namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Drawing;
    using System.IO;
    using AtlusLibSharp.Utilities;

    /// <summary>
    /// Holds internal data for an <see cref="RwMaterial"/> instance.
    /// </summary>
    internal class RwMaterialStructNode : RwNode
    {
        public Color Color { get; set; }

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
            reader.Seek(4, SeekOrigin.Current);
            Color = Color.FromArgb(reader.ReadInt32());
            reader.Seek(4, SeekOrigin.Current);
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
            writer.Seek(4, SeekOrigin.Current);
            writer.Write(Color);
            writer.Seek(4, SeekOrigin.Current);
            writer.Write(IsTextured ? (int)1 : (int)0);
            writer.Write(Ambient);
            writer.Write(Specular);
            writer.Write(Diffuse);
        }
    }
}