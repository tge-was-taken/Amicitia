namespace AtlusLibSharp.PS2.Graphics
{
    public enum PS2FilterMode
    {
        None                = 0x00,
        Nearest             = 0x01,
        Linear              = 0x02,
        MipNearest          = 0x03,
        MipLinear           = 0x04,
        LinearMipNearest    = 0x05,
        LinearMipLinear     = 0x06
    }

    public enum PS2AddressingMode
    {
        None    = 0x00,
        Wrap    = 0x01,
        Mirror  = 0x02,
        Clamp   = 0x03
    }

    public enum PS2GIFMode
    {
        Packed  = 0x00,
        RegList = 0x01,
        Image   = 0x02,
        Image2  = 0x03
    }

    public enum PS2GIFReg
    {
        PRIM    = 0x00,
        RGBA    = 0x01,
        STQ     = 0x02,
        UV      = 0x03,
        XYZF2   = 0x04,
        XYZ2    = 0x05,
        TEX0_1  = 0x06,
        TEX0_2  = 0x07,
        CLAMP_1 = 0x08,
        CLAMP_2 = 0x09,
        FOG     = 0x0A,
        INVALID = 0x0B,
        XYZF3   = 0x0C,
        XYZ3    = 0x0D,
        AD      = 0x0E,
        NOP     = 0x0F
    }

    public enum PS2PixelFormat
    {
        PSMCT32  = 0x00,
        PSMCT24  = 0x01,
        PSMCT16  = 0x02,
        PSMCT16S = 0x0A,
        PSMT8    = 0x13,
        PSMT4    = 0x14,
        PSMT8H   = 0x1B,
        PSMT4HL  = 0x24,
        PSMT4HH  = 0x2C,
        PSMZ32   = 0x30,
        PSMZ24   = 0x31,
        PSMZ16   = 0x32,
        PSMZ16S  = 0x3A
    }

    public enum PS2ColorComponent
    {
        RGB  = 0x00,
        RGBA = 0x01
    }

    public enum PS2TextureFunction
    {
        Modulate    = 0x00,
        Decal       = 0x01,
        Highlight   = 0x02,
        Highlight2  = 0x03
    }

    public enum PS2CLUTStorageMode
    {
        CSM1 = 0x00,
        CSM2 = 0x01
    }

    public enum PS2TRXPOSPixelTransmissionOrder
    {
        UpLToLoR = 0x00,
        LoLToUpR = 0x01,
        UpRToLoL = 0x02,
        LoRToUpL = 0x03
    }

    public enum PS2TRXDIRTransmissionDirection
    {
        HostToLocal  = 0x00,
        LocalToHost  = 0x01,
        LocalToLocal = 0x02,
        Deactivated  = 0x03
    }

    public enum PS2VIFCommand : byte
    {
        NoOperation = 0x00,
        SetCycle = 0x01,
        SetOffset = 0x02,
        SetBase = 0x03,
        SetITOPS = 0x04,
        SetMode = 0x05,
        MaskPath = 0x06,
        SetMark = 0x07,
        FlushEnd = 0x10,
        Flush = 0x11,
        FlushAll = 0x13,
        ActMicro = 0x14, // MSCAL
        CntMicro = 0x17, // MSCNT
        ActMicroF = 0x15,
        SetMask = 0x20,
        SetRow = 0x30,
        SetCol = 0x31,
        LoadMicro = 0x4A,
        Direct = 0x50,
        DirectHL = 0x51,
        Unpack = 0x60,
        UnpackMask = 0x70
    }
}
