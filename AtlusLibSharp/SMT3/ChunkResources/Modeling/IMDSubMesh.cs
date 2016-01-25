namespace AtlusLibSharp.SMT3.ChunkResources.Modeling
{
    // TODO: Refactor this into an abstract base class
    public interface IMDSubMesh
    {
        // Properties
        int Type { get; }

        ushort MaterialID { get; }

        int UsedNodeCount { get; }

        ushort[] UsedNodeIndices { get; }

        MDMaterial Material { get; }

        MDNode[] UsedNodes { get; }
    }
}
