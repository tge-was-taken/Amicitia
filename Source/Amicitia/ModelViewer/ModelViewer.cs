using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenTK.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using AmicitiaLibrary.Graphics.RenderWare;
using AmicitiaLibrary.PS2.Graphics;
using SN = System.Numerics;

namespace Amicitia.ModelViewer
{
    public struct GpuModel
    {
        public int Vbo;
        public int[] Ibo;
        public int Vc;
        public int[] Ic;
        public string[] Tid;
        public Color[] Color;
        public bool Strip;
        public GpuModel(int vbo, int[] ibo, int vc, int[] ic, string[] tid, Color[] color, bool strip)
        {
            this.Vbo = vbo;
            this.Ibo = ibo;
            this.Vc = vc;
            this.Ic = ic;
            this.Tid = tid;
            this.Color = color;
            this.Strip = strip;
        }
    }

    public struct BasicCol4
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public BasicCol4(Color color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }
    }

    public struct BasicVec4
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public BasicVec4(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public BasicVec4(Vector4 v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            W = v.W;
        }

        public BasicVec4(Color4 v)
        {
            X = v.R;
            Y = v.G;
            Z = v.B;
            W = v.A;
        }
    }

    public struct BasicVec3
    {
        public float X;
        public float Y;
        public float Z;

        public BasicVec3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public BasicVec3(SN.Vector3 v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }
    }

    public struct BasicVec2
    {
        public float X, Y;

        public BasicVec2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public BasicVec2(SN.Vector2 v)
        {
            X = v.X;
            Y = v.Y;
        }

    }

    public struct Vertex
    {
        public BasicVec3 Pos; 
        public BasicVec3 Nrm; 
        public BasicVec2 Tex;
        public BasicVec4 Col;

        public Vertex(BasicVec3 pos, BasicVec3 nrm, BasicVec2 tex, BasicVec4 col)
        {
            this.Pos = pos;
            this.Nrm = nrm;
            this.Tex = tex;
            this.Col = col;
        }
    }

    public class Camera
    {
        public Camera()
        {
            // default settings
            Target = new Vector3( 0, 100, 0 );
            Position = new Vector3(0, 100, 150);
            Forward = Vector3.Normalize( Target - Position );
            Up = new Vector3(0, 1, 0);
        }

        public Vector3 Position { get; set; }

        public Quaternion Rotation { get; set; }

        public Vector3 Forward { get; set; }

        public Vector3 Up { get; set; }

        public Vector3 Target { get; set; }

        public void Bind(ShaderProgram shader, GLControl control)
        {
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60.0f), (float)control.Width / control.Height, 0.1f, 100000.0f);
            Matrix4 view = Matrix4.LookAt(Position, Position + Forward, Up);
            shader.SetUniform("proj", proj);
            shader.SetUniform("view", view);
        }
    }

    internal static class RwtoGlConversionHelper
    {
        public static Dictionary<PS2FilterMode, int> FilterDictionary { get; } = new Dictionary<PS2FilterMode, int>()
        {
            { PS2FilterMode.None,               (int)TextureMagFilter.Linear },
            { PS2FilterMode.Nearest,            (int)TextureMagFilter.Nearest },
            { PS2FilterMode.Linear,             (int)TextureMagFilter.Linear },
            { PS2FilterMode.MipNearest,         (int)TextureMagFilter.Nearest },
            { PS2FilterMode.MipLinear,          (int)TextureMagFilter.Linear },
            { PS2FilterMode.LinearMipNearest,   (int)TextureMagFilter.Nearest },
            { PS2FilterMode.LinearMipLinear,    (int)TextureMagFilter.Linear }
        };

        public static Dictionary<PS2AddressingMode, int> WrapDictionary { get; } = new Dictionary<PS2AddressingMode, int>()
        {
            { PS2AddressingMode.None,       (int)TextureWrapMode.Repeat },
            { PS2AddressingMode.Wrap,       (int)TextureWrapMode.Repeat },
            { PS2AddressingMode.Mirror,     (int)TextureWrapMode.MirroredRepeat },
            { PS2AddressingMode.Clamp,      (int)TextureWrapMode.ClampToBorder }
        };
    }

    public class ModelViewer
    {
        // view states
        private bool mCanRender;
        private bool mIsViewReady;
        private bool mIsViewFocused;

        // store a handle to the loaded scene

        // gl control handle
        private GLControl mViewerCtrl;

        // model render data
        private List<GpuModel> mModels;
        private Dictionary<string, int> mTexLookup;
        private ShaderProgram mShaderProg;
        private int mCurrentGpuModelId;
        private int mCurrentTextureId;
        private Matrix4 mTransform;

        // camera data
        private Camera mCamera;
        private Vector3 mTp;

        // clear color
        private Color mBgColor;

        public Color BgColor
        {
            get { return mBgColor; }
            set
            {
                mBgColor = value;
                GL.ClearColor(mBgColor);
                mViewerCtrl.Invalidate();
            }
        }

        public static bool IsSupported
        {
            get
            {
                var version = GL.GetString( StringName.Version );
                return version[0] >= '3' && version[2] >= '3'; // X.Y
            }
        }

        public ModelViewer(GLControl controller)
        {
            mCanRender = false;

            if ( !InitializeShaderProgram() )
                return;

            mCanRender = true;

            string version = GL.GetString(StringName.Version);
            Console.WriteLine( version );

            mIsViewReady = false;
            mViewerCtrl = controller;
            mModels = new List<GpuModel>();
            mTexLookup = new Dictionary<string, int>();

            GL.ClearColor(new Color4(128, 128, 128, 255));
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            mViewerCtrl.Paint += ModelViewerPaint;
            mViewerCtrl.Resize += ModelViewerResize;
            mViewerCtrl.Enter += (s, e) => { mIsViewFocused = true; };
            mViewerCtrl.Leave += (s, e) => { mIsViewFocused = false; };
            mViewerCtrl.KeyPress += Input;

            mTp = new Vector3();
            mIsViewReady = true;
            GL.Viewport(0, 0, controller.Width, controller.Height);
            mCamera = new Camera();
        }

        private bool InitializeShaderProgram()
        {
            try
            {
                // set up shader program
                mShaderProg = new ShaderProgram( "shader" );
                mShaderProg.AddUniform( "proj" );
                mShaderProg.AddUniform( "view" );
                mShaderProg.AddUniform( "tran" );
                mShaderProg.AddUniform( "diffuse" );
                mShaderProg.AddUniform( "diffuseColor" );
                mShaderProg.AddUniform( "isTextured" ); //used like a bool but is actually an int because of glsl design limitations ¬~¬
                mShaderProg.Bind();
            }
            catch ( Exception )
            {
                return false;
            }

            return true;
        }

        private void Input(object sender, System.Windows.Forms.KeyPressEventArgs args )
        {
            if (!mIsViewFocused || !mCanRender )
                return;

            Vector3 oldpos = mCamera.Position;
            float moveSpeed = 10f;
            if (NativeMethods.GetAsyncKey(Keys.Shift))
                moveSpeed *= 2;

            bool inputWasHandled = false;

            if ( NativeMethods.GetAsyncKey( Keys.W ) )
            {
                mCamera.Position += mCamera.Forward * moveSpeed; // tp = new Vector3(tp.X, tp.Y, tp.Z + 10f);
                inputWasHandled = true;
            }

            if (NativeMethods.GetAsyncKey(Keys.S))
            {
                mCamera.Position -= mCamera.Forward * moveSpeed; // tp = new Vector3(tp.X, tp.Y, tp.Z - 10f);
                inputWasHandled = true;
            }

            if (NativeMethods.GetAsyncKey(Keys.D))
            {
                mCamera.Position -= Vector3.Cross(mCamera.Forward, new Vector3(0, 1.0f, 0)) * moveSpeed; // tp = new Vector3(tp.X, tp.Y + 10f, tp.Z);
                inputWasHandled = true;
            }

            if (NativeMethods.GetAsyncKey(Keys.A))
            {
                mCamera.Position += Vector3.Cross(mCamera.Forward, new Vector3(0, 1.0f, 0)) * moveSpeed; // tp = new Vector3(tp.X, tp.Y - 10f, tp.Z);
                inputWasHandled = true;
            }

            if (NativeMethods.GetAsyncKey(Keys.Q))
            {
                mCamera.Position += new Vector3(0, 1.0f, 0) * moveSpeed; // tp = new Vector3(tp.X, tp.Y - 10f, tp.Z);
                inputWasHandled = true;
            }

            if (NativeMethods.GetAsyncKey(Keys.E))
            {
                mCamera.Position -= new Vector3(0, 1.0f, 0) * moveSpeed; // tp = new Vector3(tp.X, tp.Y - 10f, tp.Z);
                inputWasHandled = true;
            }

            if ( inputWasHandled && IsNaN( mCamera.Position))
            {
                if (!IsNaN(oldpos))
                {
                    mCamera.Position = oldpos; // prevents the camera from dying
                }
                else
                {
                    // shouldn't really happen
                    mCamera.Position = new Vector3();
                }
            }

            mCamera.Forward = Vector3.Normalize(mCamera.Target - mCamera.Position);

            if (inputWasHandled)
            {
                mViewerCtrl.Invalidate();
            }

            Console.WriteLine( mCamera.Position );
        }

        public static bool IsNaN(Vector3 value)
        {
            return float.IsNaN(value.X) || float.IsNaN(value.Y) || float.IsNaN(value.Z);
        }

        public void LoadTextures(RwTextureDictionaryNode textures)
        {
            if (textures == null || !mCanRender )
                return;

            int textureIdx = 0;
            foreach ( RwTextureNativeNode texture in textures.Textures )
            {
                Console.WriteLine( "processing texture: {0}", textureIdx++ );

                var colorpixels = texture.GetPixels();

                // get the pixel array
                var pixels = new BasicCol4[texture.Width * texture.Height];
                for ( int i = 0; i < texture.Width * texture.Height; i++ )
                    pixels[i] = new BasicCol4( colorpixels[i] );

                // create the texture
                int tex = GL.GenTexture();
                GL.BindTexture( TextureTarget.Texture2D, tex );

                // GL 4.5
                //GL.CreateTextures( TextureTarget.Texture2D, 1, out int tex );

                // set up the params
                GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, RwtoGlConversionHelper.WrapDictionary[texture.HorrizontalAddressingMode] );
                GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapT, RwtoGlConversionHelper.WrapDictionary[texture.VerticalAddressingMode] );
                GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, RwtoGlConversionHelper.FilterDictionary[texture.FilterMode] );
                GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, RwtoGlConversionHelper.FilterDictionary[texture.FilterMode] );

                // GL 4.5
                //GL.TextureParameter( tex, TextureParameterName.TextureWrapS, RwtoGlConversionHelper.WrapDictionary[texture.HorrizontalAddressingMode] );
                //GL.TextureParameter( tex, TextureParameterName.TextureWrapT, RwtoGlConversionHelper.WrapDictionary[texture.VerticalAddressingMode] );
                //GL.TextureParameter( tex, TextureParameterName.TextureMagFilter, RwtoGlConversionHelper.FilterDictionary[texture.FilterMode] );
                //GL.TextureParameter( tex, TextureParameterName.TextureMinFilter, RwtoGlConversionHelper.FilterDictionary[texture.FilterMode] );

                // set up the bitmap data
                GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, texture.Width, texture.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels );

                // GL 4.5
                //GL.TextureStorage2D( tex, 1, SizedInternalFormat.Rgba8, texture.Width, texture.Height );
                //GL.TextureSubImage2D( tex, 0, 0, 0, texture.Width, texture.Height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels );

                // add a texture lookup for the processed texture
                mTexLookup.Add( texture.Name, tex );
            }
        }

        public void LoadModel(RwClumpNode clump)
        {
            if ( !mCanRender )
                return;

            Console.WriteLine( "geometry count: {0}", clump.GeometryCount );

            for ( var atomicIndex = 0; atomicIndex < clump.Atomics.Count; atomicIndex++ )
            {
                RwAtomicNode atomic = clump.Atomics[atomicIndex];
                Console.WriteLine( "processing draw call: {0}", atomicIndex );
                Console.WriteLine( "geo:{0}\t frame:{1}\t flag1:{2}\t flag2{3}\t", atomic.GeometryIndex, atomic.FrameIndex, atomic.Flag1, atomic.Flag2 );

                var geom = clump.GeometryList[atomic.GeometryIndex];
                var frame = clump.FrameList[atomic.FrameIndex];

                LoadGeometry(geom, frame.WorldTransform);
            }
        }

        public void LoadGeometry(RwGeometryNode geom, SN.Matrix4x4 transform)
        {
            if ( !mCanRender )
                return;

            if (geom.MeshListNode == null)
            {
                geom.MeshListNode = new RwMeshListNode(geom);
            }

            Console.WriteLine( geom.MeshListNode.MeshCount );
            Console.WriteLine( geom.MeshListNode.PrimitiveType );

            Vertex[] vertices = new Vertex[geom.VertexCount];
            int[][] allIndices = new int[geom.MeshListNode.MeshCount][];
            int[] allIndicesCount = new int[geom.MeshListNode.MeshCount];
            int[] fullIndices = new int[geom.TriangleCount * 3];

            for ( int i = 0; i < geom.TriangleCount; i++ )
            {
                fullIndices[i * 3 + 0] = geom.Triangles[i].A;
                fullIndices[i * 3 + 1] = geom.Triangles[i].B;
                fullIndices[i * 3 + 2] = geom.Triangles[i].C;
            }

            // remap the vertices
            for ( int i = 0; i < geom.VertexCount; i++ )
            {
                // set the new interleaved vertex
                Vertex vtx = new Vertex
                {
                    Pos = new BasicVec3( SN.Vector3.Transform( geom.Vertices[i], transform ) )
                };

                if ( geom.HasNormals )
                    vtx.Nrm = new BasicVec3( SN.Vector3.TransformNormal( geom.Normals[i], transform ) );

                if ( geom.HasTextureCoordinates )
                    vtx.Tex = new BasicVec2( geom.TextureCoordinateChannels[0][i] );

                if ( geom.HasColors )
                    vtx.Col = new BasicVec4( geom.Colors[i] );
                else
                    vtx.Col = new BasicVec4( 1, 1, 1, 1 );

                vertices[i] = vtx;
            }

            for ( int i = 0; i < geom.MeshListNode.MeshCount; i++ )
            {
                allIndices[i] = geom.MeshListNode.MaterialMeshes[i].Indices;
                allIndicesCount[i] = geom.MeshListNode.MaterialMeshes[i].IndexCount;
            }

            Console.WriteLine( "tex channels: " + geom.TextureCoordinateChannelCount );
            Console.WriteLine( "num materials: " + geom.MaterialCount );

            for ( int m = 0; m < geom.MaterialCount; m++ )
                Console.WriteLine( "material: {0}", geom.Materials[m] );

            // setup the vbo
            int vbo = GL.GenBuffer();
            GL.BindBuffer( BufferTarget.ArrayBuffer, vbo );
            GL.BufferData( BufferTarget.ArrayBuffer, sizeof( float ) * 12 * vertices.Length, vertices, BufferUsageHint.StaticDraw );

            // setup the ibo
            int[] ibo = new int[allIndices.Length];
            GL.GenBuffers( allIndices.Length, ibo );
            for ( int i = 0; i < ibo.Length; i++ )
            {
                GL.BindBuffer( BufferTarget.ElementArrayBuffer, ibo[i] );
                GL.BufferData( BufferTarget.ElementArrayBuffer, sizeof( int ) * allIndices[i].Length, allIndices[i], BufferUsageHint.StaticDraw );
            }

            // setup the vertex attribs
            GL.VertexAttribPointer( 0, 3, VertexAttribPointerType.Float, false, sizeof( float ) * 12, 0 );
            GL.VertexAttribPointer( 1, 3, VertexAttribPointerType.Float, false, sizeof( float ) * 12, ( sizeof( float ) * 3 ) );
            GL.VertexAttribPointer( 2, 2, VertexAttribPointerType.Float, false, sizeof( float ) * 12, ( sizeof( float ) * 6 ) );
            GL.VertexAttribPointer( 3, 4, VertexAttribPointerType.Float, false, sizeof( float ) * 12, ( sizeof( float ) * 8 ) );

            // get the mesh color from the material
            // get the texture name if there's a texture assigned
            string[] texname = new string[geom.MeshListNode.MeshCount];
            Color[] colors = new Color[geom.MeshListNode.MeshCount];
            for ( int i = 0; i < texname.Length; i++ )
            {
                texname[i] = geom.MaterialCount > 0 && geom.Materials[geom.MeshListNode.MaterialMeshes[i].MaterialIndex].IsTextured ?
                        geom.Materials[geom.MeshListNode.MaterialMeshes[i].MaterialIndex].TextureReferenceNode.ReferencedTextureName
                        : string.Empty;

                colors[i] = geom.MaterialCount > 0
                    ? geom.Materials[geom.MeshListNode.MaterialMeshes[i].MaterialIndex].Color
                    : Color.White;
            }

            // add the render model to the list
            mModels.Add( new GpuModel( vbo, ibo, geom.VertexCount, allIndicesCount, texname, colors,
                geom.MeshListNode.PrimitiveType == RwPrimitiveType.TriangleStrip ) );
        }

        public void LoadScene(RmdScene rmdScene)
        {
            if ( !mCanRender )
                return;

            Console.WriteLine("loading scene");
            Console.WriteLine("num textures: {0} ", rmdScene.TextureDictionary != null ? rmdScene.TextureDictionary.TextureCount : 0);

            LoadTextures(rmdScene.TextureDictionary);

            foreach (var clump in rmdScene.Clumps)
            {
                LoadModel( clump );
            }
        }

        public void DeleteScene()
        {
            if ( !mCanRender )
                return;

            Console.WriteLine("deleting scene");
            EndBind();
            Console.WriteLine("unbind models");
            GL.BindTexture(TextureTarget.Texture2D, 0);
            Console.WriteLine("unbind textures");
            mCurrentTextureId = mCurrentGpuModelId = 0;

            // death to the textures
            int[] texIds = new int[mTexLookup.Count];
            mTexLookup.Values.CopyTo(texIds, 0);
            for (int i = 0; i < mTexLookup.Count; i++)
            {
                Console.WriteLine("deleting texture: " + i);
                GL.DeleteTexture(texIds[i]);
            }

            // death to models
            for (int i = 0; i < mModels.Count; i++)
            {
                Console.WriteLine("deleting model: " + i);

                if(mModels[i].Vbo != 0)
                    GL.DeleteBuffer(mModels[i].Vbo);

                if(mModels[i].Ibo[0] != 0)
                    GL.DeleteBuffers(mModels[0].Ibo.Length, mModels[i].Ibo);
            }

            mModels.Clear();
            mTexLookup.Clear();
        }

        public void DisposeViewer()
        {
            Console.WriteLine("disposing");
            DeleteScene();
            mShaderProg.Delete();
            mIsViewReady = false;
        }

        private void BeginBind(GpuModel data)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, data.Vbo);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 12, 0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 12, sizeof(float) * 3);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(float) * 12, sizeof(float) * 6);
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, sizeof(float) * 12, sizeof(float) * 8);
        }

        private void SubBind(GpuModel data)
        {
            for (int i = 0; i < data.Ibo.Length; i++)
            {
                int isTextured = 0;

                if (data.Tid[i] != string.Empty)
                {
                    bool texPresent = mTexLookup.TryGetValue( data.Tid[i], out int texIdx );

                    if (texPresent)
                    {
                        isTextured = 1;
                        FullTexBind(texIdx);
                    }
                }

                // set shader vars
                mShaderProg.SetUniform("diffuseColor", data.Color[i]);
                mShaderProg.SetUniform("isTextured", isTextured);

                // bind model
                GL.BindBuffer(BufferTarget.ArrayBuffer, data.Vbo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, data.Ibo[i]);
                GL.DrawElements(data.Strip ? BeginMode.TriangleStrip : BeginMode.Triangles, data.Ic[i], DrawElementsType.UnsignedInt, 0);
            }
        }

        private void EndBind()
        {
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);
            GL.DisableVertexAttribArray(3);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        private void FullBind(GpuModel data) // for optimization
        {
            if (data.Vbo == 0)
                return;

            if (mCurrentGpuModelId != data.Vbo)
            {
                EndBind();  // incase the previous was a SubBind
                BeginBind(data);
                SubBind(data);
                mCurrentGpuModelId = data.Vbo;
            }
            else
            {
                SubBind(data);
            }
        }

        private void FullTexBind(int texture)
        {
            if (texture == 0 || texture == mCurrentTextureId)
                return;

            GL.BindTexture(TextureTarget.Texture2D, texture);
            mCurrentTextureId = texture;
        }

        private void ModelViewerResize(object sender, EventArgs e)
        {
            if (!mIsViewReady || !mCanRender )
                return;

            GL.Viewport(0, 0, mViewerCtrl.Width, mViewerCtrl.Height);
        }

        private void ModelViewerPaint(object sender, PaintEventArgs e)
        {
            if (!mIsViewReady || !mViewerCtrl.Visible || !mCanRender )
                return;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            mShaderProg.Bind();
            mCamera.Bind( mShaderProg, mViewerCtrl );
            mTransform = Matrix4.CreateTranslation( mTp ) * Matrix4.CreateFromQuaternion( Quaternion.FromEulerAngles( 0, 0, 0 ) ) * Matrix4.CreateScale( new Vector3( 1.0f, 1.0f, 1.0f ) );
            mShaderProg.SetUniform( "tran", mTransform );

            if ( mModels.Count > 0 )
                for ( int i = 0; i < mModels.Count; i++ )
                    FullBind( mModels[i] );

            mViewerCtrl.SwapBuffers();
        }
    }
}
