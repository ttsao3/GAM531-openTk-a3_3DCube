using System;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace WindowOpenTK
{
    public class Game : GameWindow
    {
        private int vertexBufferHandle;
        private int shaderProgramHandle;
        private int vertexArrayHandle;
        private int indexBufferHandle;

        private float rotationAngles = 0.0f;

        // constructor for the game class
        public Game()
              : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            //set window size to 1280x768
            this.CenterWindow(new Vector2i(1280, 768));

            // Center the window on the screen
            this.CenterWindow(this.Size);
        }

        // Called automatically whenever the window is resized
        protected override void OnResize(ResizeEventArgs e)
        {
            // Update the OpenGL viewport to match the new window dimensions
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        // Called once when the game starts, ideal for loading resources
        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(new Color4(0f, 0f, 0f, 1f));
            GL.Enable(EnableCap.DepthTest);

            //36 vertices to have 12 triangle for a 3D cube (2 triangles per face * 6 faces with * 3 vertices per triangle)
            float[] vertices = new float[]
            {
                //2 triangles each 
                // Front face
                -0.5f, -0.5f,  0.5f,  1f, 0f, 0f,
                 0.5f, -0.5f,  0.5f,  1f, 0f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 0f, 0f,

                 0.5f,  0.5f,  0.5f,  1f, 0f, 0f,
                -0.5f,  0.5f,  0.5f,  1f, 0f, 0f,
                -0.5f, -0.5f,  0.5f,  1f, 0f, 0f,

                // Back face
                -0.5f, -0.5f, -0.5f,  0f, 1f, 0f,
                -0.5f,  0.5f, -0.5f,  0f, 1f, 0f,
                 0.5f,  0.5f, -0.5f,  0f, 1f, 0f,

                 0.5f,  0.5f, -0.5f,  0f, 1f, 0f,
                 0.5f, -0.5f, -0.5f,  0f, 1f, 0f,
                -0.5f, -0.5f, -0.5f,  0f, 1f, 0f,

                // Left face
                -0.5f,  0.5f, -0.5f,  0f, 0f, 1f,
                -0.5f,  0.5f,  0.5f,  0f, 0f, 1f,
                -0.5f, -0.5f,  0.5f,  0f, 0f, 1f,

                -0.5f, -0.5f,  0.5f,  0f, 0f, 1f,
                -0.5f, -0.5f, -0.5f,  0f, 0f, 1f,
                -0.5f,  0.5f, -0.5f,  0f, 0f, 1f,

                // Right face
                 0.5f,  0.5f, -0.5f,  1f, 1f, 0f,
                 0.5f, -0.5f, -0.5f,  1f, 1f, 0f,
                 0.5f, -0.5f,  0.5f,  1f, 1f, 0f,

                 0.5f, -0.5f,  0.5f,  1f, 1f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 1f, 0f,
                 0.5f,  0.5f, -0.5f,  1f, 1f, 0f,

                // Top face
                -0.5f,  0.5f, -0.5f,  0f, 1f, 1f,
                 0.5f,  0.5f, -0.5f,  0f, 1f, 1f,
                 0.5f,  0.5f,  0.5f,  0f, 1f, 1f,

                 0.5f,  0.5f,  0.5f,  0f, 1f, 1f,
                -0.5f,  0.5f,  0.5f,  0f, 1f, 1f,
                -0.5f,  0.5f, -0.5f,  0f, 1f, 1f,

                // Bottom face
                -0.5f, -0.5f, -0.5f,  1f, 0f, 1f,
                -0.5f, -0.5f,  0.5f,  1f, 0f, 1f,
                 0.5f, -0.5f,  0.5f,  1f, 0f, 1f,

                 0.5f, -0.5f,  0.5f,  1f, 0f, 1f,
                 0.5f, -0.5f, -0.5f,  1f, 0f, 1f,
                -0.5f, -0.5f, -0.5f,  1f, 0f, 1f,

            };

            // generate a vertex buffer object (VBO) to store vertex data on GPU
            vertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // generate a vertex array object (VAO) to store the VBO configuration
            vertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayHandle);

            // bind the VBO and define the layout of vertex data for sharders
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            // vertext sharder : position of each vertex
            string vertexShaderCode =
                @"
                    #version 330 core
                    layout(location=0) in vec3 aPosition; //vertex position input
                    layout(location=1) in vec3 aColor; //vertex color input
                    out vec3 vColor; //output color to fragment shader
                    uniform mat4 uMVP;
                    void main()
                    {
                        gl_Position = uMVP * vec4(aPosition, 1.0); //convert vec3 to vec4 for output
                        vColor = aColor; //pass color to fragment shader
                    }
                ";

            //fragment shader : outputs a single color
            string fragmentShaderCode =
                @"
                    #version 330 core
                    in vec3 vColor; //input color from vertex shader
                    out vec4 fragColor; //output color  
                    void main()
                    {
                        fragColor = vec4(vColor, 1.0); //set output color with alpha 1.0
                    }
                ";

            //complier shaders
            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderCode);
            GL.CompileShader(vertexShaderHandle);
            CheckShaderCompile(vertexShaderHandle, "Vertex Shader");

            int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderCode);
            GL.CompileShader(fragmentShaderHandle);
            CheckShaderCompile(fragmentShaderHandle, "Fragment Shader");

            // Create shader program and link shaders
            shaderProgramHandle = GL.CreateProgram();
            GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
            GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);
            GL.LinkProgram(shaderProgramHandle);

            // Cleanup shaders after linking (no longer needed individually)
            GL.DetachShader(shaderProgramHandle, vertexShaderHandle);
            GL.DetachShader(shaderProgramHandle, fragmentShaderHandle);
            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);
        }

        protected override void OnUnload()
        {
            //unbind and delete all the buffers and shader programs
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vertexBufferHandle);
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(vertexArrayHandle);
            GL.UseProgram(0);
            GL.DeleteProgram(shaderProgramHandle);

            base.OnUnload();

        }

        //called every from to update game logic, phisucs, or input handling
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            rotationAngles += (float)args.Time * 2f; //rotate 2 radians per second
        }

        //called when i need to update any game visuals
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            //clear the screen with background color
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //use our shader program
            GL.UseProgram(shaderProgramHandle);

            //create model, view, projection matrices
            // Projection matrix (perspective)
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45f),
                (float)Size.X / Size.Y,
                0.1f,
                100f
            );

            // View matrix (camera placement to see cube)
            Matrix4 view = Matrix4.CreateTranslation(0f, 0f, -3f);

            // Model matrix (rotate cube over time)
            Matrix4 model = Matrix4.CreateRotationY(rotationAngles) * Matrix4.CreateRotationX(rotationAngles * 0.5f);

            // Combine to create Model-View-Projection (MVP) matrix
            Matrix4 mvp = model * view * projection;
            int mvpLoc = GL.GetUniformLocation(shaderProgramHandle, "uMVP");
            GL.UniformMatrix4(mvpLoc, false, ref mvp);

            //bind the VAO and draw the triangle
            GL.BindVertexArray(vertexArrayHandle);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.BindVertexArray(0);

            //display the rendered frame
            SwapBuffers();
        }

        // Helper function to check for shader compilation errors
        private void CheckShaderCompile(int shaderHandle, string shaderName)
        {
            GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shaderHandle);
                Console.WriteLine($"Error compiling {shaderName}: {infoLog}");
            }
        }
    }
}