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

        

        private Vector3 lightPos = new Vector3(1.2f, 0.0f, 2.0f); // Position of the light source
        private Vector3 lightColor = new Vector3(1.0f, 1.0f, 1.0f); // White light
        private Vector3 objectColor = new Vector3(0.5f, 0.5f, 0.5f); // Grey object color

        //assignment 6
        private float deltaTime = 0.0f;

        private Vector3 cameraPos = new Vector3(0.0f, 0.0f, 3.0f); // Camera position
        private Vector3 cameraFront = new Vector3(0.0f, 0.0f, -1.0f); // Camera front direction
        private Vector3 cameraUp = new Vector3(0.0f, 1.0f, 0.0f); // Camera up direction

        private float yaw = -90.0f; // Yaw is initialized to -90.0 degrees to look along the negative Z-axis
        private float pitch = 0.0f; // Pitch is initialized to 0.0 degrees
        private float _fov = 45.0f;
        float sensitivity = 0.1f; // Mouse sensitivity
        private bool _firstMove = true;
        private Vector2 _lastPos;

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

            //--------Exercise 3----------
            //make sure mouse cursor invisible and captured so we can have proper FPS-camera movement
            CursorState = CursorState.Grabbed;
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

            //-------------Exercise 2-------------
            //camera movement with WASD with frame-rate independant speed
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
            if (input.IsKeyDown(Keys.E))
                cameraPos += cameraUp * speed;
            if (input.IsKeyDown(Keys.Q))
                cameraPos -= cameraUp * speed;
            if (input.IsKeyDown(Keys.Escape))
                Close();

        }

        //-------------Exercise 3------------
        // Handle mouse movement for camera control
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            if (!IsFocused) { return; }

            if (_firstMove) { 
                _lastPos = new Vector2(e.X, e.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = e.X - _lastPos.X;
                var deltaY = e.Y - _lastPos.Y;
                _lastPos = new Vector2(e.X, e.Y);


                yaw += deltaX * sensitivity; // Adjust yaw based on mouse movement
                pitch -= deltaY * sensitivity; // Invert y-axis for typical FPS camera control

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
                
        }

        //---------Exercise 4------------
        //OnMouseWheel to control camera fov
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            _fov -= e.Offset.Y * 5.0f;

            if (_fov < 30.0f) { _fov = 30.0f; }
            if (_fov > 90.0f) { _fov = 90.0f; }
        }

        //called when i need to update any game visuals
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            //clear the screen with background color
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //use our shader program
            GL.UseProgram(shaderProgramHandle);

            //----------Exercise 1------------
            //create model, view, projection matrices
            // Projection matrix (perspective)
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                //original from exercise 1: MathHelper.DegreesToRadians(45f),
                MathHelper.DegreesToRadians(_fov),
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