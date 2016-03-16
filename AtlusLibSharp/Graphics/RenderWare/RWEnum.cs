namespace AtlusLibSharp.Graphics.RenderWare
{
    public enum RWNodeType : uint
    {
        None                        = 0x00000000,
        Struct                      = 0x00000001,
        String                      = 0x00000002,
        Extension                   = 0x00000003,
        TextureReference            = 0x00000006,
        Material                    = 0x00000007,
        MaterialList                = 0x00000008,
        World                       = 0x0000000B, // found in some map files
        FrameList                   = 0x0000000E,
        Geometry                    = 0x0000000F,
        Scene                       = 0x00000010,
        DrawCall                    = 0x00000014,
        TextureNative               = 0x00000015,
        GeometryList                = 0x0000001A,
        Animation                   = 0x0000001B,
        TextureDictionary           = 0x00000016,
        UVAnimationDictionary       = 0x0000002B,
        MeshMaterialSplitList             = 0x0000050E,
        SkyMipMapValue              = 0x00000110,
        SkinPlugin                  = 0x00000116,
        SceneNodeBoneMetadata         = 0x0000011E,
        UserDataPlugin              = 0x0000011F,
        Maestro2D                   = 0x000001B1, // ???
        RMDAnimationSetPlaceholder  = 0xF0F00001,
        RMDAnimationSetRedirect     = 0xF0F00003,
        RMDAnimationSetTerminator   = 0xF0F00004,
        RMDTransformOverride        = 0xF0F00005,
        RMDFrameLinkList            = 0xF0F00006,
        RMDVisibilityAnim           = 0xF0F00080,
        RMDAnimationSetCount        = 0xF0F000F0,
        RMDParticleList             = 0xF0F000E0,
        RMDParticleAnimation        = 0xF0F000E1,

        // Reserved for internal use
        RMDScene                    = 'R' << 8 | 'I' << 16 | 'G' << 24 | 0x00,
        RMDAnimationSet             = 'R' << 8 | 'I' << 16 | 'G' << 24 | 0x01,
    }

    public enum RWGeometryFlags : ushort
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

    public enum RWGeometryNativeFlag : byte
    {
        Default = 0x00,
        GeometryNative = 0x01,
        GeometryNativeInstance = 0x02
    }

    public enum RWRootBoneFlags : uint
    {
        SubHierarchy = 0x1,
        NoMatrices = 0x2,
        UpdateLocalMatrices = 0x100,
        UpdateGlobalMatrices = 0x200,
        LocalSpaceMatrices = 0x400
    }

    public enum RWHierarchyNodeFlag : uint
    {
        Deformable = 0,
        PopParentMatrix = 1,
        PushParentMatrix = 2
    }

    public enum RWDeviceID : ushort
    {
        Default = 0x0,
        D3D8 = 0x1,
        D3D9 = 0x2,
        PS2 = 0x6,
        XBOX = 0x8
    }

    public enum RWPlatformID : uint
    {
        XBOX = 0x05,
        D3D8 = 0x08,
        D3D9 = 0x09,
        PS2 = 'P' | 'S' << 8 | '2' << 16 | 0x00 << 24
    }

    public enum RWRasterFormats : uint
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
