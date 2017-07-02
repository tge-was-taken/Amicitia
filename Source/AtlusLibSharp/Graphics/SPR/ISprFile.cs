using System.Collections.Generic;
using AtlusLibSharp.Graphics.TMX;

namespace AtlusLibSharp.Graphics.SPR
{
    public interface ISprFile
    {
        int KeyFrameCount { get; }

        IList<SprKeyFrame> KeyFrames { get; }

        int TextureCount { get; }

        IList<ITextureFile> Textures { get; }
    }
}