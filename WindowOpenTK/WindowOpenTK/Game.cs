using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace WindowOpenTK
{
    public class Game : GameWindow
    {
        private GameWindow _window;
        private int vertexBufferHandle;
        private int shaderProgramHandle;
        private int vertexArrayHandle;
        private int indexBufferHandle;
        private int _texture;

        private float rotationAngles = 0.0f;

        // Default wrapping and filtering
        private TextureWrapMode wrapMode = TextureWrapMode.Repeat;
        private TextureMinFilter minFilter = TextureMinFilter.LinearMipmapLinear;
        private TextureMagFilter magFilter = TextureMagFilter.Linear;

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

            //vertex data for a cube (position and texture coordinates)
            float[] vertices = new float[]
            {
                // Front face
                -0.5f, -0.5f,  0.5f,  0f, 0f,
                 0.5f, -0.5f,  0.5f,  1f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 1f,
                -0.5f,  0.5f,  0.5f,  0f, 1f,


                // Back face
                -0.5f, -0.5f, -0.5f,  1f, 0f,
                 0.5f, -0.5f, -0.5f,  0f, 0f,
                 0.5f,  0.5f, -0.5f,  0f, 1f,
                -0.5f,  0.5f, -0.5f,  1f, 1f,

                // Left face
                -0.5f, -0.5f, -0.5f,  0f, 0f,
                -0.5f, -0.5f,  0.5f,  1f, 0f,
                -0.5f,  0.5f,  0.5f,  1f, 1f,
                -0.5f,  0.5f, -0.5f,  0f, 1f,

                // Right face
                 0.5f, -0.5f, -0.5f,  1f, 0f,
                 0.5f, -0.5f,  0.5f,  0f, 0f,
                 0.5f,  0.5f,  0.5f,  0f, 1f,
                 0.5f,  0.5f, -0.5f,  1f, 1f,


                // Top face
                -0.5f,  0.5f, -0.5f,  0f, 0f,
                 0.5f,  0.5f, -0.5f,  1f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 1f,
                -0.5f,  0.5f,  0.5f,  0f, 1f,


                // Bottom face
                -0.5f, -0.5f, -0.5f,  0f, 1f,
                 0.5f, -0.5f, -0.5f,  1f, 1f,
                 0.5f, -0.5f,  0.5f,  1f, 0f,
                -0.5f, -0.5f,  0.5f,  0f, 0f,
            };

            //index data to share vertices
            uint[] indices = new uint[]
            {
                // Front face
                0, 1, 2,
                2, 3, 0,
                // Back face
                4, 5, 6,
                6, 7, 4,
                // Left face
                8, 9,10,
               10,11, 8,
                // Right face
               12,13,14,
               14,15,12,
                // Top face
               16,17,18,
               18,19,16,
                // Bottom face
               20,21,22,
               22,23,20
            };

            // generate a vertex buffer object (VBO) to store vertex data on GPU
            vertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // generate a element buffer object (EBO) to store index data on GPU
            indexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            // generate a vertex array object (VAO) to store the VBO configuration
            vertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayHandle);

            // bind the VBO and define the layout of vertex data for sharders
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0); //enable position attribute
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1); //enable color attribute

            // bind the EBO
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferHandle);

            GL.BindVertexArray(0);

            // vertext sharder : position of each vertex
            string vertexShaderCode =
                @"
                    #version 330 core
                    layout(location=0) in vec3 aPosition; //vertex position input
                    layout(location=1) in vec2 aTexCoord; //vertex color input
                    out vec2 TexCoord; //output color to fragment shader
                    uniform mat4 uMVP;
                    void main()
                    {
                        gl_Position = uMVP * vec4(aPosition, 1.0); //convert vec3 to vec4 for output
                        TexCoord = aTexCoord; //pass color to fragment shader
                    }
                ";

            //fragment shader : outputs a single color
            string fragmentShaderCode =
                @"
                    #version 330 core
                    in vec2 TexCoord; //input color from vertex shader
                    out vec4 fragColor; //output color  
                    uniform sampler2D ourTexture;
                    void main()
                    {
                        fragColor = texture(ourTexture, TexCoord);
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
            CheckProgram(shaderProgramHandle);

            // Cleanup shaders after linking (no longer needed individually)
            GL.DetachShader(shaderProgramHandle, vertexShaderHandle);
            GL.DetachShader(shaderProgramHandle, fragmentShaderHandle);
            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);

            GL.UseProgram(shaderProgramHandle);
            int texLoc = GL.GetUniformLocation(shaderProgramHandle, "ourTexture");
            GL.Uniform1(texLoc, 0); //set to texture unit 0
            GL.UseProgram(0);

            // Load texture
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string texturePath = Path.Combine(baseDir, "Assets", "box.jpg");
            //string texturePath = @"D:\GAM531\w5_a5\WindowOpenTK\WindowOpenTK\Assets\box.jpg";
            _texture = LoadTexture(texturePath);
        }

        protected override void OnUnload()
        {
            //delete all the buffers and shader programs
            GL.DeleteBuffer(vertexBufferHandle);
            GL.DeleteBuffer(indexBufferHandle);
            GL.DeleteVertexArray(vertexArrayHandle);
            GL.DeleteProgram(shaderProgramHandle);
            GL.DeleteTexture(_texture);

            base.OnUnload();

        }

        //called every from to update game logic, phisucs, or input handling
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            rotationAngles += (float)args.Time * 0.5f; //rotate 2 radians per second

            // Handle input
            if (this.IsKeyDown(Keys.Escape)) this.Close();

            // Switch wrapping modes (right now has no affects due to texture coords)
            if (this.IsKeyPressed(Keys.D1)) SetWrapMode(TextureWrapMode.Repeat);
            if (this.IsKeyPressed(Keys.D2)) SetWrapMode(TextureWrapMode.MirroredRepeat);
            if (this.IsKeyPressed(Keys.D3)) SetWrapMode(TextureWrapMode.ClampToEdge);
            if (this.IsKeyPressed(Keys.D4)) SetWrapMode(TextureWrapMode.ClampToBorder);

            // Switch filtering modes
            if (this.IsKeyPressed(Keys.F1)) SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            if (this.IsKeyPressed(Keys.F2)) SetFilter(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
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

            //bind the texture
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);

            //bind the VAO and draw the cube
            GL.BindVertexArray(vertexArrayHandle);
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);

            //display the rendered frame
            SwapBuffers();
        }

        private void CheckProgram(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0) throw new Exception(GL.GetProgramInfoLog(program));
        }

        private int LoadTexture(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException($"Could not find texture file: {path}");

            int texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texId);

            // Initial wrap and filter
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);

            using (Bitmap bmp = new Bitmap(path))
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                var data = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb); // fully qualified

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                              OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bmp.UnlockBits(data);
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            return texId;
        }

        // Update wrap mode live
        private void SetWrapMode(TextureWrapMode mode)
        {
            wrapMode = mode;
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapMode);
        }

        // Update filter mode live
        private void SetFilter(TextureMinFilter min, TextureMagFilter mag)
        {
            minFilter = min;
            magFilter = mag;
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
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
