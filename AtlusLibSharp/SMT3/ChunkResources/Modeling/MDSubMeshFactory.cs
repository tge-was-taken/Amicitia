namespace AtlusLibSharp.SMT3.ChunkResources.Modeling
{
    using System;
    using System.IO;

    internal static class MDSubMeshFactory
    {
        // Methods
        public static IMDSubMesh Get(MDChunk model, MDNode node, BinaryReader reader)
        {
            IntPtr ptr = (IntPtr)reader.BaseStream.Position;
            uint type = reader.ReadUInt32();

            switch (type)
            {
                case 1:
                    return new MDSubMeshType1(model, node, ptr, reader);
                case 2:
                    return new MDSubMeshType2(model, node, ptr, reader);
                default:
                    return null;
            }
        }
    }
}
