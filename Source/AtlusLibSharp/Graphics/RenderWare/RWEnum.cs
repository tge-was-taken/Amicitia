using System;

namespace AtlusLibSharp.Graphics.RenderWare
{
    public enum RwNodeId : uint
    {
        None                        = 0x00000000,
        RwStructNode                      = 0x00000001,
        RwStringNode                      = 0x00000002,
        RwExtensionNode                   = 0x00000003,
        RwTextureReferenceNode            = 0x00000006,
        RwMaterialNode                    = 0x00000007,
        RwMaterialListNode                = 0x00000008,
        RwAtomicSector                    = 0x00000009,
        RwPlaneSector                     = 0x0000000A,
        RwWorldNode                       = 0x0000000B, // found in some map files
        RwFrameListNode                   = 0x0000000E,
        RwGeometryNode                    = 0x0000000F,
        RwClumpNode                       = 0x00000010,
        RwAtomicNode                    = 0x00000014,
        RwTextureNativeNode               = 0x00000015,
        RwGeometryListNode                = 0x0000001A,
        RwAnimationNode                   = 0x0000001B,
        RwTextureDictionaryNode           = 0x00000016,
        RwUVAnimationDictionaryNode       = 0x0000002B,
        RwMeshListNode       = 0x0000050E,
        RwSkyMipMapValueNode              = 0x00000110,
        RwSkinNode                  = 0x00000116,
        RwHAnimFrameExtensionNode       = 0x0000011E,
        RwUserDataPluginNode              = 0x0000011F,
        RwMaestro2DNode                   = 0x000001B1, // ???
        RmdAnimationPlaceholderNode  = 0xF0F00001,
        RmdAnimationInstanceNode     = 0xF0F00003,
        RmdAnimationTerminatorNode   = 0xF0F00004,
        RmdTransformOverrideNode        = 0xF0F00005,
        RmdNodeLinkListNode            = 0xF0F00006,
        RmdVisibilityAnimNode           = 0xF0F00080,
        RmdAnimationCountNode        = 0xF0F000F0,
        RmdParticleListNode             = 0xF0F000E0,
        RmdParticleAnimationNode        = 0xF0F000E1,

        // Reserved for internal use
        RmdSceneNode             = 'R' << 8 | 'I' << 16 | 'G' << 24 | 0x00,
        RmdAnimation = 'R' << 8 | 'I' << 16 | 'G' << 24 | 0x01,
        RmdAuthor                = 'R' << 8 | 'I' << 16 | 'G' << 24 | 'R' << 32
    }

    [Flags]
    public enum RwGeometryFlags : ushort
    {
        Default = 0x0000,
        CanTriStrip = 0x0001,
        HasVertices = 0x0002,
        HasTexCoord1 = 0x0004,
        HasColors = 0x0008,
        HasNormals = 0x0010,
        HasVertexLighting = 0x0020,
        ModulateMatColor = 0x0040,
        HasTexCoord2 = 0x0080
    }

    public enum RwGeometryNativeFlag : byte
    {
        Default = 0x00,
        GeometryNative = 0x01,
        GeometryNativeInstance = 0x02
    }

    public enum RwHAnimHierarchyFlags : uint
    {
        SubHierarchy = 0x1,
        NoMatrices = 0x2,
        UpdateLocalMatrices = 0x100,
        UpdateGlobalMatrices = 0x200,
        LocalSpaceMatrices = 0x400
    }

    public enum RwHierarchyNodeFlag : uint
    {
        Deformable = 0,
        PopParentMatrix = 1,
        PushParentMatrix = 2
    }

    public enum RwDeviceId : ushort
    {
        Default = 0x0,
        D3D8 = 0x1,
        D3D9 = 0x2,
        PS2 = 0x6,
        Xbox = 0x8
    }

    public enum RwPlatformId : uint
    {
        Xbox = 0x05,
        D3D8 = 0x08,
        D3D9 = 0x09,
        PS2 = 'P' | 'S' << 8 | '2' << 16
    }

    [Flags]
    public enum RwRasterFormats : uint
    {
        Default = 0x00000,
        Unknown = 0x00004, // game will hang if not set
        Format1555 = 0x00100,
        Format565 = 0x00200, // 5 bit R, 4 bit G, 5 bit B
        Format4444 = 0x00300, // 4 bit RGBA
        FormatLum8 = 0x00400, // 8 bit greyscale
        Format8888 = 0x00500, // 8 bit RGBA
        Format888 = 0x00600, // 8 bit RGB + 1 byte padding
        Format555 = 0x00A00, // 5 bit RGB
        AutoMipMap = 0x01000, // auto generate mipmaps (not supported by ps2)
        Pal8 = 0x02000, // 256 colors
        Pal4 = 0x04000, // 16 colors
        MipMap = 0x08000, // has mip maps
        Swizzled = 0x10000, // the images are "swizzled"
        HasHeaders = 0x20000  // the image data is preceeded by headers
    }
}
