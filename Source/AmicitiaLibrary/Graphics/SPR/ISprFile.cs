using System.Collections.Generic;

namespace AmicitiaLibrary.Graphics.SPR
{
    public interface ISprFile
    {
        int KeyFrameCount { get; }

        IList<SprSprite> Sprites { get; }

        int TextureCount { get; }

        IList<ITextureFile> Textures { get; }
    }
}