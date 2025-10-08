using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace WindowOpenTK
{
    internal class Camera
    {
        public float pitch = 0f;
        public float yaw = -90f;
        private Vector3 cameraPos = new Vector3(0.0f, 0.0f, 3.0f); // Camera position
        private Vector3 cameraFront = new Vector3(0.0f, 0.0f, -1.0f); // Camera front direction
        private Vector3 cameraUp = new Vector3(0.0f, 1.0f, 0.0f); // Camera up direction
        private float sensitivity = 0.1f; // Mouse sensitivity
        private float zoom = 45.0f;
        private bool IsFirstPerson = true;

        public Matrix4 GetViewMatrix()
        {
            Vector3 front;
            front.X = MathF.Cos(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));
            front.Z = MathF.Sin(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));

            return Matrix4.LookAt(cameraPos, cameraPos + Vector3.Normalize(front), Vector3.UnitY);
        }

        public Matrix4 GetProjectionMatrix(float aspectRatio)
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(zoom), aspectRatio, 0.1f, 100f);
        }

        public void KeyPressed(float deltaTime, KeyboardState input)
        {
            float speed = 2.5f * deltaTime;

            if (input.IsKeyDown(Keys.W))
                cameraPos += speed * cameraFront;
            if (input.IsKeyDown(Keys.S))
                cameraPos -= speed * cameraFront;
            if (input.IsKeyDown(Keys.A))
                cameraPos -= Vector3.Normalize(Vector3.Cross(cameraFront, cameraUp)) * speed;
            if (input.IsKeyDown(Keys.D))
                cameraPos += Vector3.Normalize(Vector3.Cross(cameraFront, cameraUp)) * speed;
        }

        public void MouseMoved(float xOffset, float yOffset)
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

        public void AdjustSensitivity(float newSensitivity)
        {
            sensitivity = MathHelper.Clamp(newSensitivity, -0.1f, 0.5f);
        }

        public void Zoom(float offset)
        {
            zoom = MathHelper.Clamp(zoom - offset, 1.0f, 90.0f); // Zoom in/out with limits
        }

        public void TogglePersonView()
        {
            IsFirstPerson = !IsFirstPerson;
        }
    }
}
