using System;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL4;


namespace WindowOpenTK
{
    public class Game : GameWindow
    {
        private Mesh _cubeMesh;
        private Texture[] _cubeTextures = new Texture[3];
        private Texture _roomTexture, _floorTexture;

        private Vector3[] _cubePos = new Vector3[]
        {
            new Vector3(-6f, -4.3f, 0f),
            new Vector3(0f, 0.5f, -2f),
            new Vector3(6f, 2f, 3f)
        };

        private Vector3[] lightPos = new Vector3[3];

        private Vector3 lightColor = new Vector3(1.0f, 1.0f, 1.0f);

        private Shader _shader;
        private Camera _camera;

        private bool _firstMove = true;
        private Vector2 _lastPos;

        private float deltaTime = 0.0f;
        private float sensitivity = 0.1f;

        private bool[] _lightOn = { true, true, true };

        //add camera limit for room
        private const float X_limit = 9.5f;
        private const float Y_limit = 3.5f;
        private const float Z_limit = 9.5f;

        // constructor for the game class
        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        // Called once when the game starts, ideal for loading resources
        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(new Color4(0.1f, 0.1f, 0.1f, 1f));
            GL.Enable(EnableCap.DepthTest);

            //36 vertices to have 12 triangle for a 3D cube (2 triangles per face * 6 faces with * 3 vertices per triangle)
            float[] _vertices =
            {
                //2 triangles each 
                // Front face
                -0.5f, -0.5f,  0.5f,  0f, 0f, 1f, 0f,0f,
                 0.5f, -0.5f,  0.5f,  0f, 0f, 1f, 1f,0f,
                 0.5f,  0.5f,  0.5f,  0f, 0f, 1f, 1f,1f,

                 0.5f,  0.5f,  0.5f,  0f, 0f, 1f, 1f,1f,
                -0.5f,  0.5f,  0.5f,  0f, 0f, 1f, 0f,1f,
                -0.5f, -0.5f,  0.5f,  0f, 0f, 1f, 0f,0f,

                // Back face
                -0.5f, -0.5f, -0.5f,  0f, 0f, -1f, 1f,0f,
                -0.5f,  0.5f, -0.5f,  0f, 0f, -1f, 1f,1f,
                 0.5f,  0.5f, -0.5f,  0f, 0f, -1f, 0f,1f,

                 0.5f,  0.5f, -0.5f,  0f, 0f, -1f, 0f,1f,
                 0.5f, -0.5f, -0.5f,  0f, 0f, -1f, 0f,0f,
                -0.5f, -0.5f, -0.5f,  0f, 0f, -1f, 1f,0f,

                // Left face
                -0.5f,  0.5f, -0.5f,  -1f, 0f, 0f, 0f,1f,
                -0.5f,  0.5f,  0.5f,  -1f, 0f, 0f, 1f,1f,
                -0.5f, -0.5f,  0.5f,  -1f, 0f, 0f, 1f,0f,

                -0.5f, -0.5f,  0.5f,  -1f, 0f, 0f, 1f,0f,
                -0.5f, -0.5f, -0.5f,  -1f, 0f, 0f, 0f,0f,
                -0.5f,  0.5f, -0.5f,  -1f, 0f, 0f, 0f,1f,

                // Right face
                 0.5f,  0.5f, -0.5f,  1f, 0f, 0f, 1f,1f,
                 0.5f, -0.5f, -0.5f,  1f, 0f, 0f, 1f,0f,
                 0.5f, -0.5f,  0.5f,  1f, 0f, 0f, 0f,0f,

                 0.5f, -0.5f,  0.5f,  1f, 0f, 0f, 0f,0f,
                 0.5f,  0.5f,  0.5f,  1f, 0f, 0f, 0f,1f,
                 0.5f,  0.5f, -0.5f,  1f, 0f, 0f, 1f,1f,

                // Top face
                -0.5f,  0.5f, -0.5f,  0f, 1f, 0f, 0f,0f,
                 0.5f,  0.5f, -0.5f,  0f, 1f, 0f, 1f,0f,
                 0.5f,  0.5f,  0.5f,  0f, 1f, 0f, 1f,1f,

                 0.5f,  0.5f,  0.5f,  0f, 1f, 0f, 1f,1f,
                -0.5f,  0.5f,  0.5f,  0f, 1f, 0f, 0f,1f,
                -0.5f,  0.5f, -0.5f,  0f, 1f, 0f, 0f,0f,

                // Bottom face
                -0.5f, -0.5f, -0.5f,  0f, -1f, 0f, 1f,1f,
                -0.5f, -0.5f,  0.5f,  0f, -1f, 0f, 1f,0f,
                 0.5f, -0.5f,  0.5f,  0f, -1f, 0f, 0f,0f,

                 0.5f, -0.5f,  0.5f,  0f, -1f, 0f, 0f,0f,
                 0.5f, -0.5f, -0.5f,  0f, -1f, 0f, 0f,1f,
                -0.5f, -0.5f, -0.5f,  0f, -1f, 0f, 1f,1f,
            };

            //create our mesh and get texture from assets
            _cubeMesh = new Mesh(_vertices);

            _cubeTextures[0] = Texture.LoadFromFile("Assets/box.jpg");
            _cubeTextures[1] = Texture.LoadFromFile("Assets/crystal.png");
            _cubeTextures[2] = Texture.LoadFromFile("Assets/gold.jpg");
            _roomTexture = Texture.LoadFromFile("Assets/wall.jpg");
            _floorTexture = Texture.LoadFromFile("Assets/floor.jpg");

            //setup shader
            _shader = new Shader("Shaders/phong.vert", "Shaders/phong.frag");
            //cam setup
            _camera = new Camera(new Vector3(0.0f, 1.0f, 8.0f));
            //make sure mouse cursor invisible and captured so we can have proper FPS-camera movement
            CursorState = CursorState.Grabbed;

            //Console.WriteLine($"aPosition location: {vertexLocation}");
            //Console.WriteLine($"aNormal location: {normalLocation}");
        }

        //called when i need to update any game visuals
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            //clear the screen with background color
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //use our shader program
            _shader.Use();

            //create and set model, view, projection matrices
            Matrix4 projection = _camera.GetProjectionMatrix(Size.X, Size.Y);
            Matrix4 view = _camera.GetViewMatrix();

            _shader.SetMatrix4("view", view);
            _shader.SetMatrix4("projection", projection);

            //set light and view positions
            _shader.SetVector3("viewPos", _camera.Position);
            //_shader.SetVector3("objectColor", objectColor); // orange object
            _shader.SetVector3("lightColor", lightColor);//white light

            //draw room first
            GL.Disable(EnableCap.CullFace);//this makes it so we see the inside of faces
            Matrix4 roomModel = Matrix4.CreateScale(20f, 10f, 20f);
            _shader.SetMatrix4("model", roomModel);
            _roomTexture.Use(TextureUnit.Texture0);
            _cubeMesh.Draw(); // reuse same cube geometry (since room is also a cube)

            //draw floor
            Matrix4 floorModel = Matrix4.CreateScale(20f, 0.3f, 20f) * Matrix4.CreateTranslation(0.0f, -5f, 0.0f);
            _shader.SetMatrix4("model", floorModel);
            _floorTexture.Use(TextureUnit.Texture0);
            _cubeMesh.Draw(); // reuse same cube geometry (since room is also a cube)

            // setup model and light to each and draw
            for (int i = 0; i < 3; i++)
            {
                lightPos[i] = _cubePos[i] + new Vector3(0.0f, 2.0f, 0.0f);
                _shader.SetVector3($"lightPos[{i}]", lightPos[i]);
                GL.Uniform1(GL.GetUniformLocation(_shader.Handle, $"lightOn[{i}]"), _lightOn[i] ? 1 : 0);

                Matrix4 model = Matrix4.CreateTranslation(_cubePos[i]);
                _shader.SetMatrix4("model", model);

                _cubeTextures[i].Use(TextureUnit.Texture0);
                _cubeMesh.Draw();
            }

            GL.Enable(EnableCap.CullFace);
            //display the rendered frame
            SwapBuffers();
        }

        //called every from to update game logic, phisucs, or input handling
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            //camera movement with WASD with frame-rate independant speed
            var input = KeyboardState;
            deltaTime = (float)args.Time;
            float speed = 6.5f * deltaTime;

            if (input.IsKeyDown(Keys.W))
                _camera.Position += _camera.Front * speed;
            if (input.IsKeyDown(Keys.S))
                _camera.Position -= _camera.Front * speed;
            if (input.IsKeyDown(Keys.A))
                _camera.Position -= _camera.Right * speed;
            if (input.IsKeyDown(Keys.D))
                _camera.Position += _camera.Right * speed;
            if (input.IsKeyDown(Keys.E))
                _camera.Position += _camera.Up * speed;
            if (input.IsKeyDown(Keys.Q))
                _camera.Position -= _camera.Up * speed;
            if (MouseState.IsButtonPressed(MouseButton.Left))
                _lightOn[0] = !_lightOn[0];
            if (MouseState.IsButtonPressed(MouseButton.Right))
                _lightOn[1] = !_lightOn[1];
            if (MouseState.IsButtonPressed(MouseButton.Middle))
                _lightOn[2] = !_lightOn[2];
            if (input.IsKeyDown(Keys.Escape))
                Close();

            // Prevent camera from leaving the room
            _camera.Position = new Vector3(
                MathHelper.Clamp(_camera.Position.X, -X_limit, X_limit),
                MathHelper.Clamp(_camera.Position.Y, -Y_limit, Y_limit),
                MathHelper.Clamp(_camera.Position.Z, -Z_limit, Z_limit)
            );
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            if (!IsFocused) { return; }

            if (_firstMove)
            {
                _lastPos = new Vector2(e.X, e.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = e.X - _lastPos.X;
                var deltaY = e.Y - _lastPos.Y;
                _lastPos = new Vector2(e.X, e.Y);

                _camera.ProcessMouseMovement(deltaX, deltaY, sensitivity);
            }
        }

        //OnMouseWheel to control camera fov
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            _camera.Fov -= e.Offset.Y * 5.0f;
            _camera.Fov = MathHelper.Clamp(_camera.Fov, 30.0f, 90.0f);
        }
  
        // Called automatically whenever the window is resized
        protected override void OnResize(ResizeEventArgs e)
        {
            // Update the OpenGL viewport to match the new window dimensions
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        protected override void OnUnload()
        {
            //unbind and delete all the buffers and shader programs
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            _cubeMesh.Dispose();
            _shader.Cleanup();

            base.OnUnload();

        }
    }
}