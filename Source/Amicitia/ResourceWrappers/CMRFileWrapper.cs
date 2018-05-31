using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmicitiaLibrary.Field;

namespace Amicitia.ResourceWrappers
{
    public class CmrFileWrapper : ResourceWrapper<CmrFile>
    {
        [Category( "Parameters" )]
        public int Field04
        {
            get => Resource.Field04;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public float Fov
        {
            get => Resource.Fov;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public int Field0C
        {
            get => Resource.Field0C;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public float Field10
        {
            get => Resource.Field10;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public int Field14
        {
            get => Resource.Field14;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public float Field18
        {
            get => Resource.Field18;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public int Field1C
        {
            get => Resource.Field1C;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public float Field20
        {
            get => Resource.Field20;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public float Field24
        {
            get => Resource.Field24;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public float Field28
        {
            get => Resource.Field28;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public int Field2C
        {
            get => Resource.Field2C;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public float Field30
        {
            get => Resource.Field30;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public float Field34
        {
            get => Resource.Field34;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public float Field38
        {
            get => Resource.Field38;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public int Field3C
        {
            get => Resource.Field3C;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public float PositionX
        {
            get => Resource.PositionX;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public float PositionY
        {
            get => Resource.PositionY;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public float PositionZ
        {
            get => Resource.PositionZ;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public float Field4C
        {
            get => Resource.Field4C;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public short CameraType
        {
            get => Resource.CameraType;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public short Field52
        {
            get => Resource.Field52;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public int Field54
        {
            get => Resource.Field54;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public float Field58
        {
            get => Resource.Field58;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public int Field5C
        {
            get => Resource.Field5C;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public float Field60
        {
            get => Resource.Field60;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public float Field64
        {
            get => Resource.Field64;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public int Field68
        {
            get => Resource.Field68;
            set => SetProperty( Resource, value );
        }

        [Category( "Parameters" )]
        public int Field6C
        {
            get => Resource.Field6C;
            set => SetProperty( Resource, value );
        }

        public CmrFileWrapper( string text, CmrFile resource ) : base( text, resource ) { }

        protected override void Initialize( )
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Move |
                                       CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction( SupportedFileType.CmrFile, ( res, path ) => res.Save( path ) );
            RegisterFileReplaceAction( SupportedFileType.CmrFile, ( res, path ) => new CmrFile( path ) );
        }

        protected override void PopulateView( )
        {
        }
    }
}
