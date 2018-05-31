using System;
using System.IO;
using System.Text;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    public class RwUserData
    {
        public string Name { get; set; }

        public RwUserDataFormat Format { get; private set; }

        public int Unknown { get; set; }

        private object mValue;
        public object Value
        {
            get => mValue;
            set
            {
                mValue = value;
                switch ( mValue )
                {
                    case null:
                        Format = RwUserDataFormat.None;
                        break;

                    case int _:
                        Format = RwUserDataFormat.Int32;
                        break;

                    case float _:
                        Format = RwUserDataFormat.Float;
                        break;

                    case string _:
                        Format = RwUserDataFormat.String;
                        break;

                    default:
                        throw new InvalidOperationException( "Incompatible value type" );
                }
            }
        }

        public int IntValue
        {
            get
            {
                switch ( Format )
                {
                    case RwUserDataFormat.Int32: return ( int )Value;
                    case RwUserDataFormat.Float: return ( int )FloatValue;
                    default: return 0;
                }
            }
        }

        public float FloatValue
        {
            get
            {
                switch ( Format )
                {
                    case RwUserDataFormat.Int32:
                        return ( float )IntValue;
                    case RwUserDataFormat.Float:
                        return ( float )Value;
                    default:
                        return 0;
                }
            }
        }

        public string StringValue
        {
            get
            {
                switch ( Format )
                {
                    case RwUserDataFormat.String: return ( string )Value;
                    default:
                        return null;
                }
            }
        }

        public RwUserData()
        {

        }

        public RwUserData(string name, object value)
        {
            Name = name;
            Value = value;
            Unknown = 1;
        }

        public RwUserData( Stream stream, bool leaveOpen )
        {
            using ( var reader = new BinaryReader( stream, Encoding.Default, leaveOpen ) )
                Read( reader );
        }

        internal void Read( BinaryReader reader )
        {
            var nameLength = reader.ReadInt32();
            Name = string.Empty;
            if ( nameLength > 0 )
            {
                Name = Encoding.ASCII.GetString( reader.ReadBytes( nameLength - 1) );
                reader.BaseStream.Position += 1;
            }

            Format = ( RwUserDataFormat ) reader.ReadInt32();
            Unknown = reader.ReadInt32();

            switch ( Format )
            {
                case RwUserDataFormat.Int32:
                    Value = reader.ReadInt32();
                    break;

                case RwUserDataFormat.Float:
                    Value = reader.ReadSingle();
                    break;

                case RwUserDataFormat.String:
                    var valueLength = reader.ReadInt32();
                    Value = string.Empty;
                    if ( valueLength > 0 )
                    {
                        Value = Encoding.ASCII.GetString( reader.ReadBytes( valueLength - 1 ) );
                        reader.BaseStream.Position += 1;
                    }              
                    break;
            }
        }

        internal void Write( BinaryWriter writer )
        {
            writer.Write( Name.Length + 1 );
            writer.Write( Encoding.ASCII.GetBytes( Name ) );
            writer.Write( ( byte ) 0 );
            writer.Write( ( int ) Format );
            writer.Write( Unknown );

            switch ( Format )
            {
                case RwUserDataFormat.Int32:
                    writer.Write( IntValue );
                    break;

                case RwUserDataFormat.Float:
                    writer.Write( FloatValue );
                    break;

                case RwUserDataFormat.String:
                    writer.Write( StringValue.Length + 1 );
                    writer.Write( Encoding.ASCII.GetBytes( StringValue ) );
                    writer.Write( ( byte ) 0 );
                    break;
            }
        }
    }

    public enum RwUserDataFormat
    {
        None,
        Int32,
        Float,
        String
    }
}