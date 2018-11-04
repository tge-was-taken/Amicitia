using System.ComponentModel;
using AmicitiaLibrary.Graphics;
using AmicitiaLibrary.Graphics.SPR;
using AmicitiaLibrary.Graphics.TGA;
using AmicitiaLibrary.Graphics.TMX;
using AmicitiaLibrary.IO;
using System.Drawing;

namespace Amicitia.ResourceWrappers
{
    public abstract class SprFileWrapper<TFile, TTexture> : ResourceWrapper<TFile>
        where TFile : BinaryBase, ISprFile 
        where TTexture : ITextureFile
    {
        [Browsable(false)]
        public GenericListWrapper<SprSprite> SpriteListWrapper { get; }

        [Browsable(false)]
        public GenericListWrapper<TTexture> TextureListWrapper { get; }

        public SprFileWrapper(string text, TFile resource) : base(text, resource)
        {
            SpriteListWrapper = new GenericListWrapper<SprSprite>("Sprites", resource.Sprites, (e, i) => !string.IsNullOrEmpty( e.Comment ) ? $"Sprite [{e.Comment}]" : $"Sprite {i:00}" );
            TextureListWrapper = GetTextureListWrapper();
            PopulateView();
        }

        protected override void PopulateView()
        {
            if ( SpriteListWrapper != null )
                Nodes.Add(SpriteListWrapper);

            if ( TextureListWrapper != null )
                Nodes.Add(TextureListWrapper);
        }

        protected abstract GenericListWrapper<TTexture> GetTextureListWrapper();
    }

    public class SprFileWrapper : SprFileWrapper<SprFile, TmxFile>
    {
        public SprFileWrapper(string text, SprFile resource) : base(text, resource)
        {
            RegisterRebuildAction((wrap) => new SprFile(TextureListWrapper.Resource, SpriteListWrapper.Resource));
        }

        protected override GenericListWrapper<TmxFile> GetTextureListWrapper()
        {
            return new GenericListWrapper<TmxFile>("Textures", Resource.Textures,
                (tex, i) => !string.IsNullOrEmpty(tex.UserComment) ? tex.UserComment : $"Texture {i:00}");
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Add |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.SprFile, (res, path) => res.Save(path));
            RegisterFileReplaceAction(SupportedFileType.SprFile, (res, path) => SprFile.Load(path));
            RegisterFileAddAction(SupportedFileType.Resource, DefaultFileAddAction);
        }
    }

    public class Spr4FileWrapper : SprFileWrapper<Spr4File, TgaFile>
    {
        public Spr4FileWrapper(string text, Spr4File resource) : base(text, resource)
        {
            RegisterRebuildAction((wrap) => new Spr4File(TextureListWrapper.Resource, SpriteListWrapper.Resource));
        }

        protected override GenericListWrapper<TgaFile> GetTextureListWrapper()
        {
            return new GenericListWrapper<TgaFile>("Textures", Resource.Textures, (tex, i) => $"Texture {i:00}");
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Add |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.SprFile, (res, path) => res.Save(path));
            RegisterFileReplaceAction(SupportedFileType.SprFile, (res, path) => Spr4File.Load(path));
            RegisterFileAddAction(SupportedFileType.Resource, DefaultFileAddAction);

            Nodes.Add(SpriteListWrapper);
            Nodes.Add(TextureListWrapper);
        }
    }

    public class SprSpriteWrapper : ResourceWrapper<SprSprite>
    {
        public string Comment
        {
            get => Resource.Comment;
            set => Resource.Comment = value;
        }

        public int TextureIndex
        {
            get => Resource.TextureIndex;
            set => Resource.TextureIndex = value;
        }

        public int OffsetX
        {
            get => Resource.OffsetX;
            set => Resource.OffsetX = value;
        }

        public int OffsetY
        {
            get => Resource.OffsetY;
            set => Resource.OffsetY = value;
        }

        public Rectangle Coordinates
        {
            get => Resource.Coordinates;
            set => Resource.Coordinates = value;
        }

        public SprSpriteWrapper(string text, SprSprite resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.SprSprite, (res, path) => res.Save(path));
            RegisterFileReplaceAction(SupportedFileType.SprSprite, (res, path) => new SprSprite(path));
        }

        protected override void PopulateView()
        {
        }
    }
}
