using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.IO;
using System.Drawing;

namespace Amicitia.ModelViewer
{
    public class ShaderProgram
    {
        private Dictionary<string, int> _uniforms;
        private int _id;

        private static void Check(int shader, bool isProgram)
        {
            Console.WriteLine(isProgram ? GL.GetProgramInfoLog(shader) : GL.GetShaderInfoLog(shader));
        }

        public ShaderProgram(string name) // name of the shader minus the extension and path
        {
            // read the vertex and fragment shader text files
            string vSource = new StreamReader(Program.Assembly.GetManifestResourceStream("Amicitia." + name + ".vs")).ReadToEnd();
            string fSource = new StreamReader(Program.Assembly.GetManifestResourceStream("Amicitia." + name + ".fs")).ReadToEnd();

            // create vertex shader
            int vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, vSource);
            GL.CompileShader(vs);
            Check(vs, false);

            // create fragment shader
            int fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, fSource);
            GL.CompileShader(fs);
            Check(fs, false);

            // link the shaders together and bind the prog
            _id = GL.CreateProgram();
            GL.AttachShader(_id, vs);
            GL.AttachShader(_id, fs);
            GL.LinkProgram(_id);
            Check(fs, true);
            GL.ValidateProgram(_id);
            Check(fs, true);
            GL.BindFragDataLocation(_id, 0, "outColor");

            // we don't need these anymore
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);
            _uniforms = new Dictionary<string, int>();
        }

        public void AddUniform(string name)
        {
            _uniforms.Add(name, GL.GetUniformLocation(_id, name));
        }

        public void SetUniform(string name, Matrix4 mat)
        {
            GL.UniformMatrix4(_uniforms[name], false, ref mat);
        }

        public void SetUniform(string name, int b)
        {
            GL.Uniform1(_uniforms[name], b);
        }

        public void SetUniform(string name, Vector3 vec)
        {
            GL.Uniform3(_uniforms[name], ref vec);
        }
        public void SetUniform(string name, Vector4 vec)
        {
            GL.Uniform4(_uniforms[name], ref vec);
        }
        public void SetUniform(string name, Color col)
        {
            Vector4 v = new Vector4(col.R / 255.0f, col.G / 255.0f, col.B / 255.0f, col.A / 255.0f);
            GL.Uniform4(_uniforms[name], ref v);
        }
        public void SetUniform(string name, float f)
        {
            GL.Uniform1(_uniforms[name], f);
        }

        public void Bind()
        {
            GL.UseProgram(_id);
        }

        public void Delete()
        {
            GL.DeleteProgram(_id);
        }
    }
}
