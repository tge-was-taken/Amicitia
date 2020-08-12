using System;

namespace AmicitiaLibrary.Graphics.DDS
{
    [Flags]
    public enum DDSHeaderCaps
    {
        Complex = 0x8,
        MipMap  = 0x400000,
        Texture = 0x1000,
    }
}