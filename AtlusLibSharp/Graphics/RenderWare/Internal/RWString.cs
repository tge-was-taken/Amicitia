namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.IO;
    using AtlusLibSharp.Utilities;

    /// <summary>
    /// Represents a RenderWare string node.
    /// </summary>
    internal class RWString : RWNode
    {
        private string _value;

        /// <summary>
        /// Gets or sets the string value for this RenderWare string node.
        /// </summary>
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Initializer only to be called in <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RWString(RWNodeFactory.RWNodeInfo header, BinaryReader reader)
                : base(header)
        {
            Value = reader.ReadCString((int)Size);
        }

        /// <summary>
        /// Initialize a RenderWare string node with a string value.
        /// </summary>
        /// <param name="value">String value to initialize the string node with.</param>
        /// <param name="parent">Parent of the string node. Value is null if not specified.</param>
        public RWString(string value, RWNode parent = null)
            : base(RWNodeType.String, parent)
        {
            Value = value;
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.WriteCString(Value);
            writer.AlignPosition(4);
        }
    }
}