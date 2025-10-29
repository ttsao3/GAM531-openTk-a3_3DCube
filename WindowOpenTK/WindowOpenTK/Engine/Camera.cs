using OpenTK.Mathematics;
using System;

namespace WindowOpenTK
{
    public class Camera
    {
        //cam directions
        public Vector3 _front = -Vector3.UnitZ;
        public Vector3 _up = Vector3.UnitY;
        public Vector3 _right = Vector3.UnitX;

        //rotations
        public float Yaw { get; set; } = -90f;
        public float Pitch { get; set; } = 0f;

        //filed of view
        public float Fov { get; set; } = 45f;

        public Camera(Vector3 position)
        {
            Position = position;
        }

        //position of cam
        public Vector3 Position { get; set; }

        public Vector3 Front => _front;
        public Vector3 Up => _up;
        public Vector3 Right => _right;

        // Get view matrix for shaders using LookAt
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + _front, _up);
        }

        // Get projection matrix for shaders
        public Matrix4 GetProjectionMatrix(float width, float height)
        {
            return Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(Fov),
                width / height,
                0.1f,
                100f);
        }

        public void ProcessMouseMovement(float deltaX, float deltaY, float sensitivity = 0.1f)
        {
            Yaw += deltaX * sensitivity;
            Pitch -= deltaY * sensitivity;

            if (Pitch > 89.0f) Pitch = 89.0f;
            if (Pitch < -89.0f) Pitch = -89.0f;

            UpdateVectors();
        }

        // Update recalculated camera direction vectors from pitch/yaw
        private void UpdateVectors()
        {
            _front.X = MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            _front.Y = MathF.Sin(MathHelper.DegreesToRadians(Pitch));
            _front.Z = MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            _front = Vector3.Normalize(_front);

            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }
}
