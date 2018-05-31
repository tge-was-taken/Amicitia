using System.IO;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    public class RwAtomicSector : RwNode
    {
        public RwAtomicSectorHeader Header { get; set; }

        public RwExtensionNode Extension { get; set; }

        public RwAtomicSector( RwNode parent ) : base( RwNodeId.RwAtomicSector, parent )
        {
        }

        internal RwAtomicSector( RwNodeFactory.RwNodeHeader header, BinaryReader reader ) : base( header )
        {
            Header = RwNodeFactory.GetNode< RwAtomicSectorHeader >( this, reader );
            Extension = RwNodeFactory.GetNode< RwExtensionNode >( this, reader );
        }
    }
}