using System;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using WindowOpenTK;

namespace WindowOpenTk
{
    public class Program : GameWindow
    {
        private Camera camera;
        private float deltaTime = 0.0f;
        private bool isFirstMove = true;
        private int vertexBufferHandle;
        private int vertexArrayHandle;
        private int shaderProgramHandle;
        private int modelLoc;
        private int viewLoc;
        private int projectionLoc;

        public Program(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            CenterWindow(new Vector2i(1280, 768));
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(new Color4(0f, 0f, 0f, 1f));
            GL.Enable(EnableCap.DepthTest);
            camera = new Camera();

            // Cube vertex data (position and normal)
            float[] vertices = new float[]
            {
                // Front face
                -0.5f, -0.5f,  0.5f,  0f, 0f, 1f,
                 0.5f, -0.5f,  0.5f,  0f, 0f, 1f,
                 0.5f,  0.5f,  0.5f,  0f, 0f, 1f,
                 0.5f,  0.5f,  0.5f,  0f, 0f, 1f,
                -0.5f,  0.5f,  0.5f,  0f, 0f, 1f,
                -0.5f, -0.5f,  0.5f,  0f, 0f, 1f,

                // Back face
                -0.5f, -0.5f, -0.5f,  0f, 0f, -1f,
                -0.5f,  0.5f, -0.5f,  0f, 0f, -1f,
                 0.5f,  0.5f, -0.5f,  0f, 0f, -1f,
                 0.5f,  0.5f, -0.5f,  0f, 0f, -1f,
                 0.5f, -0.5f, -0.5f,  0f, 0f, -1f,
                -0.5f, -0.5f, -0.5f,  0f, 0f, -1f,

                // Left face
                -0.5f,  0.5f, -0.5f,  -1f, 0f, 0f,
                -0.5f,  0.5f,  0.5f,  -1f, 0f, 0f,
                -0.5f, -0.5f,  0.5f,  -1f, 0f, 0f,
                -0.5f, -0.5f,  0.5f,  -1f, 0f, 0f,
                -0.5f, -0.5f, -0.5f,  -1f, 0f, 0f,
                -0.5f,  0.5f, -0.5f,  -1f, 0f, 0f,

                // Right face
                 0.5f,  0.5f, -0.5f,  1f, 0f, 0f,
                 0.5f, -0.5f, -0.5f,  1f, 0f, 0f,
                 0.5f, -0.5f,  0.5f,  1f, 0f, 0f,
                 0.5f, -0.5f,  0.5f,  1f, 0f, 0f,
                 0.5f,  0.5f,  0.5f,  1f, 0f, 0f,
                 0.5f,  0.5f, -0.5f,  1f, 0f, 0f,

                // Top face
                -0.5f,  0.5f, -0.5f,  0f, 1f, 0f,
                 0.5f,  0.5f, -0.5f,  0f, 1f, 0f,
                 0.5f,  0.5f,  0.5f,  0f, 1f, 0f,
                 0.5f,  0.5f,  0.5f,  0f, 1f, 0f,
                -0.5f,  0.5f,  0.5f,  0f, 1f, 0f,
                -0.5f,  0.5f, -0.5f,  0f, 1f, 0f,

                // Bottom face
                -0.5f, -0.5f, -0.5f,  0f, -1f, 0f,
                -0.5f, -0.5f,  0.5f,  0f, -1f, 0f,
                 0.5f, -0.5f,  0.5f,  0f, -1f, 0f,
                 0.5f, -0.5f,  0.5f,  0f, -1f, 0f,
                 0.5f, -0.5f, -0.5f,  0f, -1f, 0f,
                -0.5f, -0.5f, -0.5f,  0f, -1f, 0f,
            };

            // Create and configure VBO
            vertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Create and configure VAO
            vertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0); // Positions
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float)); // Normals
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            // Simple vertex and fragment shaders
            string vertexShaderSource = @"
                #version 330 core
                layout(location = 0) in vec3 aPos;
                layout(location = 1) in vec3 aNormal;
                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;
                void main()
                {
                    gl_Position = projection * view * model * vec4(aPos, 1.0);
                }
            ";

            string fragmentShaderSource = @"
                #version 330 core
                out vec4 FragColor;
                void main()
                {
                    FragColor = vec4(0.5, 0.5, 0.5, 1.0); // Grey color
                }
            ";

            // Compile shaders
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            // Link shader program
            shaderProgramHandle = GL.CreateProgram();
            GL.AttachShader(shaderProgramHandle, vertexShader);
            GL.AttachShader(shaderProgramHandle, fragmentShader);
            GL.LinkProgram(shaderProgramHandle);

            // Cleanup
            GL.DetachShader(shaderProgramHandle, vertexShader);
            GL.DetachShader(shaderProgramHandle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // Get uniform locations
            modelLoc = GL.GetUniformLocation(shaderProgramHandle, "model");
            viewLoc = GL.GetUniformLocation(shaderProgramHandle, "view");
            projectionLoc = GL.GetUniformLocation(shaderProgramHandle, "projection");
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            deltaTime = (float)args.Time;
            camera.KeyPressed(deltaTime, KeyboardState);
            if (KeyboardState.IsKeyDown(Keys.Escape)) Close();
            if (KeyboardState.IsKeyDown(Keys.Space)) camera.TogglePersonView();
            if (KeyboardState.IsKeyDown(Keys.F)) camera.AdjustSensitivity(0.5f);
            if (KeyboardState.IsKeyDown(Keys.G)) camera.AdjustSensitivity(-0.1f);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            if (isFirstMove)
            {
                isFirstMove = false;
                return;
            }
            camera.MouseMoved(e.DeltaX, e.DeltaY);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            camera.Zoom(e.OffsetY);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(shaderProgramHandle);
            Matrix4 model = Matrix4.Identity;
            Matrix4 view = camera.GetViewMatrix();
            Matrix4 projection = camera.GetProjectionMatrix((float)Size.X / Size.Y);

            GL.UniformMatrix4(modelLoc, false, ref model);
            GL.UniformMatrix4(viewLoc, false, ref view);
            GL.UniformMatrix4(projectionLoc, false, ref projection);

            GL.BindVertexArray(vertexArrayHandle);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.BindVertexArray(0);

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vertexBufferHandle);
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(vertexArrayHandle);
            GL.UseProgram(0);
            GL.DeleteProgram(shaderProgramHandle);
        }

        public static void Main(string[] args)
        {
            var gameWindowSettings = GameWindowSettings.Default;
            var nativeWindowSettings = NativeWindowSettings.Default;
            nativeWindowSettings.Title = "3D Camera Controller";

            using (var window = new Program(gameWindowSettings, nativeWindowSettings))
            {
                window.Run();
            }
        }
    }
}