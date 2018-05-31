using System.Collections.Generic;
using AmicitiaLibrary.Graphics.TMX;

namespace AmicitiaLibrary.Graphics.SPR
{
    public interface ISprFile
    {
        int KeyFrameCount { get; }

        IList<SprKeyFrame> KeyFrames { get; }

        int TextureCount { get; }

        IList<ITextureFile> Textures { get; }
    }
}