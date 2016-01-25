namespace AtlusLibSharp.Persona3.RenderWare
{
    public enum RWType : uint
    {
        None = 0x00,
        Struct = 0x1,
        String = 0x2,
        Extension = 0x3,
        TextureReference = 0x6,
        Material = 0x7,
        MaterialList = 0x8,
        World = 0xB, // found in some map files
        FrameList = 0xE,
        Geometry = 0xF,
        Clump = 0x10,
        Atomic = 0x14,
        TextureNative = 0x15,
        GeometryList = 0x1A,
        Animation = 0x1B,
        TextureDictionary = 0x16,
        UVAnimDictionary = 0x2B,
        StripMeshPlugin = 0x50E,
        SkyMipMapValue = 0x110,
        SkinPlugin = 0x116,
        HierarchyAnimPlugin = 0x11E,
        UserDataPlugin = 0x11F,
        Maestro2D = 0x1B1, // ???
        AnimSetDummy = 0xF0F00001,
        AnimSetReference = 0xF0F00003,
        AnimSetTerminator = 0xF0F00004,
        TransformOverride = 0xF0F00005,
        AtomicMatrixPlugin = 0xF0F00006,
        VisibilityAnim = 0xF0F00080,
        AnimSetCount = 0xF0F000F0,
        ParticleList = 0xF0F000E0,
        ParticleAnimation = 0xF0F000E1
    }

    public enum RWGeometryFlags : ushort
    {
        Default = 0x0000,
        CanTriStrip = 0x0001,
        HasVertexPosition = 0x0002,
        HasTexCoord1 = 0x0004,
        HasVertexColor = 0x0008,
        HasVertexNormal = 0x0010,
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

    public enum RWHierarchyAnimFlag : uint
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
        XBOX = 0x5,
        D3D8 = 0x8,
        D3D9 = 0x9,
        PS2 = 0x325350
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
