namespace AtlusLibSharp.Graphics.RenderWare
{
    using AtlusLibSharp.Utilities;
    using System;
    using System.IO;

    /// <summary>
    /// Handles the building of RenderWare nodes when loading the nodes from a binary file.
    /// </summary>
    internal static class RWNodeFactory
    {
        internal struct RWNodeProcHeader
        {
            public RWType Type;
            public uint Size;
            public uint Version;
            public RWNode Parent;
        }

        internal static T GetNode<T>(RWNode parent, BinaryReader reader)
            where T : RWNode
        {
            return (T)GetNode(parent, reader);
        }

        internal static RWNode GetNode(RWNode parent, BinaryReader reader)
        {
            RWNodeProcHeader header = new RWNodeProcHeader
            {
                Type = (RWType)reader.ReadUInt32(),
                Size = reader.ReadUInt32(),
                Version = reader.ReadUInt32(),
                Parent = parent
            };

            switch (header.Type)
            {
                case RWType.Struct:
                    return GetStructNode(header, reader);

                case RWType.String:
                    return new RWString(header, reader);

                case RWType.Extension:
                    return new RWExtension(header, reader);

                case RWType.TextureReference:
                    return new RWTextureReference(header, reader);

                case RWType.Material:
                    return new RWMaterial(header, reader);

                case RWType.MaterialList:
                    return new RWMaterialList(header, reader);

                //case RWType.World:
                //    return new RWWorld(header, reader);

                case RWType.FrameList:
                    return new RWFrameList(header, reader);

                case RWType.Geometry:
                    return new RWGeometry(header, reader);

                case RWType.Clump:
                    return new RWClump(header, reader);

                case RWType.Atomic:
                    return new RWAtomic(header, reader);

                case RWType.TextureNative:
                    return new RWTextureNative(header, reader);

                case RWType.GeometryList:
                    return new RWGeometryList(header, reader);

                //case RWType.Animation:
                //    return new RWAnimation(header, reader);

                case RWType.TextureDictionary:
                    return new RWTextureDictionary(header, reader);

                case RWType.UVAnimationDictionary:
                    return new RWUVAnimationDictionary(header, reader);

                //case RWType.StripMeshPlugin:
                //    return new RWStripMeshPlugin(header, reader);

                case RWType.SkyMipMapValue:
                    return new RWSkyMipMapValue(header, reader);

                case RWType.SkinPlugin:
                    return new RWSkinPlugin(header, reader, parent.Parent as RWGeometry);

                case RWType.HierarchyAnimPlugin:
                    return new RWHierarchyAnimPlugin(header, reader);

                //case RWType.UserDataPlugin:
                //    return new RWUserDataPlugin(header, reader);

                //case RWType.Maestro2D:
                //    return new RWMaestro2D(header, reader);

                case RWType.RMDAnimationSetPlaceholder:
                    return new RMDAnimationSetPlaceholder(header);

                case RWType.RMDAnimationSetRedirect:
                    return new RMDAnimationSetRedirect(header, reader);

                case RWType.RMDAnimationSetTerminator:
                    return new RMDAnimationSetTerminator(header);

                //case RWType.RMDTransformOverride:
                //    return new RMDTransformOverride(header, reader);

                case RWType.RMDFrameLinkList:
                    return new RMDFrameLinkList(header, reader);

                //case RWType.RMDVisibilityAnim:
                //    return new RMDVisibilityAnim(header, reader);

                case RWType.RMDAnimationSetCount:
                    return new RMDAnimationSetCount(header, reader);

                //case RWType.RMDParticleList:
                    //return new RMDParticleList(header, reader);

                //case RWType.RMDParticleAnimation:
                //    return new RMDParticleAnimation(header, reader);

                default:
                    return new RWNode(header, reader);
            }
        }

        private static RWNode GetStructNode(RWNodeProcHeader header, BinaryReader reader)
        {
            switch (header.Parent.Type)
            {
                case RWType.Clump:
                    return new RWClumpStruct(header, reader);

                case RWType.FrameList:
                    return new RWFrameListStruct(header, reader);

                case RWType.GeometryList:
                    return new RWGeometryListStruct(header, reader);

                case RWType.Geometry:
                    return new RWGeometryStruct(header, reader);

                case RWType.Material:
                    return new RWMaterialStruct(header, reader);

                case RWType.MaterialList:
                    return new RWMaterialListStruct(header, reader);

                case RWType.TextureReference:
                    return new RWTextureReferenceStruct(header, reader);

                case RWType.Atomic:
                    return new RWAtomicStruct(header, reader);

                case RWType.TextureDictionary:
                    return new RWTextureDictionaryStruct(header, reader);

                case RWType.TextureNative:
                    return GetStructNodeParentIsTextureNative(header, reader);

                case RWType.Struct:
                    return GetStructNodeParentIsStruct(header, reader);

                case RWType.UVAnimationDictionary:
                    return new RWUVAnimationDictionaryStruct(header, reader);

                default:
                    return new RWNode(header, reader);

            }
        }

        private static RWNode GetStructNodeParentIsTextureNative(RWNodeProcHeader header, BinaryReader reader)
        {
            RWTextureNative txn = header.Parent as RWTextureNative;

            if (txn == null)
            {
                throw new InvalidDataException("Texture native shouldn't be null!");
            }

            if (txn.Struct == null)
            {
                return new RWTextureNativeStruct(header, reader);
            }
            else if (txn.Raster == null)
            {
                return new RWRaster(header, reader);
            }
            else
            {
                throw new InvalidDataException("Unexpected data.");
            }
        }

        private static RWNode GetStructNodeParentIsStruct(RWNodeProcHeader header, BinaryReader reader)
        {
            RWNode grandParent = header.Parent.Parent;

            // If the grandparent is null then I don't know what kind of node this is.
            if (grandParent == null)
                return new RWNode(header, reader);

            switch (grandParent.Type)
            {
                case RWType.TextureNative:
                    return GetStructNodeParentIsStructGrandParentIsTextureNative(header, reader);

                default:
                    throw new NotImplementedException();
            }
        }

        // award for longest method name goes to...
        private static RWNode GetStructNodeParentIsStructGrandParentIsTextureNative(RWNodeProcHeader header, BinaryReader reader)
        {
            RWRaster raster = header.Parent as RWRaster;

            if (raster == null)
            {
                throw new InvalidDataException("Raster shouldn't be null!");
            }

            if (raster.Info == null)
            {
                return new RWRasterInfo(header, reader);
            }
            else if (raster.Data == null)
            {
                return new RWRasterData(header, reader);
            }
            else
            {
                throw new InvalidDataException("Unexpected data!");
            }
        }
    }
}
