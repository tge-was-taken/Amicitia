using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenTK.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using AtlusLibSharp.Utilities;
using AtlusLibSharp.Graphics.RenderWare;
using AtlusLibSharp.PS2.Graphics;

namespace Amicitia.ModelViewer
{
    public struct GPUModel
    {
        public int vbo;
        public int ibo;
        public int vc;
        public int ic;
        public string tid;
        public Color color;

        public GPUModel(int vbo, int ibo, int vc, int ic, string tid, Color color)
        {
            this.vbo = vbo;
            this.ibo = ibo;
            this.vc = vc;
            this.ic = ic;
            this.tid = tid;
            this.color = color;
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

        public BasicVec3(Vector3 v)
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

        public BasicVec2(Vector2 v)
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
            _position = new Vector3(0.0f, 0.0f, 0.0f);
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
        private bool _ready;
        private bool _sceneready;
        private GLControl _viewerCtrl;
        private List<GPUModel> _models;
        private Dictionary<string, int> _texLookup;
        private ShaderProgram _shaderProg;
        private int _currentGPUModelID;
        private int _currentTextureID;
        private Matrix4 _transform;
        private Camera _camera;
        private Vector3 _tp;
        private Vector3 _tr;
        private float _flo = 0;

        public bool IsSceneReady
        {
            get { return _sceneready; }
        }

        public ModelViewer(GLControl controller)
        {
            _ready = _sceneready = false;
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
            _tp = new Vector3();
            _tr = new Vector3();
            _viewerCtrl.KeyPress += ModelViewerKeyPress;
            _ready = true;
            GL.Viewport(0, 0, controller.Width, controller.Height);
        }

        public void LoadScene(RMDScene rmdScene)
        {
            // set up shader program
            _shaderProg = new ShaderProgram("shader");
            _shaderProg.AddUniform("proj");
            _shaderProg.AddUniform("view");
            _shaderProg.AddUniform("tran");
            _shaderProg.AddUniform("diffuse");
            _shaderProg.AddUniform("diffuseColor");
            _shaderProg.Bind();

            _camera = new Camera();
            _transform = Matrix4.CreateTranslation(new Vector3(-10, 0, -200)) * Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(20, 0, 0)) * Matrix4.CreateScale(new Vector3(1.0f, 1.0f, 1.0f));

            Console.WriteLine("loading scene");
            Console.WriteLine("num textures: {0} ", rmdScene.TextureDictionary != null ? rmdScene.TextureDictionary.TextureCount : 0);

            if (rmdScene.TextureDictionary != null)
            {
                int textureIdx = 0;
                foreach (RWTextureNative texture in rmdScene.TextureDictionary.Textures)
                {
                    Console.WriteLine("processing texture: {0}", textureIdx++);

                    // get the pixel array
                    BasicCol4[] pixels = new BasicCol4[texture.Width * texture.Height];
                    for (int i = 0; i < texture.Width * texture.Height; i++)
                        pixels[i] = texture.IsIndexed ? new BasicCol4(texture.Palette[texture.PixelIndices[i]]) : new BasicCol4(texture.Pixels[i]);

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

            Vector3 extentMin = new Vector3(999999, 999999, 999999);
            Vector3 extentMax = new Vector3(-999999, -999999, -999999);

            int clumpIdx = 0;
            foreach (RWScene rwScene in rmdScene.Scenes)
            {
                Console.WriteLine("processing clump: {0}", clumpIdx++);

                int drawCallIdx = 0;
                foreach (RWDrawCall drawCall in rwScene.DrawCalls)
                {
                    Console.WriteLine("processing draw call: {0}", drawCallIdx);
                    Console.WriteLine("geo:{0}\t frame:{1}\t flag1:{2}\t flag2{3}\t", 
                        drawCall.MeshIndex, drawCall.NodeIndex, drawCall.Flag1, drawCall.Flag2);

                    var geom = rwScene.Meshes[drawCall.MeshIndex];
                    var frame = rwScene.Nodes[drawCall.NodeIndex];

                    Vertex[] vertices = new Vertex[geom.VertexCount];
                    ushort[] indices = new ushort[geom.TriangleCount * 3];

                    // remap the vertices
                    for (int i = 0; i < geom.VertexCount; i++)
                    {
                        // get the vertex info
                        var oldPos = geom.HasVertices ? Vector3.Transform(geom.Vertices[i], frame.WorldTransform) : new Vector3(0, 0, 0);
                        var oldNrm = geom.HasNormals ? Vector3.TransformNormal(geom.Normals[i], frame.WorldTransform) : new Vector3(0, 0, 0);
                        var oldTcd = geom.HasTexCoords ? geom.TextureCoordinateChannels[0][i] : new Vector2(0, 0);
                        var oldCol = geom.HasColors ? geom.Colors[i] : Color.White;

                        // min
                        if (oldPos.X < extentMin.X) extentMin.X = oldPos.X;
                        if (oldPos.Y < extentMin.Y) extentMin.Y = oldPos.Y;
                        if (oldPos.Z < extentMin.Z) extentMin.Z = oldPos.Z;

                        // max
                        if (oldPos.X > extentMax.X) extentMax.X = oldPos.X;
                        if (oldPos.X > extentMax.Y) extentMax.Y = oldPos.Y;
                        if (oldPos.X > extentMax.Z) extentMax.Z = oldPos.Z;

                        // create basic vecs
                        var pos = new BasicVec3(oldPos);
                        var nrm = new BasicVec3(oldNrm);
                        var tcd = new BasicVec2(oldTcd);
                        var col = new BasicVec4(oldCol);

                        // set the new interleaved vertex
                        vertices[i] = new Vertex(pos, nrm, tcd, col);
                    }

                    // remap the triangle indices
                    int triPointIdx = 0;
                    for (int i = 0; i < geom.TriangleCount; i++)
                    {
                        indices[triPointIdx++] = geom.Triangles[i].A;
                        indices[triPointIdx++] = geom.Triangles[i].B;
                        indices[triPointIdx++] = geom.Triangles[i].C;
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
                    int ibo = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(ushort) * indices.Length, indices, BufferUsageHint.StaticDraw);

                    // setup the vertex attribs
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 12, 0);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 12, (sizeof(float) * 3));
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(float) * 12, (sizeof(float) * 6));
                    GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, sizeof(float) * 12, (sizeof(float) * 8));

                    // get the mesh color from the material
                    Color color = geom.MaterialCount > 0 ? geom.Materials[0].Color : Color.White;

                    // get the texture name if there's a texture assigned
                    string texname = geom.MaterialCount > 0 && geom.Materials[0].IsTextured ? geom.Materials[0].TextureReference.ReferencedTextureName : string.Empty;

                    // add the render model to the list
                    _models.Add(new GPUModel(vbo, ibo, geom.VertexCount, geom.TriangleCount * 3, texname, color));
                }
            }

            Vector3 extentCenter = Vector3.Divide((extentMin + extentMax), 0.5f);

            //_camera.Forward = Vector3.Divide(extentMin, 2f);
            //_camera.Position = extentCenter;

            // everything's processed, the scene is ready to be rendered
            _sceneready = true;
        }

        public void DeleteScene()
        {
            if (!_sceneready)
                return;

            Console.WriteLine("deleting scene");
            EndBind();
            Console.WriteLine("unbind models");
            GL.BindTexture(TextureTarget.Texture2D, 0);
            Console.WriteLine("unbind textures");
            _currentTextureID = _currentGPUModelID = 0;

            for (int i = 0; i < _texLookup.Count; i++)
            {
                Console.WriteLine("deleting texture: " + i);
                GL.DeleteTexture(_texLookup.GetEnumerator().Current.Value);
                _texLookup.GetEnumerator().MoveNext();
            }

            for (int i = 0; i < _models.Count; i++)
            {
                Console.WriteLine("deleting model: " + i);

                if(_models[i].vbo != 0)
                    GL.DeleteBuffer(_models[i].vbo);

                if(_models[i].ibo != 0)
                    GL.DeleteBuffer(_models[i].ibo);
            }

            _models.Clear();
            _texLookup.Clear();
            _sceneready = false;
        }

        public void DisposeViewer()
        {
            Console.WriteLine("disposing");

            if (_sceneready)
                DeleteScene();

            if (_shaderProg != null)
                _shaderProg.Delete();

            _sceneready = false;
            _ready = false;
        }

        private void BeginBind(GPUModel data)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, data.vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, data.ibo);
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
            GL.DrawElements(BeginMode.Triangles, data.ic, DrawElementsType.UnsignedShort, 0);
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
            if (data.vbo != 0)
            {
                if (data.tid != string.Empty)
                {
                    int texIdx;
                    bool texPresent = _texLookup.TryGetValue(data.tid, out texIdx);

                    if (texPresent)
                        FullTexBind(texIdx);
                }

                _shaderProg.SetUniform("diffuseColor", data.color);

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
        }

        private void FullTexBind(int texture)
        {
            if (texture != 0)
            {
                if (texture != _currentTextureID)
                {
                    GL.BindTexture(TextureTarget.Texture2D, texture);
                    _currentTextureID = texture;
                }
            }
        }

        private void ModelViewerResize(object sender, EventArgs e)
        {
            if (!_ready)
                return;

            GL.Viewport(0, 0, _viewerCtrl.Width, _viewerCtrl.Height);
        }

        private void ModelViewerPaint(object sender, PaintEventArgs e)
        {
            _flo += 0.01f;

            if (!_ready || !_viewerCtrl.Visible)
                return;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (_sceneready)
            {
                _shaderProg.Bind();
                _camera.Bind(_shaderProg, _viewerCtrl);
                _transform = Matrix4.CreateTranslation(_tp) * Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(90), 0, 0)) * Matrix4.CreateScale(new Vector3(1.0f, 1.0f, 1.0f));
                _shaderProg.SetUniform("tran", _transform);

                if (_models.Count > 0)
                    for (int i = 0; i < _models.Count; i++)
                        FullBind(_models[i]);
            }

            _viewerCtrl.SwapBuffers();
        }

        private void ModelViewerKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (!_ready || !_sceneready)
                return;

            Vector3 oldpos = _camera.Position;

            bool doInvalidate = true;
            const float MOVE_SPEED = 7f;

            if (e.KeyChar == 'w')
            {
                _camera.Position += _camera.Forward * MOVE_SPEED; //tp = new Vector3(tp.X, tp.Y, tp.Z + 10f);
            }
            else if (e.KeyChar == 's')
            {
                _camera.Position -= _camera.Forward * MOVE_SPEED; //tp = new Vector3(tp.X, tp.Y, tp.Z - 10f);
            }
            else if (e.KeyChar == 'd')
            {
                _camera.Position -= Vector3.Cross(_camera.Forward, new Vector3(0, 1.0f, 0)) * MOVE_SPEED; //tp = new Vector3(tp.X, tp.Y + 10f, tp.Z);
            }
            else if (e.KeyChar == 'a')
            {
                _camera.Position += Vector3.Cross(_camera.Forward, new Vector3(0, 1.0f, 0)) * MOVE_SPEED; // tp = new Vector3(tp.X, tp.Y - 10f, tp.Z);
            }
            else if (e.KeyChar == 'q')
            {
                _camera.Position += new Vector3(0, 1.0f, 0) * MOVE_SPEED; // tp = new Vector3(tp.X, tp.Y - 10f, tp.Z);
            }
            else if (e.KeyChar == 'e')
            {
                _camera.Position -= new Vector3(0, 1.0f, 0) * MOVE_SPEED; // tp = new Vector3(tp.X, tp.Y - 10f, tp.Z);
            }
            else
            {
                doInvalidate = false;
            }

            // invalidate only if an input was handled
            if (doInvalidate)
            {
                _viewerCtrl.Invalidate();
            }

            if (_camera.Position.IsNaN())
            {
                if (!oldpos.IsNaN())
                {
                    _camera.Position = oldpos; //prevents the camera from dying
                }
                else
                {
                    // shouldn't really happen
                    _camera.Position = new Vector3();
                }
            }

            _camera.Forward = Vector3.Normalize(_tp - _camera.Position);

            Console.WriteLine(_camera.Position);
        }
    }
}
