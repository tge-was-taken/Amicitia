using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using AmicitiaLibrary.Graphics.DDS;
using AmicitiaLibrary.IO;
using AmicitiaLibrary.Utilities;

namespace AmicitiaLibrary.Graphics.SPD
{
    public class SpdFile
    {
        public const string MAGIC = "SPR0";

        public int Field04 { get; set; }

        public int Field0C { get; set; }

        public List<SpdTexture> Textures { get; }

        public List<SpdSprite> Sprites { get; }

        public SpdFile()
        {
            Field04 = 2;
            Field0C = 0;
            Textures = new List<SpdTexture>();
            Sprites = new List<SpdSprite>();
        }
        public SpdFile( Stream stream, bool leaveOpen ) : this()
        {
            using ( var reader = new EndianBinaryReader( stream, Encoding.Default, leaveOpen, Endianness.LittleEndian ) )
                Read( reader );
        }

        public SpdFile( string filepath ) : this( File.OpenRead( filepath ), false ) { }

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
            var magic = reader.ReadString( StringBinaryFormat.FixedLength, 4 );

            if ( magic != MAGIC )
                throw new InvalidDataException( "Invalid header magic value" );

            Field04 = reader.ReadInt32();
            var fileSize = reader.ReadInt32();
            Field0C = reader.ReadInt32();
            var field10 = reader.ReadInt32();

            if ( field10 != 0x20 )
                throw new InvalidDataException( "Field10 isn't 0x20" );

            var textureCount = reader.ReadInt16();
            var entryCount = reader.ReadInt16();
            reader.ReadOffset( () =>
            {
                for ( int i = 0; i < textureCount; ++i )
                {
                    var texture = new SpdTexture();
                    texture.Read( reader );
                    Textures.Add( texture );
                }
            } );
            reader.ReadOffset( () =>
            {
                for ( int i = 0; i < entryCount; ++i )
                {
                    var sprite = new SpdSprite();
                    sprite.Read( reader );
                    Sprites.Add( sprite );
                }
            } );
        }

        internal void Write( EndianBinaryWriter writer )
        {
            writer.Write( MAGIC, StringBinaryFormat.FixedLength, 4 );
            writer.Write( Field04 );
            writer.ScheduleFileSizeWrite();
            writer.Write( Field0C );
            writer.Write( 0x20 );
            writer.Write( (short)Textures.Count );
            writer.Write( (short)Sprites.Count );
            writer.ScheduleOffsetWrite( () => Textures.ForEach( x => x.Write( writer ) ) );
            writer.ScheduleOffsetWrite( () => Sprites.ForEach( x => x.Write( writer ) ) );
        }
    }

    public class SpdTexture : ITextureFile
    {
        // Cached decoded DDS bitmap
        private Bitmap mBitmap;

        /// <summary>
        /// Starts at 1.
        /// </summary>
        public int Id { get; set; }

        public int Field04 { get; set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int Field18 { get; set; }

        public int Field1C { get; set; }

        /// <summary>
        /// Description for debugging / editor.
        /// </summary>
        public string Description { get; set; }

        public byte[] Data { get; private set; }

        internal SpdTexture() { }

        public SpdTexture( byte[] ddsData )
        {
            var ddsHeader = new DDSHeader( new MemoryStream( ddsData ) );

            Id = 1;
            Field04 = 0;
            Width = ddsHeader.Width;
            Height = ddsHeader.Height;
            Field18 = 0;
            Field1C = 0;
            Description = "Created with <3 by Amicitia";
            Data = ddsData;
        }

        public SpdTexture( Bitmap bitmap ) 
        {
            Id = 1;
            Field04 = 0;
            Width = bitmap.Width;
            Height = bitmap.Height;
            Field18 = 0;
            Field1C = 0;
            Description = "Created with <3 by Amicitia";
            Data = DDSCodec.CompressImage( bitmap );
        }

        public SpdTexture( string filepath ) : this( File.OpenRead( filepath ), false ) { }

        public SpdTexture( Stream stream, bool leaveOpen )
        {
            using ( var reader = new EndianBinaryReader( stream, Encoding.Default, leaveOpen, Endianness.LittleEndian ) )
                Read( reader );
        }

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

        public Bitmap GetBitmap()
        {
            return mBitmap ?? ( mBitmap = DDSCodec.DecompressImage( Data ) );
        }

        internal void Read( EndianBinaryReader reader )
        {
            Id = reader.ReadInt32();
            Field04 = reader.ReadInt32();
            var offset = reader.ReadInt32();
            var size = reader.ReadInt32();
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            Field18 = reader.ReadInt32();
            Field1C = reader.ReadInt32();
            Description = EncodingCache.ShiftJIS.GetString( reader.ReadBytes( 16 ) ).Trim( '\0' );
            reader.ReadAtOffset( offset, () => Data = reader.ReadBytes( size ) );
        }

        internal void Write( EndianBinaryWriter writer )
        {
            writer.Write( Id );
            writer.Write( Field04 );
            writer.ScheduleOffsetWrite( () => writer.Write( Data ) );
            writer.Write( Data.Length );
            writer.Write( Width );
            writer.Write( Height );
            writer.Write( Field18 );
            writer.Write( Field1C );

            var bytes = EncodingCache.ShiftJIS.GetBytes( Description );
            for ( int i = 0; i < 16; ++i )
            {
                byte b = 0;
                if ( i < bytes.Length )
                    b = bytes[i];

                writer.Write( b );
            }
        }

        public Color[] GetPixels()
        {
            throw new NotImplementedException();
        }
    }

    public class SpdSprite
    {
        /// <summary>
        /// Starts at 1.
        /// </summary>
        public int Id { get; set; }
        public int AttachedToTextureId { get; set; }
        public int Field08 { get; set; }
        public int Field0C { get; set; }
        public int Field10 { get; set; }
        public int Field14 { get; set; }
        public int Field18 { get; set; }
        public int Field1C { get; set; }
        public int X1Coordinate { get; set; }
        public int Y1Coordinate { get; set; }
        public int X1Length { get; set; }
        public int Y1Length { get; set; }
        public int Field30 { get; set; }
        public int Field34 { get; set; }
        public int X2Length { get; set; }
        public int Y2Length { get; set; }
        public int Field40 { get; set; }
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
        public int Field6C { get; set; }
        public string Description { get; set; }

        public SpdSprite() { }

        public SpdSprite( string filepath ) : this( File.OpenRead( filepath ), false ) { }

        public SpdSprite( Stream stream, bool leaveOpen )
        {
            using ( var reader = new EndianBinaryReader( stream, Encoding.Default, leaveOpen, Endianness.LittleEndian ) )
                Read( reader );
        }

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
            Id = reader.ReadInt32();
            AttachedToTextureId = reader.ReadInt32();
            Field08 = reader.ReadInt32();
            Field0C = reader.ReadInt32();
            Field10 = reader.ReadInt32();
            Field14 = reader.ReadInt32();
            Field18 = reader.ReadInt32();
            Field1C = reader.ReadInt32();
            X1Coordinate = reader.ReadInt32();
            Y1Coordinate = reader.ReadInt32();
            X1Length = reader.ReadInt32();
            Y1Length = reader.ReadInt32();
            Field30 = reader.ReadInt32();
            Field34 = reader.ReadInt32();
            X2Length = reader.ReadInt32();
            Y2Length = reader.ReadInt32();
            Field40 = reader.ReadInt32();
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
            Field6C = reader.ReadInt32();
            Description = EncodingCache.ShiftJIS.GetString( reader.ReadBytes( 48 ) ).Trim( '\0' );
        }

        internal void Write( EndianBinaryWriter writer )
        {
            writer.Write( Id );
            writer.Write( AttachedToTextureId );
            writer.Write( Field08 );
            writer.Write( Field0C );
            writer.Write( Field10 );
            writer.Write( Field14 );
            writer.Write( Field18 );
            writer.Write( Field1C );
            writer.Write( X1Coordinate );
            writer.Write( Y1Coordinate );
            writer.Write( X1Length );
            writer.Write( Y1Length );
            writer.Write( Field30 );
            writer.Write( Field34 );
            writer.Write( X2Length );
            writer.Write( Y2Length );
            writer.Write( Field40 );
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
            writer.Write( Field6C );

            var bytes = EncodingCache.ShiftJIS.GetBytes( Description );
            for ( int i = 0; i < 48; ++i )
            {
                byte b = 0;
                if ( i < bytes.Length )
                    b = bytes[ i ];

                writer.Write( b );
            }
        }
    }
}
