using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using AmicitiaLibrary.Graphics.SPD;

namespace Amicitia.ResourceWrappers
{
    public class SpdFileWrapper : ResourceWrapper<SpdFile>
    {
        public int Field04
        {
            get => Resource.Field04;
            set => SetProperty( Resource, value );
        }

        public int Field0C
        {
            get => Resource.Field0C;
            set => SetProperty( Resource, value );
        }

        public int TextureCount => Textures.Count;

        public int SpriteCount => Sprites.Count;

        [Browsable( false )]
        public GenericListWrapper<SpdTexture> Textures { get; private set; }

        [Browsable( false )]
        public GenericListWrapper<SpdSprite> Sprites { get; private set; }

        public SpdFileWrapper( string text, SpdFile resource ) : base( text, resource )
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Add |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction( SupportedFileType.SpdFile, ( res,  path ) => res.Save( path ) );
            RegisterFileReplaceAction( SupportedFileType.SpdFile, ( res, path ) => new SpdFile( path ) );
            RegisterFileAddAction( SupportedFileType.Resource, DefaultFileAddAction );
            RegisterRebuildAction( ( wrap ) =>
            {
                var file = new SpdFile
                {
                    Field04 = Field04,
                    Field0C = Field0C
                };

                file.Textures.AddRange( Textures.Resource );
                file.Sprites.AddRange( Sprites.Resource );

                return file;
            } );
        }

        protected override void PopulateView()
        {
            Textures = new GenericListWrapper<SpdTexture>( "Textures", Resource.Textures, ( r, i ) => $"Texture {i:D2} [{r.Description.Trim('\n')}]" );
            Sprites  = new GenericListWrapper<SpdSprite>( "Sprites", Resource.Sprites, ( r,    i ) => $"Sprite {i:D2} [{r.Description.Trim('\n')}]" );

            Nodes.Add( Textures );
            Nodes.Add( Sprites );
        }
    }

    public class SpdTextureWrapper : ResourceWrapper<SpdTexture>
    {
        public int Id
        {
            get => Resource.Id;
            set => SetProperty( Resource, value );
        }

        public int Field04
        {
            get => Resource.Field04;
            set => SetProperty( Resource, value );
        }

        public int Width => Resource.Width;

        public int Height => Resource.Height;

        public int Field18
        {
            get => Resource.Field18;
            set => SetProperty( Resource, value );
        }

        public int Field1C
        {
            get => Resource.Field1C;
            set => SetProperty( Resource, value );
        }

        public string Description
        {
            get => Resource.Description;
            set => SetProperty( Resource, value );
        }

        public SpdTextureWrapper( string text, SpdTexture resource ) : base( text, resource )
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Move |
                                       CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction( SupportedFileType.SpdTexture, ( res,  path ) => res.Save( path ) );
            RegisterFileExportAction( SupportedFileType.Bitmap,  ( res,  path ) => res.GetBitmap().Save( path, ImageFormat.Png ) );
            RegisterFileExportAction( SupportedFileType.DDS, ( res, path ) => File.WriteAllBytes( path, res.Data ) );
            RegisterFileReplaceAction( SupportedFileType.SpdTexture, ( res, path ) => new SpdTexture( path ) );
            RegisterFileReplaceAction( SupportedFileType.DDS, ( res, path ) => new SpdTexture( File.ReadAllBytes( path ) )
            {
                Id          = res.Id,
                Field04     = res.Field04,
                Field18     = res.Field18,
                Field1C     = res.Field1C,
                Description = res.Description
            } );
            RegisterFileReplaceAction( SupportedFileType.Bitmap, ( res, path ) => new SpdTexture( new Bitmap( path ) )
            {
                Id          = res.Id,
                Field04     = res.Field04,
                Field18     = res.Field18,
                Field1C     = res.Field1C,
                Description = res.Description
            } );
        }

        protected override void PopulateView()
        {
        }
    }

    public class SpdSpriteWrapper : ResourceWrapper<SpdSprite>
    {
        public int Id
        {
            get => Resource.Id;
            set => SetProperty( Resource, value );
        }

        public int AttachedToTextureId { get => Resource.AttachedToTextureId; set => SetProperty( Resource, value ); }
        public int Field08 { get => Resource.Field08; set => SetProperty( Resource, value ); }
        public int Field0C { get => Resource.Field0C; set => SetProperty( Resource, value ); }
        public int Field10 { get => Resource.Field10; set => SetProperty( Resource, value ); }
        public int Field14 { get => Resource.Field14; set => SetProperty( Resource, value ); }
        public int Field18 { get => Resource.Field18; set => SetProperty( Resource, value ); }
        public int Field1C { get => Resource.Field1C; set => SetProperty( Resource, value ); }
        public int X1Coordinate { get => Resource.X1Coordinate; set => SetProperty( Resource, value ); }
        public int Y1Coordinate { get => Resource.Y1Coordinate; set => SetProperty( Resource, value ); }
        public int X1Length { get => Resource.X1Length; set => SetProperty( Resource, value ); }
        public int Y1Length { get => Resource.Y1Length; set => SetProperty( Resource, value ); }
        public int Field30 { get => Resource.Field30; set => SetProperty( Resource, value ); }
        public int Field34 { get => Resource.Field34; set => SetProperty( Resource, value ); }
        public int X2Length { get => Resource.X2Length; set => SetProperty( Resource, value ); }
        public int Y2Length { get => Resource.Y2Length; set => SetProperty( Resource, value ); }
        public int Field40 { get => Resource.Field40; set => SetProperty( Resource, value ); }
        public int Field44 { get => Resource.Field44; set => SetProperty( Resource, value ); }
        public int Field48 { get => Resource.Field48; set => SetProperty( Resource, value ); }
        public int Field4C { get => Resource.Field4C; set => SetProperty( Resource, value ); }
        public int Field50 { get => Resource.Field50; set => SetProperty( Resource, value ); }
        public int Field54 { get => Resource.Field54; set => SetProperty( Resource, value ); }
        public int Field58 { get => Resource.Field58; set => SetProperty( Resource, value ); }
        public int Field5C { get => Resource.Field5C; set => SetProperty( Resource, value ); }
        public int Field60 { get => Resource.Field60; set => SetProperty( Resource, value ); }
        public int Field64 { get => Resource.Field64; set => SetProperty( Resource, value ); }
        public int Field68 { get => Resource.Field68; set => SetProperty( Resource, value ); }
        public int Field6C { get => Resource.Field6C; set => SetProperty( Resource, value ); }

        public string Description
        {
            get => Resource.Description;
            set => SetProperty( Resource, value );
        }

        public SpdSpriteWrapper( string text, SpdSprite resource ) : base( text, resource )
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Move |
                                       CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction( SupportedFileType.SpdSprite, ( res,  path ) => res.Save( path ) );
            RegisterFileReplaceAction( SupportedFileType.SpdSprite, ( res, path ) => new SpdSprite( path ) );
        }

        protected override void PopulateView()
        {
        }
    }
}