using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using AmicitiaLibrary.Graphics.TGA;
using AmicitiaLibrary.IO;

namespace AmicitiaLibrary.Graphics.SPR6
{
    public class Spr6File
    {
        private const int MAGIC = 0x36525053;

        public short Field04 { get; set; }
        public short Field08 { get; set; }
        public short Field0C { get; set; }
        public int Field1C { get; set; }

        public List<Spr6Texture> Textures { get; } = new List<Spr6Texture>();

        public List<Spr6Panel> Panels { get; } = new List<Spr6Panel>();

        public List<Spr6Sprite> Sprites { get; } = new List<Spr6Sprite>();

        public Spr6File()
        {
            Field04 = 0;
            Field08 = 1;
            Field0C = 24;
            Field1C = 0;
            Textures = new List<Spr6Texture>();
            Panels = new List<Spr6Panel>();
            Sprites = new List<Spr6Sprite>();
        }

        public Spr6File( string filepath ) : this( File.OpenRead( filepath ), false ) { }

        public Spr6File( Stream stream, bool leaveOpen ) : this()
        {
            using ( var reader = new EndianBinaryReader( stream, Encoding.Default, leaveOpen, Endianness.LittleEndian ) )
                Read( reader );
        }

        internal Spr6File( EndianBinaryReader reader ) => Read( reader );

        public void Save( Stream stream, bool leaveOpen )
        {
            using ( var writer = new EndianBinaryWriter( stream, Encoding.Default, leaveOpen, Endianness.LittleEndian ) )
            {
                Write( writer );
                writer.PerformScheduledWrites();
            }
        }

        public void Save( string filepath ) => Save( File.Create( filepath ), false );

        public MemoryStream Save()
        {
            var stream = new MemoryStream();
            Save( stream, true );
            return stream;
        }
        internal void Read( EndianBinaryReader reader )
        {
            var magic = reader.ReadInt32();
            if ( magic != MAGIC )
                throw new InvalidDataException( "Header magic value doesn't match" );

            Field04 = reader.ReadInt16();
            Field08 = reader.ReadInt16();
            var fileSize = reader.ReadInt32();
            Field0C = reader.ReadInt16();
            var textureCount = reader.ReadInt16();
            var spriteCount = reader.ReadInt16();
            var panelCount = reader.ReadInt16();
            reader.ReadOffset( textureCount, () => Textures.Add( new Spr6Texture( reader ) ) );
            reader.ReadOffset( () =>
            {
                for ( int i = 0; i < panelCount; ++i )
                    Panels.Add( new Spr6Panel( reader ) );

                for ( int i = 0; i < spriteCount; i++ )
                {
                    var field00 = reader.ReadInt32();
                    Debug.Assert( field00 == 0, "Sprite entry Field00 != 0" );
                    reader.ReadOffset( () => { Sprites.Add( new Spr6Sprite( reader ) ); } );
                }
            } );
            Field1C = reader.ReadInt32();
        }
        internal void Write( EndianBinaryWriter writer )
        {
            writer.Write( MAGIC );
            writer.Write( Field04 );
            writer.Write( Field08 );
            writer.ScheduleFileSizeWrite();
            writer.Write( Field0C );
            writer.Write( (short)Textures.Count );
            writer.Write( (short)Sprites.Count );
            writer.Write( (short)Panels.Count );
            writer.ScheduleOffsetWrite( () => Textures.ForEach( x => x.Write( writer ) ) );
            writer.ScheduleOffsetWrite( () =>
            {
                Panels.ForEach(x => x.Write(writer));

                foreach ( var sprite in Sprites )
                {
                    writer.Write( 0 );
                    writer.ScheduleOffsetWrite( () => sprite.Write( writer ) );
                }
            } );
            writer.Write( Field1C );
        }
    }

    public class Spr6Texture : ITextureFile
    {
        private TgaFile mTga;
        private Bitmap mBitmap;

        public string Description{ get; set; }
        public int    Field00    { get; set; }
        public int    Field04    { get; set; }
        public short  Field08    { get; set; }
        public short  Field0A    { get; set; }
        public int    Field14    { get; set; }
        public byte[] Data { get; set; }

        public Spr6Texture()
        {
            Description = "Created with <3 by Amicitia";
            Field00     = 1;
            Field04     = 0x3AEEDC;
            Field08     = 3;
            Field0A     = 0x1278;
            Field14     = 0;
        }

        public Spr6Texture( byte[] textureData ) : this()
        {
            Data = textureData;
        }

        public Spr6Texture( Bitmap bitmap ) : this()
        {
            Data = new TgaFile( bitmap ).GetBytes();
        }

        public Spr6Texture( string filepath ) : this( File.OpenRead( filepath ), false ) { }

        public Spr6Texture( Stream stream, bool leaveOpen )
        {
            using ( var reader = new EndianBinaryReader( stream, Encoding.Default, leaveOpen, Endianness.LittleEndian ) )
                Read( reader );
        }

        internal Spr6Texture( EndianBinaryReader reader ) => Read( reader );

        public void Save( Stream stream, bool leaveOpen )
        {
            using ( var writer = new EndianBinaryWriter( stream, Encoding.Default, leaveOpen, Endianness.LittleEndian ) )
            {
                Write( writer );
                writer.PerformScheduledWrites();
            }
        }

        public void Save( string filepath ) => Save( File.Create( filepath ), false );

        public MemoryStream Save()
        {
            var stream = new MemoryStream();
            Save( stream, true );
            return stream;
        }
        internal void Read( EndianBinaryReader reader )
        {
            Description       = reader.ReadString( StringBinaryFormat.FixedLength, 20 );
            Field00    = reader.ReadInt32();
            Field04    = reader.ReadInt32();
            Field08    = reader.ReadInt16();
            Field0A    = reader.ReadInt16();
            var size   = reader.ReadInt32();
            Field14    = reader.ReadInt32();
            reader.ReadOffset( () => Data = reader.ReadBytes( size ) );
        }
        internal void Write( EndianBinaryWriter writer )
        {
            writer.Write( Description, StringBinaryFormat.FixedLength, 20 );
            writer.Write( Field00 );
            writer.Write( Field04 );
            writer.Write( Field08 );
            writer.Write( Field0A );
            writer.Write( Data.Length );
            writer.Write( Field14 );
            writer.ScheduleOffsetWrite( 1, () => writer.Write( Data ) );
        }

        public TgaFile GetTgaFile()
        {
            return mTga ?? ( mTga = new TgaFile( new MemoryStream( Data ) ) );
        }

        public Bitmap GetBitmap()
        {
            return mBitmap ?? ( mBitmap = GetTgaFile().GetBitmap() );
        }

        public Color[] GetPixels()
        {
            return GetTgaFile().GetPixels();
        }
    }

    public class Spr6Panel
    {
        public string Description { get; set; }
        public short  Field08 { get; set; }
        public short  Field0A { get; set; }
        public short  Field0C { get; set; }
        public short  Field0E { get; set; }
        public int    Field10 { get; set; }
        public int    Field14 { get; set; }


        public Spr6Panel()
        {
            Field08 = 3;
            Field0A = 0x1278;
            Field0C = 0x412;
            Field0E = 1;
            Field10 = 0;
            Field14 = 46;
        }

        public Spr6Panel( string filepath ) : this( File.OpenRead( filepath ), false ) { }

        public Spr6Panel( Stream stream, bool leaveOpen )
        {
            using ( var reader = new EndianBinaryReader( stream, Encoding.Default, leaveOpen, Endianness.LittleEndian ) )
                Read( reader );
        }

        internal Spr6Panel( EndianBinaryReader reader ) => Read( reader );

        public void Save( Stream stream, bool leaveOpen )
        {
            using ( var writer = new EndianBinaryWriter( stream, Encoding.Default, leaveOpen, Endianness.LittleEndian ) )
                Write( writer );
        }

        public void Save( string filepath ) => Save( File.Create( filepath ), false );

        public MemoryStream Save()
        {
            var stream = new MemoryStream();
            Save( stream, true );
            return stream;
        }
        internal void Read( EndianBinaryReader reader )
        {
            Description    = reader.ReadString( StringBinaryFormat.FixedLength, 20 );
            Field08 = reader.ReadInt16();
            Field0A = reader.ReadInt16();
            Field0C = reader.ReadInt16();
            Field0E = reader.ReadInt16();
            Field10 = reader.ReadInt32();
            Field14 = reader.ReadInt32();
        }
        internal void Write( EndianBinaryWriter writer )
        {
            writer.Write( Description, StringBinaryFormat.FixedLength, 20 );
            writer.Write( Field08 );
            writer.Write( Field0A );
            writer.Write( Field0C );
            writer.Write( Field0E );
            writer.Write( Field10 );
            writer.Write( Field14 );
        }
    }

    public class Spr6Sprite
    {
        public short Field00 { get; set; }
        public short TextureId { get; set; }
        public string Description { get; set; }
        public int Field30 { get; set; }
        public int Field34 { get; set; }
        public int Field38 { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Field44 { get; set; }
        public int Field48 { get; set; }
        public int Field4C { get; set; }
        public int Field50 { get; set; }
        public int Field54 { get; set; }
        public int Field58 { get; set; }
        public int Field5C { get; set; }
        public int Field60 { get; set; }
        public int Field64 { get; set; }
        public int Field68 { get; set; }

        public Spr6Sprite()
        {
            Field00 = 0;
            TextureId = 1;
            Description = "Created with <3 by Amicitia";
            Field30 = 0;
            Field34 = 1;
            Field38 = 1;
            Width = 85;
            Height = 76;
            Field44 = -1;
            Field44 = -1;
            Field48 = -1;
            Field4C = -1;
            Field50 = -1;
            Field54 = 0;
            Field58 = 0;
            Field5C = 0;
            Field60 = 0;
            Field64 = 0;
            Field68 = 0;
        }

        public Spr6Sprite( string filepath ) : this( File.OpenRead( filepath ), false ) { }

        public Spr6Sprite( Stream stream, bool leaveOpen )
        {
            using ( var reader = new EndianBinaryReader( stream, Encoding.Default, leaveOpen, Endianness.LittleEndian ) )
                Read( reader );
        }

        internal Spr6Sprite( EndianBinaryReader reader ) => Read( reader );

        public void Save( Stream stream, bool leaveOpen )
        {
            using ( var writer = new EndianBinaryWriter( stream, Encoding.Default, leaveOpen, Endianness.LittleEndian ) )
                Write( writer );
        }

        public void Save( string filepath ) => Save( File.Create( filepath ), false );

        public MemoryStream Save()
        {
            var stream = new MemoryStream();
            Save( stream, true );
            return stream;
        }

        internal void Read( EndianBinaryReader reader )
        {
            Field00 = reader.ReadInt16();
            TextureId = reader.ReadInt16();
            Description = reader.ReadString( StringBinaryFormat.FixedLength, 32 );
            Field30 = reader.ReadInt32();
            Field34 = reader.ReadInt32();
            Field38 = reader.ReadInt32();
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            Field44 = reader.ReadInt32();
            Field48 = reader.ReadInt32();
            Field4C = reader.ReadInt32();
            Field50 = reader.ReadInt32();
            Field54 = reader.ReadInt32();
            Field58 = reader.ReadInt32();
            Field5C = reader.ReadInt32();
            Field60 = reader.ReadInt32();
            Field64 = reader.ReadInt32();
            Field68 = reader.ReadInt32();
        }

        internal void Write( EndianBinaryWriter writer )
        {
            writer.Write( Field00 );
            writer.Write( TextureId );
            writer.Write( Description, StringBinaryFormat.FixedLength, 32 );
            writer.Write( Field30 );
            writer.Write( Field34 );
            writer.Write( Field38 );
            writer.Write( Width );
            writer.Write( Height );
            writer.Write( Field44 );
            writer.Write( Field48 );
            writer.Write( Field4C );
            writer.Write( Field50 );
            writer.Write( Field54 );
            writer.Write( Field58 );
            writer.Write( Field5C );
            writer.Write( Field60 );
            writer.Write( Field64 );
            writer.Write( Field68 );
        }
    }
}
