using System.IO;
using System.Text;
using AmicitiaLibrary.IO;

namespace AmicitiaLibrary.Field
{
    public class CmrFile : BinaryBase
    {
        public const int MAGIC = 0x34567890;

        public short Field04 { get; set; }

        public short Field06 { get; set; }

        public float Fov     { get; set; }

        public int   Field0C { get; set; }

        public float Field10 { get; set; }

        public int   Field14 { get; set; }

        public float Field18 { get; set; }

        public int   Field1C { get; set; }

        public float Field20 { get; set; }

        public float Field24 { get; set; }

        public float Field28 { get; set; }

        public int   Field2C { get; set; }

        public float Field30 { get; set; }

        public float Field34 { get; set; }

        public float Field38 { get; set; }

        public int   Field3C { get; set; }

        public float PositionX { get; set; }

        public float PositionY { get; set; }

        public float PositionZ { get; set; }

        public float Field4C { get; set; }

        public short CameraType { get; set; }

        public short Field52 { get; set; }

        public int   Field54 { get; set; }

        public float Field58 { get; set; }

        public int   Field5C { get; set; }

        public float Field60 { get; set; }

        public float Field64 { get; set; }

        public int   Field68 { get; set; }

        public int   Field6C { get; set; }

        public CmrFile( string filepath ) : this(File.OpenRead(filepath), false) { }

        public CmrFile( Stream stream, bool leaveOpen = false )
        {
            using ( var reader = new BinaryReader( stream, Encoding.Default, leaveOpen ) )
                Read( reader );
        }

        internal void Read( BinaryReader reader )
        {
            int magic = reader.ReadInt32();
            if ( magic != MAGIC )
                throw new InvalidDataException();

            Field04 = reader.ReadInt16();
            Field06 = reader.ReadInt16();
            Fov = reader.ReadSingle();
            Field0C = reader.ReadInt32();
            Field10 = reader.ReadSingle();
            Field14 = reader.ReadInt32();
            Field18 = reader.ReadSingle();
            Field1C = reader.ReadInt32();
            Field20 = reader.ReadSingle();
            Field24 = reader.ReadSingle();
            Field28 = reader.ReadSingle();
            Field2C = reader.ReadInt32();
            Field30 = reader.ReadSingle();
            Field34 = reader.ReadSingle();
            Field38 = reader.ReadSingle();
            Field3C = reader.ReadInt32();
            PositionX = reader.ReadSingle();
            PositionY = reader.ReadSingle();
            PositionZ = reader.ReadSingle();
            Field4C = reader.ReadSingle();
            CameraType = reader.ReadInt16();
            Field52 = reader.ReadInt16();
            Field54 = reader.ReadInt32();
            Field58 = reader.ReadSingle();
            Field5C = reader.ReadInt32();
            Field60 = reader.ReadSingle();
            Field64 = reader.ReadSingle();
            Field68 = reader.ReadInt32();
            Field6C = reader.ReadInt32();
        }

        internal override void Write( BinaryWriter writer )
        {
            writer.Write( MAGIC );
            writer.Write( Field04 );
            writer.Write( Field06 );
            writer.Write( Fov );
            writer.Write( Field0C );
            writer.Write( Field10 );
            writer.Write( Field14 );
            writer.Write( Field18 );
            writer.Write( Field1C );
            writer.Write( Field20 );
            writer.Write( Field24 );
            writer.Write( Field28 );
            writer.Write( Field2C );
            writer.Write( Field30 );
            writer.Write( Field34 );
            writer.Write( Field38 );
            writer.Write( Field3C );
            writer.Write( PositionX );
            writer.Write( PositionY );
            writer.Write( PositionZ );
            writer.Write( Field4C );
            writer.Write( CameraType );
            writer.Write( Field52 );
            writer.Write( Field54 );
            writer.Write( Field58 );
            writer.Write( Field5C );
            writer.Write( Field60 );
            writer.Write( Field64 );
            writer.Write( Field68 );
            writer.Write( Field6C );
        }
    }
}
