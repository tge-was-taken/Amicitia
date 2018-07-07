using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using AmicitiaLibrary.Graphics.SPR6;

namespace Amicitia.ResourceWrappers
{
    public class Spr6FileWrapper : ResourceWrapper<Spr6File>
    {
        public short Field04
        {
            get => Resource.Field04;
            set => SetProperty( Resource, value );
        }

        public short Field08
        {
            get => Resource.Field08;
            set => SetProperty( Resource, value );
        }

        public short Field0C
        {
            get => Resource.Field0C;
            set => SetProperty( Resource, value );
        }

        public int Field1C
        {
            get => Resource.Field1C;
            set => SetProperty( Resource, value );
        }

        public int TextureCount => Textures.Count;

        public int PanelCount => Panels.Count;

        public int SpriteCount => Sprites.Count;

        [Browsable( false )]
        public GenericListWrapper<Spr6Texture> Textures { get; private set; }

        [Browsable( false )]
        public GenericListWrapper<Spr6Panel> Panels { get; private set; }

        [Browsable( false )]
        public GenericListWrapper<Spr6Sprite> Sprites { get; private set; }

        public Spr6FileWrapper( string text, Spr6File resource ) : base( text, resource )
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Add |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction( SupportedFileType.Spr6File, ( res, path ) => res.Save( path ) );
            RegisterFileReplaceAction( SupportedFileType.Spr6File, ( res, path ) => new Spr6File( path ) );
            RegisterFileAddAction( SupportedFileType.Resource, DefaultFileAddAction );
            RegisterRebuildAction( ( wrap ) =>
            {
                var file = new Spr6File
                {
                    Field04 = Field04,
                    Field08 = Field08,
                    Field0C = Field0C,
                    Field1C = Field1C
                };

                file.Textures.AddRange( Textures.Resource );
                file.Panels.AddRange( Panels.Resource );
                file.Sprites.AddRange( Sprites.Resource );

                return file;
            } );
        }

        protected override void PopulateView()
        {
            Textures = new GenericListWrapper<Spr6Texture>( "Textures", Resource.Textures, ( r, i ) => $"Texture {i:D2} [{r.Description.Trim( '\n' )}]" );
            Panels = new GenericListWrapper<Spr6Panel>( "Panels", Resource.Panels, ( r, i ) => $"Panel {i:D2} [{r.Description.Trim( '\n' )}]" );
            Sprites = new GenericListWrapper<Spr6Sprite>( "Sprites", Resource.Sprites, ( r, i ) => $"Sprite {i:D2} [{r.Description.Trim( '\n' )}]" );

            Nodes.Add( Textures );
            Nodes.Add( Panels );
            Nodes.Add( Sprites );
        }
    }

    public class Spr6TextureWrapper : ResourceWrapper<Spr6Texture>
    {
        public string Description
        {
            get => Resource.Description;
            set => SetProperty( Resource, value );
        }

        public int Field00
        {
            get => Resource.Field00;
            set => SetProperty( Resource, value );
        }

        public int Field04
        {
            get => Resource.Field04;
            set => SetProperty( Resource, value );
        }

        public short Field08
        {
            get => Resource.Field08;
            set => SetProperty( Resource, value );
        }

        public short Field0A
        {
            get => Resource.Field0A;
            set => SetProperty( Resource, value );
        }

        public int Field14
        {
            get => Resource.Field14;
            set => SetProperty( Resource, value );
        }

        public Spr6TextureWrapper( string text, Spr6Texture resource ) : base( text, resource )
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Move |
                                       CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction( SupportedFileType.Spr6Texture, ( res, path ) => res.Save( path ) );
            RegisterFileExportAction( SupportedFileType.Bitmap, ( res, path ) => res.GetBitmap().Save( path, ImageFormat.Png ) );
            RegisterFileExportAction( SupportedFileType.TgaFile, ( res, path ) => File.WriteAllBytes( path, res.Data ) );
            RegisterFileReplaceAction( SupportedFileType.Spr6Texture, ( res, path ) => new Spr6Texture( path ) );
            RegisterFileReplaceAction( SupportedFileType.TgaFile, ( res, path ) => new Spr6Texture( File.ReadAllBytes( path ) )
            {
                Description = res.Description,
                Field00 = res.Field00,
                Field04 = res.Field04,
                Field08 = res.Field08,
                Field0A = res.Field0A,
                Field14 = res.Field14,
            } );
            RegisterFileReplaceAction( SupportedFileType.Bitmap, ( res, path ) => new Spr6Texture( new Bitmap( path ) )
            {
                Description = res.Description,
                Field00 = res.Field00,
                Field04 = res.Field04,
                Field08 = res.Field08,
                Field0A = res.Field0A,
                Field14 = res.Field14,
            } );
        }

        protected override void PopulateView()
        {
        }
    }

    public class Spr6PanelWrapper : ResourceWrapper<Spr6Panel>
    {
        public string Description { get => Resource.Description; set => SetProperty( Resource, value ); }
        public short Field08 { get => Resource.Field08; set => SetProperty( Resource, value ); }
        public short Field0A { get => Resource.Field0A; set => SetProperty( Resource, value ); }
        public short Field0C { get => Resource.Field0C; set => SetProperty( Resource, value ); }
        public short Field0E { get => Resource.Field0E; set => SetProperty( Resource, value ); }
        public int Field10 { get => Resource.Field10; set => SetProperty( Resource, value ); }
        public int Field14 { get => Resource.Field14; set => SetProperty( Resource, value ); }

        public Spr6PanelWrapper( string text, Spr6Panel resource ) : base( text, resource )
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Move |
                                       CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction( SupportedFileType.Spr6Panel, ( res, path ) => res.Save( path ) );
            RegisterFileReplaceAction( SupportedFileType.Spr6Panel, ( res, path ) => new Spr6Panel( path ) );
        }

        protected override void PopulateView()
        {
        }
    }

    public class Spr6SpriteWrapper : ResourceWrapper<Spr6Sprite>
    {
        public short Field00 { get => Resource.Field00; set => SetProperty( Resource, value ); }
        public short TextureId { get => Resource.TextureId; set => SetProperty( Resource, value ); }
        public string Description { get => Resource.Description; set => SetProperty( Resource, value ); }
        public int Field30 { get => Resource.Field30; set => SetProperty( Resource, value ); }
        public int Field34 { get => Resource.Field34; set => SetProperty( Resource, value ); }
        public int Field38 { get => Resource.Field38; set => SetProperty( Resource, value ); }
        public int Field3C { get => Resource.Field3C; set => SetProperty( Resource, value ); }
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

        public Spr6SpriteWrapper( string text, Spr6Sprite resource ) : base( text, resource )
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Move |
                                       CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction( SupportedFileType.Spr6Sprite, ( res, path ) => res.Save( path ) );
            RegisterFileReplaceAction( SupportedFileType.Spr6Sprite, ( res, path ) => new Spr6Sprite( path ) );
        }

        protected override void PopulateView()
        {
        }
    }
}
