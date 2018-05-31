using System.Collections.Generic;

namespace AmicitiaLibrary.Graphics.SPR
{
    public interface ISprFile
    {
        int KeyFrameCount { get; }

        IList<SprSprite> KeyFrames { get; }

        int TextureCount { get; }

        IList<ITextureFile> Textures { get; }
    }
}