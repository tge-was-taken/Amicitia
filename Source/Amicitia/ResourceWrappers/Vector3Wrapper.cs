using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AmicitiaLibrary.Graphics.RenderWare;

namespace Amicitia.ResourceWrappers
{
    public class Vector3Wrapper : ResourceWrapper<Vector3>
    {
        public float X
        {
            get => Resource.X;
            set => SetProperty( Resource, value );
        }

        public float Y
        {
            get => Resource.Y;
            set => SetProperty( Resource, value );
        }

        public float Z
        {
            get => Resource.Z;
            set => SetProperty( Resource, value );
        }

        public Vector3Wrapper(string text, Vector3 resource) : base(text, resource)
        {
            
        }

        protected override void Initialize()
        {
        }

        protected override void PopulateView()
        {
        }
    }
    public class Vector2Wrapper : ResourceWrapper<Vector2>
    {
        public float X
        {
            get => Resource.X;
            set => SetProperty( Resource, value );
        }

        public float Y
        {
            get => Resource.Y;
            set => SetProperty( Resource, value );
        }

        public Vector2Wrapper( string text, Vector2 resource ) : base( text, resource )
        {

        }

        protected override void Initialize()
        {
        }

        protected override void PopulateView()
        {
        }
    }

    public class ColorWrapper : ResourceWrapper<Color>
    {
        public byte R
        {
            get => Resource.R;
            set => SetProperty( Resource, value );
        }

        public byte G
        {
            get => Resource.G;
            set => SetProperty( Resource, value );
        }

        public byte B
        {
            get => Resource.B;
            set => SetProperty( Resource, value );
        }

        public byte A
        {
            get => Resource.A;
            set => SetProperty( Resource, value );
        }

        public ColorWrapper( string text, Color resource ) : base( text, resource )
        {

        }

        protected override void Initialize()
        {
        }

        protected override void PopulateView()
        {
        }
    }

    public class RwTriangleWrapper : ResourceWrapper<RwTriangle>
    {
        public ushort A
        {
            get => Resource.A;
            set => SetProperty( Resource, value );
        }

        public ushort B
        {
            get => Resource.B;
            set => SetProperty( Resource, value );
        }

        public ushort C
        {
            get => Resource.C;
            set => SetProperty( Resource, value );
        }

        public short MatId
        {
            get => Resource.MatId;
            set => SetProperty( Resource, value );
        }

        public RwTriangleWrapper( string text, RwTriangle resource ) : base( text, resource )
        {

        }

        protected override void Initialize()
        {
        }

        protected override void PopulateView()
        {
        }
    }
}
