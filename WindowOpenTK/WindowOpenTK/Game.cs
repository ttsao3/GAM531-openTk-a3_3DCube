using System;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace WindowOpenTK
{
    public class Game : GameWindow
    {
        private int vertexBufferHandle;
        private int shaderProgramHandle;
        private int vertexArrayHandle;
        //private int indexBufferHandle;
        //private float rotationAngles = 0.0f;

        private int modelLoc;
        private int viewLoc;
        private int projectionLoc;
        private int lightPosLoc;
        private int viewPosLoc;
        private int lightColorLoc;
        private int objectColorLoc;

        private float deltaTime = 0.0f;

        private Vector3 lightPos = new Vector3(1.2f, 0.0f, 2.0f); // Position of the light source
        private Vector3 lightColor = new Vector3(1.0f, 1.0f, 1.0f); // White light
        private Vector3 objectColor = new Vector3(0.5f, 0.5f, 0.5f); // Grey object color
        private Vector3 cameraPos = new Vector3(0.0f, 0.0f, 3.0f); // Camera position
        private Vector3 cameraFront = new Vector3(0.0f, 0.0f, -1.0f); // Camera front direction
        private Vector3 cameraUp = new Vector3(0.0f, 1.0f, 0.0f); // Camera up direction

        private float yaw = -90.0f; // Yaw is initialized to -90.0 degrees to look along the negative Z-axis
        private float pitch = 0.0f; // Pitch is initialized to 0.0 degrees
        float sensitivity = 0.1f; // Mouse sensitivity

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

            //load shaders from Shaders folder
            string vertexShaderCode = System.IO.File.ReadAllText("Shaders/phong.vert");
            string fragmentShaderCode = System.IO.File.ReadAllText("Shaders/phong.frag");

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

            // Get uniform locations
            modelLoc = GL.GetUniformLocation(shaderProgramHandle, "model");
            viewLoc = GL.GetUniformLocation(shaderProgramHandle, "view");
            projectionLoc = GL.GetUniformLocation(shaderProgramHandle, "projection");
            lightPosLoc = GL.GetUniformLocation(shaderProgramHandle, "lightPos");
            viewPosLoc = GL.GetUniformLocation(shaderProgramHandle, "viewPos");
            lightColorLoc = GL.GetUniformLocation(shaderProgramHandle, "lightColor");
            objectColorLoc = GL.GetUniformLocation(shaderProgramHandle, "objectColor");
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
            //rotationAngles += (float)args.Time * 2f; //rotate 2 radians per second
            var input = KeyboardState;
            deltaTime = (float)args.Time;
            float speed = 2.5f * deltaTime;

            if (input.IsKeyDown(Keys.W))
                cameraPos += speed * cameraFront;
            if (input.IsKeyDown(Keys.S))
                cameraPos -= speed * cameraFront;
            if (input.IsKeyDown(Keys.A))
                cameraPos -= Vector3.Normalize(Vector3.Cross(cameraFront, cameraUp)) * speed;
            if (input.IsKeyDown(Keys.D))
                cameraPos += Vector3.Normalize(Vector3.Cross(cameraFront, cameraUp)) * speed;
            if (input.IsKeyDown(Keys.Escape))
                Close();

        }

        // Handle mouse movement for camera control
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            MouseMoved(e.DeltaX, e.DeltaY);
        }
        // Update camera direction based on mouse movement
        private void MouseMoved(float xOffset, float yOffset)
        {
            yaw += xOffset *= sensitivity; // Adjust yaw based on mouse movement
            pitch -= yOffset *= sensitivity; // Invert y-axis for typical FPS camera control
            // Constrain the pitch to prevent screen flip
            if (pitch > 89.0f)
                pitch = 89.0f;
            if (pitch < -89.0f)
                pitch = -89.0f;
            // Update camera front vector
            Vector3 front;
            front.X = MathF.Cos(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));
            front.Z = MathF.Sin(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
            cameraFront = Vector3.Normalize(front);
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
            Matrix4 view = Matrix4.LookAt(cameraPos, cameraPos + cameraFront, cameraUp);

            // Model matrix (object placement in world)
            Matrix4 model = Matrix4.Identity;

            // Combine to create Model-View-Projection (MVP) matrix
            //Matrix4 mvp = model * view * projection;
            //int mvpLoc = GL.GetUniformLocation(shaderProgramHandle, "uMVP");
            //GL.UniformMatrix4(mvpLoc, false, ref mvp);

            // Set uniform values
            GL.UniformMatrix4(modelLoc, false, ref model);
            GL.UniformMatrix4(viewLoc, false, ref view);
            GL.UniformMatrix4(projectionLoc, false, ref projection);

            // Set light and view positions
            GL.Uniform3(lightPosLoc, ref lightPos);
            GL.Uniform3(viewPosLoc, ref cameraPos);
            GL.Uniform3(lightColorLoc, ref lightColor);
            GL.Uniform3(objectColorLoc, ref objectColor);

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