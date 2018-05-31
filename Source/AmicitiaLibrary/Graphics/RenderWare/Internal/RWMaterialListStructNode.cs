using System.IO;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    /// <summary>
    /// Holds internal data for an <see cref="RwMaterialListNode"/> instance.
    /// </summary>
    internal class RwMaterialListStructNode : RwNode
    {
        private int mMatCount;
        private int[] mMatReferences;

        public int MaterialCount
        {
            get { return mMatCount; }
        }

        public int[] MaterialReferences
        {
            get { return mMatReferences; }
        }

        internal RwMaterialListStructNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
                : base(header)
        {
            mMatCount = reader.ReadInt32();
            mMatReferences = new int[mMatCount];
            for (int i = 0; i < mMatCount; i++)
                mMatReferences[i] = reader.ReadInt32();
        }

        internal RwMaterialListStructNode(RwMaterialListNode listNode)
            : base(RwNodeId.RwStructNode, listNode)
        {
            mMatCount = listNode.Count;
            mMatReferences = new int[mMatCount];
            for (int i = 0; i < mMatCount; i++)
            {
                mMatReferences[i] = -1;
            }
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write(mMatCount);
            for (int i = 0; i < mMatCount; i++)
                writer.Write(mMatReferences[i]);
        }
    }
}