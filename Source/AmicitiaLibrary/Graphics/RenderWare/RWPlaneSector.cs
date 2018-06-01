using System.IO;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    public class RwPlaneSector : RwNode
    {
        public RwPlaneSectorHeader Header { get; set; }

        public RwPlaneSector( RwNode parent ) : base( RwNodeId.RwPlaneSector, parent )
        {
        }

        internal RwPlaneSector( RwNodeFactory.RwNodeHeader header, BinaryReader reader ) : base( header )
        {
            var endPosition = reader.BaseStream.Position + header.Size;

            Header = RwNodeFactory.GetNode<RwPlaneSectorHeader>( this, reader );

            while ( reader.BaseStream.Position < endPosition )
            {
                RwNodeFactory.GetNode( this, reader );
            }
        }

        protected internal override void WriteBody( BinaryWriter writer )
        {
            Header.Write( writer );
            foreach ( var node in Children )
            {
                node.Write( writer );
            }
        }
    }
}