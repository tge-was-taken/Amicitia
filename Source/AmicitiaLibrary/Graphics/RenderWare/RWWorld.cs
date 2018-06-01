using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    public class RwWorld : RwNode
    {
        public RwWorldHeader Header { get; set; }

        public RwMaterialListNode Materials { get; set; }

        public RwPlaneSector PlaneSector { get; set; }

        public RwExtensionNode Extension { get; set; }

        public RwWorld(RwNode parent ) : base( RwNodeId.RwWorldNode, parent )
        {
        }

        internal RwWorld( RwNodeFactory.RwNodeHeader header, BinaryReader reader ) : base( header )
        {
            Read( reader );
        }

        public RwWorld( Stream stream, bool leaveOpen = false ) : base(RwNodeId.RwWorldNode)
        {
            using ( var reader = new BinaryReader( stream, Encoding.Default, leaveOpen ) )
                Read( reader );
        }

        private void Read( BinaryReader reader )
        {
            Header = RwNodeFactory.GetNode<RwWorldHeader>( this, reader );
            Materials = RwNodeFactory.GetNode<RwMaterialListNode>( this, reader );
            PlaneSector = RwNodeFactory.GetNode<RwPlaneSector>( this, reader );
            Extension = RwNodeFactory.GetNode<RwExtensionNode>( this, reader );
        }

        protected internal override void WriteBody( BinaryWriter writer )
        {
            Header.Write( writer );
            Materials.Write( writer );
            PlaneSector.Write( writer );
            Extension.Write( writer );
        }

        public void ExportToObj( string path )
        {
            using ( var writer = File.CreateText( path ) )
            {
                var mtllibName = Path.GetFileNameWithoutExtension( path ) + ".mtl";
                writer.WriteLine( $"mtllib {mtllibName}" );

                // Write mtl
                var mtllibPath = Path.Combine( Path.GetDirectoryName( path ), mtllibName );
                using ( var mtlWriter = File.CreateText( mtllibPath ) )
                {
                    for ( var i = 0; i < Materials.Count; i++ )
                    {
                        var material = Materials[ i ];
                        mtlWriter.WriteLine( $"newmtl Material{i}" );
                        if ( material.IsTextured )
                            mtlWriter.WriteLine( $"map_Kd {material.TextureReferenceNode.Name + ".png"}" );
                    }
                }

                // Write meshes
                var atomicSectors = new List< RwAtomicSector >();

                void RecursivelyFindAtomicSectors( RwNode node )
                {
                    if ( node.Id == RwNodeId.RwAtomicSector )
                        atomicSectors.Add( ( RwAtomicSector ) node );

                    foreach ( var child in node.Children )
                        RecursivelyFindAtomicSectors( child );
                }

                RecursivelyFindAtomicSectors( this );

                int vertexBaseIndex = 0;

                for ( var i = 0; i < atomicSectors.Count; i++ )
                {
                    var atomicSector = atomicSectors[ i ];
                    var header = atomicSector.Header;
                    if ( header.VertexCount == 0 )
                        continue;

                    foreach ( var pos in header.Positions )
                    {
                        writer.WriteLine(
                            $"v {pos.X.ToString( CultureInfo.InvariantCulture )} {pos.Y.ToString( CultureInfo.InvariantCulture )} {pos.Z.ToString( CultureInfo.InvariantCulture )}" );
                    }

                    if ( header.TextureCoordinateChannels.Length > 0 )
                    {
                        foreach ( var tex in header.TextureCoordinateChannels[ 0 ] )
                        {
                            writer.WriteLine(
                                $"vt {tex.X.ToString( CultureInfo.InvariantCulture )} {( 1f - tex.Y ).ToString( CultureInfo.InvariantCulture )}" );
                        }
                    }

                    writer.WriteLine( $"g AtomicSector{i}" );

                    int curMatId = -1;
                    foreach ( var triangle in header.Triangles.OrderBy( x => x.MatId ) )
                    {
                        if ( triangle.MatId != curMatId )
                        {
                            writer.WriteLine( $"usemtl Material{triangle.MatId}" );
                            curMatId = triangle.MatId;
                        }

                        writer.WriteLine( "f {0}/{0} {1}/{1} {2}/{2}", triangle.A + 1 + vertexBaseIndex, triangle.B + 1 + vertexBaseIndex,
                                          triangle.C + 1 + vertexBaseIndex );
                    }

                    vertexBaseIndex += header.VertexCount;
                }
            }
        }

        public void ExportToDae( string path )
        {
            var atomicSectors = new List<RwAtomicSector>();

            void RecursivelyFindAtomicSectors( RwNode node )
            {
                if ( node.Id == RwNodeId.RwAtomicSector )
                    atomicSectors.Add( ( RwAtomicSector )node );

                foreach ( var child in node.Children )
                    RecursivelyFindAtomicSectors( child );
            }

            RecursivelyFindAtomicSectors( this );

            if ( atomicSectors.Count == 0 )
                return;

            var aiScene = new Assimp.Scene();
            for ( var i = 0; i < Materials.Count; i++ )
            {
                var material = Materials[ i ];
                var aiMaterial = new Assimp.Material();

                if ( material.IsTextured )
                {
                    // TextureDiffuse
                    var texture = material.TextureReferenceNode;
                    aiMaterial.TextureDiffuse = new Assimp.TextureSlot(
                        texture.Name + ".png", Assimp.TextureType.Diffuse, 0, Assimp.TextureMapping.FromUV, 0, 0, Assimp.TextureOperation.Add,
                        Assimp.TextureWrapMode.Wrap, Assimp.TextureWrapMode.Wrap, 0 );
                }

                // Name
                aiMaterial.Name = $"Material{i}";
                if ( material.IsTextured )
                    aiMaterial.Name = material.TextureReferenceNode.Name;

                aiMaterial.ShadingMode = Assimp.ShadingMode.Phong;
                aiScene.Materials.Add( aiMaterial );
            }

            for ( var i = 0; i < atomicSectors.Count; i++ )
            {
                var atomicSector = atomicSectors[ i ];
                var header = atomicSector.Header;
                if ( header.VertexCount == 0 )
                    continue;

                foreach ( var materialGroup in header.Triangles.GroupBy(x => x.MatId) )
                {
                    var materialId = materialGroup.Key;

                    var aiMesh = new Assimp.Mesh( $"AtomicSector{i}_Material{materialId}", Assimp.PrimitiveType.Triangle );
                    aiMesh.MaterialIndex = materialId;

                    foreach ( var triangle in materialGroup )
                    {
                        var pos1 = header.Positions[ triangle.A ];
                        var pos2 = header.Positions[triangle.B];
                        var pos3 = header.Positions[triangle.C];

                        aiMesh.Vertices.Add( new Assimp.Vector3D( pos1.X, pos1.Y, pos1.Z ) );                      
                        aiMesh.Vertices.Add( new Assimp.Vector3D( pos2.X, pos2.Y, pos2.Z ) );                     
                        aiMesh.Vertices.Add( new Assimp.Vector3D( pos3.X, pos3.Y, pos3.Z ) );

                        if ( header.TextureCoordinateChannels != null && header.TextureCoordinateChannels.Length > 0 )
                        {
                            var tex1 = header.TextureCoordinateChannels[ 0 ][ triangle.A ];
                            var tex2 = header.TextureCoordinateChannels[ 0 ][ triangle.B ];
                            var tex3 = header.TextureCoordinateChannels[ 0 ][ triangle.C ];

                            aiMesh.TextureCoordinateChannels[0].Add( new Assimp.Vector3D( tex1.X, 1f - tex1.Y, 0 ) );
                            aiMesh.TextureCoordinateChannels[0].Add( new Assimp.Vector3D( tex2.X, 1f - tex2.Y, 0 ) );
                            aiMesh.TextureCoordinateChannels[0].Add( new Assimp.Vector3D( tex3.X, 1f - tex3.Y, 0 ) );
                        }

                        if ( header.Colors != null && header.Colors.Length > 0 )
                        {
                            var color1 = header.Colors[ triangle.A ];
                            var color2 = header.Colors[ triangle.B ];
                            var color3 = header.Colors[ triangle.C ];

                            aiMesh.VertexColorChannels[0].Add( new Assimp.Color4D( (float)color1.R / 255f, ( float )color1.G / 255f, ( float )color1.B / 255f, ( float )color1.A / 255f ) );
                            aiMesh.VertexColorChannels[0].Add( new Assimp.Color4D( ( float )color2.R / 255f, ( float )color2.G / 255f, ( float )color2.B / 255f, ( float )color2.A / 255f ) );
                            aiMesh.VertexColorChannels[0].Add( new Assimp.Color4D( ( float )color3.R / 255f, ( float )color3.G / 255f, ( float )color3.B / 255f, ( float )color3.A / 255f ) );
                        }
                    }

                    for ( int j = 0; j < aiMesh.VertexCount; j += 3 )
                    {
                        aiMesh.Faces.Add( new Assimp.Face( new[] { j, j + 1, j + 2 } ) );
                    }

                    aiScene.Meshes.Add( aiMesh );
                }
            }

            aiScene.RootNode = new Assimp.Node( "RootNode" );
            aiScene.RootNode.MeshIndices.AddRange( Enumerable.Range( 0, aiScene.Meshes.Count ) );

            var aiContext = new Assimp.AssimpContext();
            aiContext.ExportFile( aiScene, path, "collada",
                                  Assimp.PostProcessSteps.GenerateSmoothNormals | Assimp.PostProcessSteps.JoinIdenticalVertices |
                                  Assimp.PostProcessSteps.ImproveCacheLocality );
        }
    }
}
