using System.Collections.Generic;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    public class RwHAnimHierarchy
    {
        public RwHAnimHierarchyFlags Flags { get; set; }

        public List<RwHAnimNodeInfo> Nodes { get; internal set; }

        public RwHAnimHierarchy(RwHAnimHierarchyFlags flags, List<RwHAnimNodeInfo> nodes)
        {
            Flags = flags;
            Nodes = nodes;
        }
    }
}