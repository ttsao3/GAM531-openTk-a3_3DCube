using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace WindowOpenTK
{
    public class Shader
    {
        public int Handle { get; private set; }

        public Shader(string verPath, string fragPath)
        {
            //load and compile vertex shader
            var shaderSource = File.ReadAllText(verPath);
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, shaderSource);
            GL.CompileShader(vertexShader);
            CheckShaderCompile(vertexShader, "Vertex");

            //load and compile fragment shader
            shaderSource = File.ReadAllText(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            GL.CompileShader(fragmentShader);
            CheckShaderCompile(fragmentShader, "Fragment");

            //create shader program and link them together
            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                throw new Exception($"Program linking failed: {infoLog}");
            }

            //cleanup shaders
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            //Console.WriteLine($"Loaded vertex shader from {verPath}");
            //Console.WriteLine($"Loaded fragment shader from {fragPath}");
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public int GetUniformLocation(string name)
        {
            return GL.GetUniformLocation(Handle, name);
        }

        //Uniform Setters
        public void SetMatrix4(string name, Matrix4 data)
        {
            int loc = GetUniformLocation(name);
            GL.UniformMatrix4(loc, false, ref data);
        }

        public void SetVector3(string name, Vector3 data)
        {
            int loc = GetUniformLocation(name);
            GL.Uniform3(loc, data);
        }

        //check for errror
        private void CheckShaderCompile(int shader, string type)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"{type} shader compilation error:\n{infoLog}");
            }
        }

        //cleanup
        public void Cleanup()
        {
            GL.DeleteProgram(Handle);
        }
    }
}
