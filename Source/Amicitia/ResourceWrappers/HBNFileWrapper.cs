using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmicitiaLibrary.Field;
using AmicitiaLibrary.IO;

namespace Amicitia.ResourceWrappers
{
    public class HbnFileWrapper : ResourceWrapper<HbnFile>
    {
        [Browsable( false )]
        public GenericListWrapper<BinaryFile> Entries1Wrapper { get; private set; }

        [Browsable( false )]
        public GenericListWrapper<BinaryFile> Entries2Wrapper { get; private set; }

        [Browsable( false )]
        public GenericListWrapper<BinaryFile> Entries3Wrapper { get; private set; }

        [Browsable( false )]
        public GenericListWrapper<BinaryFile> Entries4Wrapper { get; private set; }

        [Browsable( false )]
        public GenericListWrapper<BinaryFile> Entries5Wrapper { get; private set; }

        [Browsable( false )]
        public GenericListWrapper<BinaryFile> Entries6Wrapper { get; private set; }

        [Browsable( false )]
        public GenericListWrapper<BinaryFile> Entries7Wrapper { get; private set; }

        [Browsable( false )]
        public GenericListWrapper<BinaryFile> Entries8Wrapper { get; private set; }

        public int VersionMajor
        {
            get => Resource.VersionMajor;
            set => SetProperty( Resource, value );
        }

        public int VersionMinor
        {
            get => Resource.VersionMinor;
            set => SetProperty( Resource, value );
        }

        public int Entry1Count => Resource.Entry1Count;

        public int Entry1Size => Resource.Entry1Size;

        public int Entry2Count => Resource.Entry2Count;

        public int Entry2Size => Resource.Entry2Size;

        public int Entry3Count => Resource.Entry3Count;

        public int Entry3Size => Resource.Entry3Size;

        public int Entry4Count => Resource.Entry4Count;

        public int Entry4Size => Resource.Entry4Size;

        public int Entry5Count => Resource.Entry5Count;

        public int Entry5Size => Resource.Entry5Size;

        public int Entry6Count => Resource.Entry6Count;

        public int Entry6Size => Resource.Entry6Size;

        public int Entry7Count => Resource.Entry7Count;

        public int Entry7Size => Resource.Entry7Size;

        public int Entry8Count => Resource.Entry8Count;

        public int Entry8Size => Resource.Entry8Size;

        public HbnFileWrapper( string text, HbnFile resource ) : base( text, resource )
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Add |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction( SupportedFileType.HbnFile, ( res, path ) => res.Save( path ) );
            RegisterFileReplaceAction( SupportedFileType.HbnFile, ( res, path ) => new HbnFile( path ) );
            RegisterFileAddAction( SupportedFileType.Resource, DefaultFileAddAction );
            RegisterRebuildAction( ( wrap ) =>
            {
                HbnFile file = new HbnFile( Resource.VersionMajor, Resource.VersionMinor );

                foreach ( var node in Entries1Wrapper.Resource )
                {
                    file.Entries1.Add(node);
                }

                foreach ( var node in Entries2Wrapper.Resource )
                {
                    file.Entries2.Add( node );
                }

                foreach ( var node in Entries3Wrapper.Resource )
                {
                    file.Entries3.Add( node );
                }

                foreach ( var node in Entries4Wrapper.Resource )
                {
                    file.Entries4.Add( node );
                }

                foreach ( var node in Entries5Wrapper.Resource )
                {
                    file.Entries5.Add( node );
                }

                foreach ( var node in Entries6Wrapper.Resource )
                {
                    file.Entries6.Add( node );
                }

                foreach ( var node in Entries7Wrapper.Resource )
                {
                    file.Entries7.Add( node );
                }

                foreach ( var node in Entries8Wrapper.Resource )
                {
                    file.Entries8.Add( node );
                }

                file.FooterData = Resource.FooterData;
                return file;
            } );
        }

        protected override void PopulateView()
        {
            Entries1Wrapper = new GenericListWrapper<BinaryFile>( "Entries1", Resource.Entries1, ( r, i ) => $"Entry1_{i:D2}" );
            Entries2Wrapper = new GenericListWrapper<BinaryFile>( "Entries2", Resource.Entries2, ( r, i ) => $"Entry2_{i:D2}" );
            Entries3Wrapper = new GenericListWrapper<BinaryFile>( "Entries3", Resource.Entries3, ( r, i ) => $"Entry3_{i:D2}" );
            Entries4Wrapper = new GenericListWrapper<BinaryFile>( "Entries4", Resource.Entries4, ( r, i ) => $"Entry4_{i:D2}" );
            Entries5Wrapper = new GenericListWrapper<BinaryFile>( "Entries5", Resource.Entries5, ( r, i ) => $"Entry5_{i:D2}" );
            Entries6Wrapper = new GenericListWrapper<BinaryFile>( "Entries6", Resource.Entries6, ( r, i ) => $"Entry6_{i:D2}" );
            Entries7Wrapper = new GenericListWrapper<BinaryFile>( "Entries7", Resource.Entries7, ( r, i ) => $"Entry7_{i:D2}" );
            Entries8Wrapper = new GenericListWrapper<BinaryFile>( "Entries8", Resource.Entries8, ( r, i ) => $"Entry8_{i:D2}" );

            Nodes.Add( Entries1Wrapper );
            Nodes.Add( Entries2Wrapper );
            Nodes.Add( Entries3Wrapper );
            Nodes.Add( Entries4Wrapper );
            Nodes.Add( Entries5Wrapper );
            Nodes.Add( Entries6Wrapper );
            Nodes.Add( Entries7Wrapper );
            Nodes.Add( Entries8Wrapper );
        }
    }
}
