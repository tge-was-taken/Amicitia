namespace AtlusLibSharp.Graphics.RenderWare
{
    using AtlusLibSharp.Utilities;
    using System;
    using System.IO;

    /// <summary>
    /// Handles the building of RenderWare nodes when loading the nodes from a binary file.
    /// </summary>
    internal static class RwNodeFactory
    {
        internal struct RwNodeHeader
        {
            public RwNodeId Id;
            public uint Size;
            public uint Version;
            public RwNode Parent;
        }

        internal static RwNodeId PeekNode(BinaryReader reader)
        {
            if ( ( reader.BaseStream.Position + 12 ) > reader.BaseStream.Length )
                return RwNodeId.None;

            var header = ReadHeader( reader, null );
            reader.Seek( -12, SeekOrigin.Current );
            return header.Id;
        }

        internal static void SkipNode( BinaryReader reader )
        {
            var header = ReadHeader( reader, null );
            reader.Seek( header.Size, SeekOrigin.Current );
        }

        internal static T GetNode<T>(RwNode parent, BinaryReader reader)
            where T : RwNode
        {
            return (T)GetNode(parent, reader);
        }

        internal static RwNodeHeader ReadHeader(BinaryReader reader, RwNode parent)
        {
            return new RwNodeHeader
            {
                Id = ( RwNodeId )reader.ReadUInt32(),
                Size = reader.ReadUInt32(),
                Version = reader.ReadUInt32(),
                Parent = parent
            };
        }

        internal static RwNode GetNode(RwNode parent, BinaryReader reader)
        {

            //Console.WriteLine("RWNode read at offset 0x{0}", reader.BaseStream.Position.ToString("X"));

            RwNodeHeader header = ReadHeader( reader, parent );

            //Console.WriteLine("Id: {0}        Size: {1}       Version: {2}\n", header.Id, header.Size, header.Version);

            switch (header.Id)
            {
                case RwNodeId.RwStructNode:
                    return GetStructNode(header, reader);

                case RwNodeId.RwStringNode:
                    return new RwStringNode(header, reader);

                case RwNodeId.RwExtensionNode:
                    return new RwExtensionNode(header, reader);

                case RwNodeId.RwTextureReferenceNode:
                    return new RwTextureReferenceNode(header, reader);

                case RwNodeId.RwMaterialNode:
                    return new RwMaterial(header, reader);

                case RwNodeId.RwMaterialListNode:
                    return new RwMaterialListNode(header, reader);

                case RwNodeId.RwWorldNode:
                    return new RwWorld(header, reader);

                case RwNodeId.RwFrameListNode:
                    return new RwFrameListNode(header, reader);

                case RwNodeId.RwGeometryNode:
                    return new RwGeometryNode(header, reader);

                case RwNodeId.RwClumpNode:
                    return new RwClumpNode(header, reader);

                case RwNodeId.RwAtomicNode:
                    return new RwAtomicNode(header, reader);

                case RwNodeId.RwTextureNativeNode:
                    return new RwTextureNativeNode(header, reader);

                case RwNodeId.RwGeometryListNode:
                    return new RwGeometryListNode(header, reader);

                case RwNodeId.RwAnimationNode:
                    return new RwAnimationNode(header, reader);

                case RwNodeId.RwTextureDictionaryNode:
                    return new RwTextureDictionaryNode(header, reader);

                //case RwNodeId.UVAnimationDictionaryNode:
                //    return new RwUVAnimationDictionary(header, reader);

                case RwNodeId.RwMeshListNode:
                    return new RwMeshListNode(header, reader);

                case RwNodeId.RwSkyMipMapValueNode:
                    return new RwSkyMipMapValueNode(header, reader);

                case RwNodeId.RwSkinNode:
                    return new RwSkinNode(header, reader, parent.Parent as RwGeometryNode);

                case RwNodeId.RwHAnimFrameExtensionNode:
                    return new RwHAnimFrameExtensionNode(header, reader);

                //case RWType.UserDataPluginNode:
                //    return new RWUserDataPlugin(header, reader);

                //case RWType.Maestro2DNode:
                //    return new RWMaestro2D(header, reader);

                case RwNodeId.RmdAnimation:
                    return new RmdAnimation(header, reader);

                case RwNodeId.RmdAnimationPlaceholderNode:
                    return new RmdAnimationPlaceholderNode(header);

                case RwNodeId.RmdAnimationInstanceNode:
                    return new RmdAnimationInstanceNode(header, reader);

                case RwNodeId.RmdAnimationTerminatorNode:
                    return new RmdAnimationTerminatorNode(header);

                //case RWType.RMDTransformOverride:
                //    return new RMDTransformOverride(header, reader);

                case RwNodeId.RmdNodeLinkListNode:
                    return new RmdNodeLinkListNode(header, reader);

                //case RWType.RMDVisibilityAnim:
                //    return new RMDVisibilityAnim(header, reader);

                case RwNodeId.RmdAnimationCountNode:
                    return new RmdAnimationCountNode(header, reader);

                //case RWType.RMDParticleList:
                //return new RMDParticleList(header, reader);

                //case RWType.RMDParticleAnimation:
                //    return new RMDParticleAnimation(header, reader);

                case RwNodeId.RwPlaneSector:
                    return new RwPlaneSector( header, reader );

                case RwNodeId.RwAtomicSector:
                    return new RwAtomicSector( header, reader );

                default:
                    return new RwNode(header, reader);
            }
        }

        private static RwNode GetStructNode(RwNodeHeader header, BinaryReader reader)
        {
            switch (header.Parent.Id)
            {
                case RwNodeId.RwClumpNode:
                    return new RwClumpStructNode(header, reader);

                case RwNodeId.RwFrameListNode:
                    return new RwFrameListStructNode(header, reader);

                case RwNodeId.RwGeometryListNode:
                    return new RwGeometryListStructNode(header, reader);

                case RwNodeId.RwGeometryNode:
                    return new RwGeometryStructNode(header, reader);

                case RwNodeId.RwMaterialNode:
                    return new RwMaterialStructNode(header, reader);

                case RwNodeId.RwMaterialListNode:
                    return new RwMaterialListStructNode(header, reader);

                case RwNodeId.RwTextureReferenceNode:
                    return new RwTextureReferenceStruct(header, reader);

                case RwNodeId.RwAtomicNode:
                    return new RwAtomicStructNode(header, reader);

                case RwNodeId.RwTextureDictionaryNode:
                    return new RwTextureDictionaryStructNode(header, reader);

                case RwNodeId.RwTextureNativeNode:
                    return GetStructNodeParentIsTextureNative(header, reader);

                case RwNodeId.RwStructNode:
                    return GetStructNodeParentIsStruct(header, reader);

                case RwNodeId.RwUVAnimationDictionaryNode:
                    return new RWUVAnimationDictionaryStructNode(header, reader);

                case RwNodeId.RwWorldNode:
                    return new RwWorldHeader( header, reader );

                case RwNodeId.RwPlaneSector:
                    return new RwPlaneSectorHeader( header, reader );

                case RwNodeId.RwAtomicSector:
                    return new RwAtomicSectorHeader( header, reader );

                default:
                    return new RwNode(header, reader);

            }
        }

        private static RwNode GetStructNodeParentIsTextureNative(RwNodeHeader header, BinaryReader reader)
        {
            RwTextureNativeNode txn = header.Parent as RwTextureNativeNode;

            if (txn == null)
            {
                throw new InvalidDataException("Texture native shouldn't be null!");
            }

            if (txn.StructNode == null)
            {
                return new RwTextureNativeStructNode(header, reader);
            }
            else if (txn.RasterStructNode == null)
            {
                return new RwRasterStructNode(header, reader);
            }
            else
            {
                throw new InvalidDataException("Unexpected data.");
            }
        }

        private static RwNode GetStructNodeParentIsStruct(RwNodeHeader header, BinaryReader reader)
        {
            RwNode grandParent = header.Parent.Parent;

            // If the grandparent is null then I don't know what kind of node this is.
            if (grandParent == null)
                return new RwNode(header, reader);

            switch (grandParent.Id)
            {
                case RwNodeId.RwTextureNativeNode:
                    return GetStructNodeParentIsStructGrandParentIsTextureNative(header, reader);

                default:
                    throw new NotImplementedException();
            }
        }

        private static RwNode GetStructNodeParentIsStructGrandParentIsTextureNative(RwNodeHeader header, BinaryReader reader)
        {
            RwRasterStructNode rasterStructNode = header.Parent as RwRasterStructNode;

            if (rasterStructNode == null)
            {
                throw new InvalidDataException("RasterStructNode shouldn't be null!");
            }

            if (rasterStructNode.InfoStructNode == null)
            {
                return new RwRasterInfoStructNode(header, reader);
            }
            else if (rasterStructNode.DataStructNode == null)
            {
                return new RwRasterDataStructNode(header, reader);
            }
            else
            {
                throw new InvalidDataException("Unexpected data!");
            }
        }
    }
}
