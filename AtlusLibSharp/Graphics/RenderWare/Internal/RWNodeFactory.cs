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
            public RWNodeType Type;
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
                Type = (RWNodeType)reader.ReadUInt32(),
                Size = reader.ReadUInt32(),
                Version = reader.ReadUInt32(),
                Parent = parent
            };

            switch (header.Type)
            {
                case RWNodeType.Struct:
                    return GetStructNode(header, reader);

                case RWNodeType.String:
                    return new RWString(header, reader);

                case RWNodeType.Extension:
                    return new RWExtension(header, reader);

                case RWNodeType.TextureReference:
                    return new RWTextureReference(header, reader);

                case RWNodeType.Material:
                    return new RWMaterial(header, reader);

                case RWNodeType.MaterialList:
                    return new RWMaterialList(header, reader);

                //case RWType.World:
                //    return new RWWorld(header, reader);

                case RWNodeType.FrameList:
                    return new RWSceneNodeList(header, reader);

                case RWNodeType.Geometry:
                    return new RWMesh(header, reader);

                case RWNodeType.Scene:
                    return new RWScene(header, reader);

                case RWNodeType.DrawCall:
                    return new RWDrawCall(header, reader);

                case RWNodeType.TextureNative:
                    return new RWTextureNative(header, reader);

                case RWNodeType.GeometryList:
                    return new RWMeshList(header, reader);

                //case RWType.Animation:
                //    return new RWAnimation(header, reader);

                case RWNodeType.TextureDictionary:
                    return new RWTextureDictionary(header, reader);

                case RWNodeType.UVAnimationDictionary:
                    return new RWUVAnimationDictionary(header, reader);

                //case RWType.StripMeshPlugin:
                //    return new RWStripMeshPlugin(header, reader);

                case RWNodeType.SkyMipMapValue:
                    return new RWSkyMipMapValue(header, reader);

                case RWNodeType.SkinPlugin:
                    return new RWSkinPlugin(header, reader, parent.Parent as RWMesh);

                case RWNodeType.SceneNodeBoneMetadata:
                    return new RWSceneNodeBoneMetadata(header, reader);

                //case RWType.UserDataPlugin:
                //    return new RWUserDataPlugin(header, reader);

                //case RWType.Maestro2D:
                //    return new RWMaestro2D(header, reader);

                case RWNodeType.RMDAnimationSet:
                    return new RMDAnimationSet(header, reader);

                case RWNodeType.RMDAnimationSetPlaceholder:
                    return new RMDAnimationSetPlaceholder(header);

                case RWNodeType.RMDAnimationSetRedirect:
                    return new RMDAnimationSetRedirect(header, reader);

                case RWNodeType.RMDAnimationSetTerminator:
                    return new RMDAnimationSetTerminator(header);

                //case RWType.RMDTransformOverride:
                //    return new RMDTransformOverride(header, reader);

                case RWNodeType.RMDFrameLinkList:
                    return new RMDFrameLinkList(header, reader);

                //case RWType.RMDVisibilityAnim:
                //    return new RMDVisibilityAnim(header, reader);

                case RWNodeType.RMDAnimationSetCount:
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
                case RWNodeType.Scene:
                    return new RWSceneStruct(header, reader);

                case RWNodeType.FrameList:
                    return new RWSceneNodeListStruct(header, reader);

                case RWNodeType.GeometryList:
                    return new RWGeometryListStruct(header, reader);

                case RWNodeType.Geometry:
                    return new RWMeshStruct(header, reader);

                case RWNodeType.Material:
                    return new RWMaterialStruct(header, reader);

                case RWNodeType.MaterialList:
                    return new RWMaterialListStruct(header, reader);

                case RWNodeType.TextureReference:
                    return new RWTextureReferenceStruct(header, reader);

                case RWNodeType.DrawCall:
                    return new RWDrawCallStruct(header, reader);

                case RWNodeType.TextureDictionary:
                    return new RWTextureDictionaryStruct(header, reader);

                case RWNodeType.TextureNative:
                    return GetStructNodeParentIsTextureNative(header, reader);

                case RWNodeType.Struct:
                    return GetStructNodeParentIsStruct(header, reader);

                case RWNodeType.UVAnimationDictionary:
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
                case RWNodeType.TextureNative:
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
