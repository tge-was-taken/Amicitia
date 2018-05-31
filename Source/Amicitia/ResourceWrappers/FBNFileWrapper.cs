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
    public class FbnFileWrapper : ResourceWrapper<FbnFile>
    {
        [Browsable(false)]
        public GenericListWrapper<BinaryFile> ObjectsWrapper { get; private set; }

        [Browsable(false)]
        public GenericListWrapper<BinaryFile> Entries2Wrapper { get; private set; }

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

        public int ObjectCount => Resource.ObjectCount;

        public int ObjectEntrySize => Resource.ObjectEntrySize;

        public int Entry2Count => Resource.Entry2Count;

        public int Entry2Size => Resource.Entry2Size;

        public FbnFileWrapper( string text, FbnFile resource ) : base( text, resource )
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Add |
                                       CommonContextMenuOptions.Move | CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction( SupportedFileType.FbnFile, ( res, path ) => res.Save( path ) );
            RegisterFileReplaceAction( SupportedFileType.FbnFile, ( res, path ) => new FbnFile( path ) );
            RegisterFileAddAction( SupportedFileType.Resource, DefaultFileAddAction );
            RegisterRebuildAction( ( wrap ) =>
            {
                FbnFile file = new FbnFile(Resource.VersionMajor, Resource.VersionMinor);

                file.Objects.AddRange( ObjectsWrapper.Resource );
                file.Entries2.AddRange( Entries2Wrapper.Resource );

                return file;
            } );
        }

        protected override void PopulateView()
        {
            ObjectsWrapper = new GenericListWrapper<BinaryFile>("Objects", Resource.Objects, (r, i) => $"Object{i:D2}" );
            Entries2Wrapper = new GenericListWrapper<BinaryFile>("Entries2", Resource.Entries2, (r, i) => $"Entry2_{i:D2}");

            Nodes.Add( ObjectsWrapper );
            Nodes.Add( Entries2Wrapper );
        }
    }
}
