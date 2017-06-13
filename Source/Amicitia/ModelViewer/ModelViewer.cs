using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenTK.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using AtlusLibSharp.Graphics.RenderWare;
using AtlusLibSharp.PS2.Graphics;
using SN = System.Numerics;
using System.Runtime.InteropServices;

namespace Amicitia.ModelViewer
{
    public struct GPUModel
    {
        public int vbo;
        public int[] ibo;
        public int vc;
        public int[] ic;
        public string[] tid;
        public Color[] color;
        public bool strip;
        public GPUModel(int vbo, int[] ibo, int vc, int[] ic, string[] tid, Color[] color, bool strip)
        {
            this.vbo = vbo;
            this.ibo = ibo;
            this.vc = vc;
            this.ic = ic;
            this.tid = tid;
            this.color = color;
            this.strip = strip;
        }
    }

    public struct BasicCol4
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;

        public BasicCol4(Color color)
        {
            r = color.R;
            g = color.G;
            b = color.B;
            a = color.A;
        }
    }

    public struct BasicVec4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public BasicVec4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public BasicVec4(Vector4 v)
        {
            x = v.X;
            y = v.Y;
            z = v.Z;
            w = v.W;
        }

        public BasicVec4(Color4 v)
        {
            x = v.R;
            y = v.G;
            z = v.B;
            w = v.A;
        }
    }

    public struct BasicVec3
    {
        public float x;
        public float y;
        public float z;

        public BasicVec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public BasicVec3(SN.Vector3 v)
        {
            x = v.X;
            y = v.Y;
            z = v.Z;
        }
    }

    public struct BasicVec2
    {
        public float x, y;

        public BasicVec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public BasicVec2(SN.Vector2 v)
        {
            x = v.X;
            y = v.Y;
        }

    }

    public struct Vertex
    {
        public BasicVec3 pos; 
        public BasicVec3 nrm; 
        public BasicVec2 tex;
        public BasicVec4 col;

        public Vertex(BasicVec3 pos, BasicVec3 nrm, BasicVec2 tex, BasicVec4 col)
        {
            this.pos = pos;
            this.nrm = nrm;
            this.tex = tex;
            this.col = col;
        }
    }

    public class Camera
    {
        private Vector3 _position;
        private Vector3 _forward;
        private Vector3 _up;
        private Quaternion _rot;

        public Camera()
        {
            // default settings
            _position = new Vector3(100, 100, 100);
            _forward = new Vector3(0, 0, 1);
            _up = new Vector3(0, 1, 0);
        }

        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public Quaternion Rotation
        {
            get { return _rot; }
            set { _rot = value; }
        }

        public Vector3 Forward
        {
            get { return _forward; }
            set { _forward = value; }
        }

        public void Bind(ShaderProgram shader, GLControl control)
        {
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60.0f), (float)control.Width / control.Height, 0.1f, 10000.0f);
            Matrix4 view = Matrix4.LookAt(_position, _position + _forward, _up);
            shader.SetUniform("proj", proj);
            shader.SetUniform("view", view);
        }
    }

    internal static class RWToGLConversionHelper
    {
        private static Dictionary<PS2FilterMode, int> _ps2Filter_GLFilter_dict = new Dictionary<PS2FilterMode, int>()
        {
            { PS2FilterMode.None,               (int)TextureMagFilter.Linear },
            { PS2FilterMode.Nearest,            (int)TextureMagFilter.Nearest },
            { PS2FilterMode.Linear,             (int)TextureMagFilter.Linear },
            { PS2FilterMode.MipNearest,         (int)TextureMagFilter.Nearest },
            { PS2FilterMode.MipLinear,          (int)TextureMagFilter.Linear },
            { PS2FilterMode.LinearMipNearest,   (int)TextureMagFilter.Nearest },
            { PS2FilterMode.LinearMipLinear,    (int)TextureMagFilter.Linear }
        };

        private static Dictionary<PS2AddressingMode, int> _ps2AddrMode_GLWrapMode_dict = new Dictionary<PS2AddressingMode, int>()
        {
            { PS2AddressingMode.None,       (int)TextureWrapMode.Repeat },
            { PS2AddressingMode.Wrap,       (int)TextureWrapMode.Repeat },
            { PS2AddressingMode.Mirror,     (int)TextureWrapMode.MirroredRepeat },
            { PS2AddressingMode.Clamp,      (int)TextureWrapMode.ClampToBorder }
        };

        public static Dictionary<PS2FilterMode, int> FilterDictionary
        {
            get { return _ps2Filter_GLFilter_dict; }
        }

        public static Dictionary<PS2AddressingMode, int> WrapDictionary
        {
            get { return _ps2AddrMode_GLWrapMode_dict; }
        }

    }

    public class ModelViewer
    {
        // view states
        private bool _isViewReady;
        private bool _isViewFocused;

        // scene states
        private bool _isSceneReady;

        // store a handle to the loaded scene
        private RMDScene _loadedScene;

        // gl control handle
        private GLControl _viewerCtrl;

        // model render data
        private List<GPUModel> _models;
        private Dictionary<string, int> _texLookup;
        private ShaderProgram _shaderProg;
        private int _currentGPUModelID;
        private int _currentTextureID;
        private Matrix4 _transform;

        // camera data
        private Camera _camera;
        private Vector3 _tp;
        private Vector3 _cameraTarget;

        // clear color
        private Color _bgColor;

        public Color BGColor
        {
            get { return _bgColor; }
            set
            {
                _bgColor = value;
                GL.ClearColor(_bgColor);
                _viewerCtrl.Invalidate();
            }
        }

        public bool IsSceneReady
        {
            get { return _isSceneReady; }
        }

        public RMDScene LoadedScene
        {
            get { return _loadedScene; }
        }

        public ModelViewer(GLControl controller)
        {
            Console.WriteLine(GL.GetString(StringName.Version));
            _isViewReady = _isSceneReady = false;
            _viewerCtrl = controller;
            _models = new List<GPUModel>();
            _texLookup = new Dictionary<string, int>();
            GL.ClearColor(new Color4(128, 128, 128, 255));
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            
            _viewerCtrl.Paint += ModelViewerPaint;
            _viewerCtrl.Resize += ModelViewerResize;
            _viewerCtrl.Enter += (object sender, EventArgs e) => { _isViewFocused = true; };
            _viewerCtrl.Leave += (object sender, EventArgs e) => { _isViewFocused = false; };
            
            _tp = new Vector3();
            _isViewReady = true;
            GL.Viewport(0, 0, controller.Width, controller.Height);
        }

        private void Input()
        {
            if (!_isSceneReady || !_isViewFocused)
                return;

            Vector3 oldpos = _camera.Position;
            const float MOVE_SPEED = 0.1f;
            bool inputWasHandled = true;

            if (NativeMethods.GetAsyncKey(Keys.W))
            {
                _camera.Position += _camera.Forward * MOVE_SPEED; // tp = new Vector3(tp.X, tp.Y, tp.Z + 10f);
            }
            else if (NativeMethods.GetAsyncKey(Keys.S))
            {
                _camera.Position -= _camera.Forward * MOVE_SPEED; // tp = new Vector3(tp.X, tp.Y, tp.Z - 10f);
            }
            else if (NativeMethods.GetAsyncKey(Keys.D))
            {
                _camera.Position -= Vector3.Cross(_camera.Forward, new Vector3(0, 1.0f, 0)) * MOVE_SPEED; // tp = new Vector3(tp.X, tp.Y + 10f, tp.Z);
            }
            else if (NativeMethods.GetAsyncKey(Keys.A))
            {
                _camera.Position += Vector3.Cross(_camera.Forward, new Vector3(0, 1.0f, 0)) * MOVE_SPEED; // tp = new Vector3(tp.X, tp.Y - 10f, tp.Z);
            }
            else if (NativeMethods.GetAsyncKey(Keys.Q))
            {
                _camera.Position += new Vector3(0, 1.0f, 0) * MOVE_SPEED; // tp = new Vector3(tp.X, tp.Y - 10f, tp.Z);
            }
            else if (NativeMethods.GetAsyncKey(Keys.E))
            {
                _camera.Position -= new Vector3(0, 1.0f, 0) * MOVE_SPEED; // tp = new Vector3(tp.X, tp.Y - 10f, tp.Z);
            }
            else
            {
                inputWasHandled = false;
            }

            if (IsNaN(_camera.Position))
            {
                if (!IsNaN(oldpos))
                {
                    _camera.Position = oldpos; // prevents the camera from dying
                }
                else
                {
                    // shouldn't really happen
                    _camera.Position = new Vector3();
                }
            }

            _camera.Forward = Vector3.Normalize(_cameraTarget - _camera.Position);

            if (inputWasHandled)
            {
                _viewerCtrl.Invalidate();
            }
        }

        public static bool IsNaN(Vector3 value)
        {
            if (float.IsNaN(value.X))
                return true;
            else if (float.IsNaN(value.Y))
                return true;
            else if (float.IsNaN(value.Z))
                return true;
            else
                return false;
        }

        public void LoadScene(RMDScene rmdScene)
        {
            _camera = new Camera();

            // set up shader program
            _shaderProg = new ShaderProgram("shader");
            _shaderProg.AddUniform("proj");
            _shaderProg.AddUniform("view");
            _shaderProg.AddUniform("tran");
            _shaderProg.AddUniform("diffuse");
            _shaderProg.AddUniform("diffuseColor");
            _shaderProg.AddUniform("isTextured"); //used like a bool but is actually an int because of glsl design limitations ¬~¬
            _shaderProg.Bind();

            Console.WriteLine("loading scene");
            Console.WriteLine("num textures: {0} ", rmdScene.TextureDictionary != null ? rmdScene.TextureDictionary.TextureCount : 0);

            if (rmdScene.TextureDictionary != null)
            {
                int textureIdx = 0;
                foreach (RWTextureNative texture in rmdScene.TextureDictionary.Textures)
                {
                    Console.WriteLine("processing texture: {0}", textureIdx++);

                    var colorpixels = texture.GetPixels();

                    // get the pixel array
                    BasicCol4[] pixels = new BasicCol4[texture.Width * texture.Height];
                    for (int i = 0; i < texture.Width * texture.Height; i++)
                        pixels[i] = new BasicCol4(colorpixels[i]);

                    // create the texture
                    int tex = 0;
                    GL.CreateTextures(TextureTarget.Texture2D, 1, out tex);

                    // set up the params
                    GL.TextureParameter(tex, TextureParameterName.TextureWrapS, RWToGLConversionHelper.WrapDictionary[texture.HorrizontalAddressingMode]);
                    GL.TextureParameter(tex, TextureParameterName.TextureWrapT, RWToGLConversionHelper.WrapDictionary[texture.VerticalAddressingMode]);
                    GL.TextureParameter(tex, TextureParameterName.TextureMagFilter, RWToGLConversionHelper.FilterDictionary[texture.FilterMode]);
                    GL.TextureParameter(tex, TextureParameterName.TextureMinFilter, RWToGLConversionHelper.FilterDictionary[texture.FilterMode]);

                    // set up the bitmap data
                    GL.TextureStorage2D(tex, 1, SizedInternalFormat.Rgba8, texture.Width, texture.Height);
                    GL.TextureSubImage2D(tex, 0, 0, 0, texture.Width, texture.Height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

                    // add a texture lookup for the processed texture
                    _texLookup.Add(texture.Name, tex);
                }
            }

            float min_x = 0, min_y = 0, min_z = 0;
            float max_x = 0, max_y = 0, max_z = 0;
            int clumpIdx = 0;
            foreach (RWScene rwScene in rmdScene.Scenes)
            {
                Console.WriteLine("geometry count: {0}", rwScene.MeshCount);
                Console.WriteLine("processing clump: {0}", clumpIdx++);

                int drawCallIdx = 0;
                foreach (RWDrawCall drawCall in rwScene.DrawCalls)
                {
                    Console.WriteLine("processing draw call: {0}", drawCallIdx);
                    Console.WriteLine("geo:{0}\t frame:{1}\t flag1:{2}\t flag2{3}\t", drawCall.MeshIndex, drawCall.NodeIndex, drawCall.Flag1, drawCall.Flag2);

                    var geom = rwScene.Meshes[drawCall.MeshIndex];
                    var frame = rwScene.Nodes[drawCall.NodeIndex];

                    Console.WriteLine(geom.MaterialSplitData.MaterialSplitCount);
                    Console.WriteLine(geom.MaterialSplitData.PrimitiveType);

                    Vertex[] vertices = new Vertex[geom.VertexCount];
                    int[][] allIndices = new int[geom.MaterialSplitData.MaterialSplitCount][];
                    int[] allIndicesCount = new int[geom.MaterialSplitData.MaterialSplitCount];
                    int[] fullIndices = new int[geom.TriangleCount*3];

                    for(int i = 0; i < geom.TriangleCount; i++)
                    {
                        fullIndices[i * 3 + 0] = geom.Triangles[i].A;
                        fullIndices[i * 3 + 1] = geom.Triangles[i].B;
                        fullIndices[i * 3 + 2] = geom.Triangles[i].C;
                    }

                    // remap the vertices
                    for (int i = 0; i < geom.VertexCount; i++)
                    {
                        // set the new interleaved vertex
                        Vertex vtx = new Vertex();
                        vtx.pos = new BasicVec3(SN.Vector3.Transform(geom.Vertices[i], frame.WorldTransform) / 10);

                        if (vtx.pos.x < min_x) min_x = vtx.pos.x;
                        if (vtx.pos.x > max_x) max_x = vtx.pos.x;
                        if (vtx.pos.y < min_y) min_y = vtx.pos.y;
                        if (vtx.pos.y > max_y) max_y = vtx.pos.y;
                        if (vtx.pos.z < min_z) min_z = vtx.pos.z;
                        if (vtx.pos.z > max_z) max_z = vtx.pos.z;

                        if (geom.HasNormals)
                            vtx.nrm = new BasicVec3(SN.Vector3.TransformNormal(geom.Normals[i], frame.WorldTransform));

                        if (geom.HasTexCoords)
                            vtx.tex = new BasicVec2(geom.TextureCoordinateChannels[0][i]);

                        if (geom.HasColors)
                            vtx.col = new BasicVec4(geom.Colors[i]);
                        else
                            vtx.col = new BasicVec4(1, 1, 1, 1);

                        vertices[i] = vtx;
                    }

                    for (int i = 0; i < geom.MaterialSplitData.MaterialSplitCount; i++)
                    {
                        allIndices[i] = geom.MaterialSplitData.MaterialSplits[i].Indices;
                        allIndicesCount[i] = geom.MaterialSplitData.MaterialSplits[i].IndexCount;
                    }

                    Console.WriteLine("tex channels: " + geom.TextureCoordinateChannelCount);
                    Console.WriteLine("num materials: " + geom.MaterialCount);

                    for (int m = 0; m < geom.MaterialCount; m++)
                        Console.WriteLine("material: {0}", geom.Materials[m]);

                    // setup the vbo
                    int vbo = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                    GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 12 * vertices.Length, vertices, BufferUsageHint.StaticDraw);

                    // setup the ibo
                    int[] ibo = new int[allIndices.Length];
                    GL.GenBuffers(allIndices.Length, ibo);
                    for (int i = 0; i < ibo.Length; i++)
                    {
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo[i]);
                        GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * allIndices[i].Length, allIndices[i], BufferUsageHint.StaticDraw);
                    }

                    // setup the vertex attribs
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 12, 0);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 12, (sizeof(float) * 3));
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(float) * 12, (sizeof(float) * 6));
                    GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, sizeof(float) * 12, (sizeof(float) * 8));

                    // get the mesh color from the material
                    // get the texture name if there's a texture assigned
                    string[] texname = new string[geom.MaterialSplitData.MaterialSplitCount];
                    Color[] colors = new Color[geom.MaterialSplitData.MaterialSplitCount];
                    for (int i = 0; i < texname.Length; i++)
                    {
                        texname[i] = geom.MaterialCount > 0 && geom.Materials[geom.MaterialSplitData.MaterialSplits[i].MaterialIndex].IsTextured ? geom.Materials[geom.MaterialSplitData.MaterialSplits[i].MaterialIndex].TextureReference.ReferencedTextureName : string.Empty;
                        colors[i] = geom.MaterialCount > 0 ? geom.Materials[geom.MaterialSplitData.MaterialSplits[i].MaterialIndex].Color : Color.White;
                    }

                    // add the render model to the list
                    _models.Add(new GPUModel(vbo, ibo, geom.VertexCount, allIndicesCount, texname, colors, geom.MaterialSplitData.PrimitiveType == RWPrimitiveType.TriangleStrip));
                }
            }

            _cameraTarget = new Vector3((min_x + max_x) / 2, (min_y + max_y) / 2, (min_z + max_z) / 2);
            Console.WriteLine(_cameraTarget);
            
            // everything's processed, the scene is ready to be rendered
            _isSceneReady = true;
            _loadedScene = rmdScene;

            Program.LoopFunctions.Add(Input);
        }

        public void DeleteScene()
        {
            if (!_isSceneReady)
                return;

            Program.LoopFunctions.Remove(Input);
            Console.WriteLine("deleting scene");
            EndBind();
            Console.WriteLine("unbind models");
            GL.BindTexture(TextureTarget.Texture2D, 0);
            Console.WriteLine("unbind textures");
            _currentTextureID = _currentGPUModelID = 0;

            // death to the textures
            int[] texIds = new int[_texLookup.Count];
            _texLookup.Values.CopyTo(texIds, 0);
            for (int i = 0; i < _texLookup.Count; i++)
            {
                Console.WriteLine("deleting texture: " + i);
                GL.DeleteTexture(texIds[i]);
            }

            // death to models
            for (int i = 0; i < _models.Count; i++)
            {
                Console.WriteLine("deleting model: " + i);

                if(_models[i].vbo != 0)
                    GL.DeleteBuffer(_models[i].vbo);

                if(_models[i].ibo[0] != 0)
                    GL.DeleteBuffers(_models[0].ibo.Length, _models[i].ibo);
            }

            _models.Clear();
            _texLookup.Clear();
            _isSceneReady = false;
            _loadedScene = null;
        }

        public void DisposeViewer()
        {
            Console.WriteLine("disposing");

            if (_isSceneReady)
                DeleteScene();

            if (_shaderProg != null)
                _shaderProg.Delete();

            _isSceneReady = false;
            _loadedScene = null;
            _isViewReady = false;
        }

        private void BeginBind(GPUModel data)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, data.vbo);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 12, 0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 12, sizeof(float) * 3);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(float) * 12, sizeof(float) * 6);
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, sizeof(float) * 12, sizeof(float) * 8);
        }

        private void SubBind(GPUModel data)
        {
            for (int i = 0; i < data.ibo.Length; i++)
            {
                int isTextured = 0;

                if (data.tid[i] != string.Empty)
                {
                    int texIdx;
                    bool texPresent = _texLookup.TryGetValue(data.tid[i], out texIdx);

                    if (texPresent)
                    {
                        isTextured = 1;
                        FullTexBind(texIdx);
                    }
                }

                // set shader vars
                _shaderProg.SetUniform("diffuseColor", data.color[i]);
                _shaderProg.SetUniform("isTextured", isTextured);

                // bind model
                GL.BindBuffer(BufferTarget.ArrayBuffer, data.vbo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, data.ibo[i]);
                GL.DrawElements(data.strip ? BeginMode.TriangleStrip : BeginMode.Triangles, data.ic[i], DrawElementsType.UnsignedInt, 0);
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

        private void FullBind(GPUModel data) // for optimization
        {
            if (data.vbo == 0)
                return;

            if (_currentGPUModelID != data.vbo)
            {
                EndBind();  // incase the previous was a SubBind
                BeginBind(data);
                SubBind(data);
                _currentGPUModelID = data.vbo;
            }
            else
            {
                SubBind(data);
            }
        }

        private void FullTexBind(int texture)
        {
            if (texture == 0 || texture == _currentTextureID)
                return;

            GL.BindTexture(TextureTarget.Texture2D, texture);
            _currentTextureID = texture;
        }

        private void ModelViewerResize(object sender, EventArgs e)
        {
            if (!_isViewReady)
                return;

            GL.Viewport(0, 0, _viewerCtrl.Width, _viewerCtrl.Height);
        }

        private void ModelViewerPaint(object sender, PaintEventArgs e)
        {
            if (!_isViewReady || !_viewerCtrl.Visible)
                return;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (_isSceneReady)
            {
                _shaderProg.Bind();
                _camera.Bind(_shaderProg, _viewerCtrl);
                _transform = Matrix4.CreateTranslation(_tp) * Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(0, 0, 0)) * Matrix4.CreateScale(new Vector3(1.0f, 1.0f, 1.0f));
                _shaderProg.SetUniform("tran", _transform);

                if (_models.Count > 0)
                    for (int i = 0; i < _models.Count; i++)
                        FullBind(_models[i]);
            }

            _viewerCtrl.SwapBuffers();
        }
    }
}
