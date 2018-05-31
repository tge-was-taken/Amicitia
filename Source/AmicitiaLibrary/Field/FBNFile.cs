using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AmicitiaLibrary.IO;

namespace AmicitiaLibrary.Field
{
    public class FbnFile : BinaryBase
    {
        public const int MAGIC = 0x12345678;

        public short VersionMajor { get; set; }

        public short VersionMinor { get; set; }

        public int ObjectCount => Objects.Count;

        public int ObjectEntrySize
        {
            get
            {
                switch ( VersionMajor )
                {
                    case 1:
                        return 0x70;  // Persona 3
                    case 2:
                        return 0x140; // Persona 4
                    default:
                        return 0;
                }
            }
        }

        public int Entry2Count => Entries2.Count;

        public int Entry2Size => 0x60;

        public List<BinaryFile> Objects { get; private set; }

        public List<BinaryFile> Entries2 { get; private set; }

        public FbnFile( string filepath ) : this(File.OpenRead(filepath), false) { }

        public FbnFile( Stream stream, bool leaveOpen = false )
        {
            using ( var reader = new BinaryReader( stream, Encoding.Default, leaveOpen ) )
                Read( reader );
        }

        public FbnFile(short versionMajor, short versionMinor )
        {
            VersionMajor = versionMajor;
            VersionMinor = versionMinor;
            Objects = new List<BinaryFile>();
            Entries2 = new List<BinaryFile>();
        }

        internal void Read( BinaryReader reader )
        {
            int magic = reader.ReadInt32();
            if ( magic != MAGIC )
                throw new InvalidDataException();

            VersionMajor = reader.ReadInt16();
            VersionMinor = reader.ReadInt16();
            int objectCount = reader.ReadInt32();

            int objectEntrySize = reader.ReadInt32();
            if ( objectEntrySize != ObjectEntrySize )
                throw new Exception();

            int entries2Count = reader.ReadInt32();
            int entries2EntrySize = reader.ReadInt32();
            if ( entries2EntrySize != Entry2Size )
                throw new Exception();

            Objects = new List<BinaryFile>( objectCount );
            for ( int i = 0; i < Objects.Capacity; i++ )
            {
                Objects.Add( new BinaryFile( reader.ReadBytes( objectEntrySize ) ) );
            }

            Entries2 = new List<BinaryFile>( entries2Count );
            for ( int i = 0; i < Entries2.Capacity; i++ )
            {
                Entries2.Add( new BinaryFile( reader.ReadBytes( entries2EntrySize ) ) );
            }
        }

        internal override void Write( BinaryWriter writer )
        {
            writer.Write( MAGIC );
            writer.Write( VersionMajor );
            writer.Write( VersionMinor );
            writer.Write( ObjectCount );
            writer.Write( ObjectEntrySize );
            writer.Write( Entry2Count );
            writer.Write( Entry2Size );

            foreach ( var obj in Objects )
            {
                var bytes = obj.GetBytes();
                for ( int i = 0; i < ObjectEntrySize; i++ )
                {
                    writer.Write( bytes[ i ] );
                }
            }

            foreach ( var entry in Entries2 )
            {
                var bytes = entry.GetBytes();
                for ( int i = 0; i < Entry2Size; i++ )
                {
                    writer.Write( bytes[i] );
                }
            }
        }
    }

    public class FbnObject : BinaryBase
    {
        // Version 2
        public short Field00 { get; set; }

        public short Field02 { get; set; }

        public short Field04 { get; set; }

        public short ModelId { get; set; }

        public short Field08 { get; set; }

        public short Field0B { get; set; }

        public Matrix4x4 Rotation { get; set; }

        public int Field4C { get; set; }

        public Vector3 Position { get; set; }

        public int Field5C { get; set; }

        internal override void Write( BinaryWriter writer )
        {
            throw new NotImplementedException();
        }
    }
}
