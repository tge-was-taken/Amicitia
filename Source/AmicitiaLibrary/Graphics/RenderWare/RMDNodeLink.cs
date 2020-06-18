using System.IO;
using System.Numerics;
using AmicitiaLibrary.IO;
using AmicitiaLibrary.Utilities;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    /// <summary>
    /// Used to eg. attach weapons to frames by specifying which frame on the source model attaches to a frame on the weapon model.
    /// <para>It also provides a matrix pivot point for the weapon (or any other object) to orient itself around.</para>
    /// </summary>
    public class RmdNodeLink : BinaryBase
    {
        public int SourceNodeId { get; set; }

        public int TargetNodeId { get; set; }

        /// <summary>
        /// Gets or sets the matrix pivot point of the attachment link.
        /// </summary>
        public Matrix4x4 Matrix { get; set; }

        /// <summary>
        /// Initialize a new empty <see cref="RmdNodeLink"/> instance.
        /// </summary>
        public RmdNodeLink()
        {
            SourceNodeId = 0;
            TargetNodeId = 0;
            Matrix = Matrix4x4.Identity;
        }

        public RmdNodeLink(byte[] data)
            : this(new MemoryStream(data))
        {
        }

        public RmdNodeLink(Stream stream, bool leaveOpen = false)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen) )
            {
                Read(reader);
            }
        }

        public RmdNodeLink(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                Read(reader);
            }
        }

        /// <summary>
        /// Initialize a <see cref="RmdNodeLink"/> by reading the data from a <see cref="Stream"/> using a <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/> used to read the data from the <see cref="Stream"/>.</param>
        internal RmdNodeLink(BinaryReader reader)
        {
            Read(reader);
        }

        internal void Read(BinaryReader reader)
        {
            SourceNodeId = reader.ReadInt32();
            TargetNodeId = reader.ReadInt32();
            Matrix = reader.ReadMatrix4();
        }

        /// <summary>
        /// Write the <see cref="RmdNodeLink"/> instance to the stream using a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> used to write to the stream.</param>
        internal override void Write(BinaryWriter writer)
        {
            writer.Write(SourceNodeId);
            writer.Write(TargetNodeId);
            writer.Write(Matrix, false);
        }
    }
}