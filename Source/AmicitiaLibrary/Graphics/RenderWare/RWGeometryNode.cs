namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.Numerics;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;

    /// <summary>
    /// Encapsulates a RenderWare mesh and all of its corresponding data structures.
    /// </summary>
    public class RwGeometryNode : RwNode
    {
        private RwGeometryStructNode mStructNode;
        private RwExtensionNode mExtensionNode;

        #region Properties

        /*****************************************/
        /* RWGeometryStruct forwarded properties */
        /*****************************************/

        /// <summary>
        /// Gets if the mesh contains any vertices.
        /// </summary>
        public bool HasVertices => mStructNode.HasVertices;

        /// <summary>
        /// Gets if the mesh contains any vertex normals.
        /// </summary>
        public bool HasNormals => mStructNode.HasNormals;

        /// <summary>
        /// Gets if the mesh contains any vertex colors.
        /// </summary>
        public bool HasColors => mStructNode.HasColors;

        /// <summary>
        /// Gets if the mesh contains any vertex texture coordinates.
        /// </summary>
        public bool HasTextureCoordinates => mStructNode.HasTexCoords;

        /// <summary>
        /// Gets the <see cref="RwGeometryFlags"/> of the mesh.
        /// </summary>
        public RwGeometryFlags Flags
        {
            get => mStructNode.Flags;
            set => mStructNode.Flags = value;
        }

        /// <summary>
        /// Gets the number of texture coordinate channels in the mesh.
        /// </summary>
        public int TextureCoordinateChannelCount => mStructNode.TextureCoordinateChannelCount;

        /// <summary>
        /// Gets the <see cref="RwGeometryNativeFlag"/> of the mesh.
        /// </summary>
        public RwGeometryNativeFlag NativeFlag => mStructNode.NativeFlag;

        /// <summary>
        /// Gets the number of triangles in the mesh.
        /// </summary>
        public int TriangleCount => mStructNode.TriangleCount;

        /// <summary>
        /// Gets the number of vertices in the mesh.
        /// </summary>
        public int VertexCount => mStructNode.VertexCount;

        /// <summary>
        /// Gets the array of vertex colors in the mesh.
        /// </summary>
        public Color[] Colors
        {
            get => mStructNode.Colors;
            set => mStructNode.Colors = value;
        }

        /// <summary>
        /// Gets the array of texture coordinate channels in the mesh.
        /// </summary>
        public Vector2[][] TextureCoordinateChannels
        {
            get => mStructNode.TextureCoordinateChannels;
            set => mStructNode.TextureCoordinateChannels = value;
        }

        /// <summary>
        /// Gets the array of triangles in the mesh.
        /// </summary>
        public RwTriangle[] Triangles
        {
            get => mStructNode.Triangles;
            set => mStructNode.Triangles = value;
        }

        /// <summary>
        /// Gets the bounding sphere of the mesh.
        /// </summary>
        public RwBoundingSphere BoundingSphere
        {
            get => mStructNode.BoundingSphere;
            set => mStructNode.BoundingSphere = value;
        }

        /// <summary>
        /// Gets the array of vertices in the mesh.
        /// </summary>
        public Vector3[] Vertices
        {
            get => mStructNode.Vertices;
            set => mStructNode.Vertices = value;
        }

        /// <summary>
        /// Gets the array of vertex normals in the mesh.
        /// </summary>
        public Vector3[] Normals
        {
            get => mStructNode.Normals;
            set => mStructNode.Normals = value;
        }

        /***************************************/
        /* RWMaterialList forwarded properties */
        /***************************************/

        /// <summary>
        /// Gets the number of materials in the mesh.
        /// </summary>
        public int MaterialCount => Materials.Count;

        /// <summary>
        /// Gets the array of materials in the mesh.
        /// </summary>
        public RwMaterialListNode Materials { get; }

        /************************************/
        /* RWExtension forwarded properties */
        /************************************/

        /// <summary>
        /// Gets the mesh material split data.
        /// </summary>
        public RwMeshListNode MeshListNode
        {
            get => mExtensionNode.FindChild<RwMeshListNode>( RwNodeId.RwMeshListNode );
            set => mExtensionNode.AddOrReplaceChild( value );
        }

        public RwSkinNode SkinNode
        {
            get => mExtensionNode.FindChild<RwSkinNode>( RwNodeId.RwSkinNode );
            set => mExtensionNode.AddOrReplaceChild( value );
        }

        /// <summary>
        /// Gets the extension nodes of the mesh.
        /// </summary>
        public List<RwNode> ExtensionNodes => mExtensionNode.Children;

        /****************************************************************************************/
        /* TODO: add the skin plugin and mesh strip plugin here instead of the extension itself */
        /****************************************************************************************/

        #endregion

        /// <summary>
        /// Initializer only to be called <see cref="RwNodeFactory"/>
        /// </summary>
        internal RwGeometryNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
                : base(header)
        {
            mStructNode = RwNodeFactory.GetNode<RwGeometryStructNode>(this, reader);
            Materials = RwNodeFactory.GetNode<RwMaterialListNode>(this, reader);
            mExtensionNode = RwNodeFactory.GetNode<RwExtensionNode>(this, reader);
        }

        public RwGeometryNode(RwNode parent, Assimp.Mesh mesh, Assimp.Material material, RwFrameListNode frameList, Matrix4x4[] inverseBoneMatrices, out bool singleWeight)
            : base(RwNodeId.RwGeometryNode, parent)
        {
            bool forceSingleWeight = inverseBoneMatrices == null;
            mStructNode = new RwGeometryStructNode(this, mesh, frameList, forceSingleWeight, out byte[][] skinBoneIndices, out float[][] skinBoneWeights, out singleWeight);
            Materials = new RwMaterialListNode(this, material);

            //var materialSplit = new RwMeshListNode( this );
            if ( !singleWeight )
            {
                var skinPlugin = new RwSkinNode( skinBoneIndices, skinBoneWeights, inverseBoneMatrices );
                //mExtensionNode = new RwExtensionNode( this, skinPlugin, materialSplit );
                mExtensionNode = new RwExtensionNode( this, skinPlugin );
            }
            else
            {
                //mExtensionNode = new RwExtensionNode( this, materialSplit );
                mExtensionNode = new RwExtensionNode( this );
            }
        }

        public void SetVertexColors( Color color )
        {
            Colors = new Color[VertexCount];
            for ( int i = 0; i < Colors.Length; i++ )
                Colors[i] = color;
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            mStructNode.Write(writer);
            Materials.Write(writer);
            mExtensionNode.Write(writer);
        }
    }
}