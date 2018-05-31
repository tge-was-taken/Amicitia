using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    public class RwUserDataList : RwNode, IList<RwUserData>
    {
        private List<RwUserData> mList;

        public RwUserDataList() : base( RwNodeId.RwUserDataPluginNode )
        {
            mList = new List<RwUserData>();
        }

        public RwUserDataList( RwNode parent ) : base( RwNodeId.RwUserDataPluginNode, parent )
        {
            mList = new List<RwUserData>();
        }

        public RwUserDataList( RwNode parent, uint version ) : base( RwNodeId.RwUserDataPluginNode, parent, version )
        {
            mList = new List<RwUserData>();
        }

        internal RwUserDataList( RwNodeFactory.RwNodeHeader header, BinaryReader reader ) : base( header )
        {
            ReadBody( reader );
        }

        protected internal override void ReadBody( BinaryReader reader )
        {
            var count = reader.ReadInt32();
            mList = new List<RwUserData>( count );
            for ( int i = 0; i < count; i++ )
            {
                var userData = new RwUserData();
                userData.Read( reader );
                Add( userData );
            }
        }

        protected internal override void WriteBody( BinaryWriter writer )
        {
            writer.Write( Count );
            foreach ( var userData in this )
                userData.Write( writer );
        }

        public IEnumerator<RwUserData> GetEnumerator()
        {
            return mList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ( ( IEnumerable ) mList ).GetEnumerator();
        }

        public void Add( RwUserData item )
        {
            mList.Add( item );
        }

        public void Clear()
        {
            mList.Clear();
        }

        public bool Contains( RwUserData item )
        {
            return mList.Contains( item );
        }

        public void CopyTo( RwUserData[] array, int arrayIndex )
        {
            mList.CopyTo( array, arrayIndex );
        }

        public bool Remove( RwUserData item )
        {
            return mList.Remove( item );
        }

        public int Count
        {
            get { return mList.Count; }
        }

        public bool IsReadOnly
        {
            get { return ( ( ICollection<RwUserData> ) mList ).IsReadOnly; }
        }

        public int IndexOf( RwUserData item )
        {
            return mList.IndexOf( item );
        }

        public void Insert( int index, RwUserData item )
        {
            mList.Insert( index, item );
        }

        public void RemoveAt( int index )
        {
            mList.RemoveAt( index );
        }

        public RwUserData this[ int index ]
        {
            get => mList[ index ];
            set => mList[ index ] = value;
        }

        public RwUserData this[ string name ]
        {
            get => mList.FirstOrDefault( x => x.Name == name );
            set
            {
                var existing = this[ name ];
                if ( existing != null )
                    mList.Remove( existing );

                mList.Add( value );
            }
        }
    }
}
