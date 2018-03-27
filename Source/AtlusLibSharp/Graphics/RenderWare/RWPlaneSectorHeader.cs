using System.IO;

namespace AtlusLibSharp.Graphics.RenderWare
{
    public class RwPlaneSectorHeader : RwNode
    {
        public int SectorType { get; set; }

        public float Value { get; set; }

        public bool LeftIsWorldSector { get; set; }

        public bool RightIsWorldSector { get; set; }

        public float LeftValue { get; set; }

        public float RightValue { get; set; }

        public RwPlaneSectorHeader( RwNode parent ) : base( RwNodeId.RwStructNode, parent )
        {
        }

        internal RwPlaneSectorHeader( RwNodeFactory.RwNodeHeader header, BinaryReader reader ) : base( header )
        {
            SectorType = reader.ReadInt32();
            Value = reader.ReadSingle();
            LeftIsWorldSector = reader.ReadInt32() == 1;
            RightIsWorldSector = reader.ReadInt32() == 1;
            LeftValue = reader.ReadSingle();
            RightValue = reader.ReadSingle();
        }
    }
}