using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK;

namespace template_P3
{
    class Camera
    {
        const float Near = 0.1f, Far = 1000f;
        private static Vector3 Up = Vector3.UnitY;

        public Vector3 Position;
        private Quaternion _rotation;
        private Matrix4 _matrix;

        public Quaternion Rotation { get { return _rotation; } }
        public Matrix4 Matrix { get { return _matrix; } }

        public Camera(Vector3 position)
        {
            Position = position;
            _rotation = Quaternion.Identity;
            UpdateMatrix();
        }

        #region Translation
        public void Translate(Vector3 translation)
        {
            Position += translation;
            UpdateMatrix();
        }

        public void UpdatePosition(Vector3 position)
        {
            Position = position;
            UpdateMatrix();
        }

        public void Move(Vector3 translation)
        {
            if (translation == Vector3.Zero) return;

            Position += Rotation * translation;

            UpdateMatrix();
        }

        public void AddRotation(Vector2 rotation)
        {
            if (rotation == Vector2.Zero) return;

            Quaternion deltaY = Quaternion.FromAxisAngle(Vector3.UnitY, rotation.X);
            Vector3 dirX = deltaY * _rotation * Vector3.UnitX;
            Quaternion deltaX = Quaternion.FromAxisAngle(dirX, rotation.Y);
            _rotation = deltaX * deltaY * _rotation;

            UpdateMatrix();
        }

        public void AddTransformation(Vector2 rotation, Vector3 translation)
        {
            if (rotation != Vector2.Zero)
            {
                Console.WriteLine("Add rotation " + rotation.ToString());
                Quaternion deltaY = Quaternion.FromAxisAngle(Vector3.UnitY, rotation.X);
                Vector3 dirX = deltaY * _rotation * Vector3.UnitX;
                Quaternion deltaX = Quaternion.FromAxisAngle(dirX, rotation.Y);
                Console.WriteLine("Before: " + _rotation);
                _rotation = deltaX * deltaY * _rotation;
                Console.WriteLine("After: " + _rotation);
                Console.WriteLine("Add rotation " + deltaX + " ; " + deltaY);

                if (translation == Vector3.Zero) UpdateMatrix();
            }
            if (translation != Vector3.Zero)
            {
                Position += Rotation * translation;
                UpdateMatrix();
            }
        }
        #endregion

        private void UpdateMatrix()
        {
            Vector3 lookat = _rotation * -Vector3.UnitZ;
            _matrix = Matrix4.LookAt(Position, Position + lookat, Up);
            _matrix = _matrix * Matrix4.CreatePerspectiveFieldOfView(1.2f, 1.3f, Near, Far);
        }
    }
}
