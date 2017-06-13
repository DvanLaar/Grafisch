using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK;

namespace template_P3
{

    // Inspired by and partially copied: http://neokabuto.blogspot.nl/2014/01/opentk-tutorial-5-basic-camera.html
    class Camera
    {

        public Vector3 Position;
        public Vector3 Orientation;

        public float Near = 0.1f;
        public float Far = 1000f;

        public Vector3 Up = Vector3.UnitY;

        public Matrix4 Matrix
        {
            get
            {
                return matrix;
            }
        }
        private Matrix4 matrix = Matrix4.Identity;

        public Camera(Vector3 position, Vector3 orientation)
        {
            this.Position = position;
            this.Orientation = orientation;
            updateMatrix();
        }

        #region Translation

        public void Translate(Vector3 translation)
        {
            Position += translation;
            updateMatrix();
        }

        public void UpdatePosition(Vector3 position)
        {
            this.Position = position;
            updateMatrix();
        }

        public void Move(Vector3 movement)
        {
            Vector3 offset = new Vector3();

            Vector3 forward = new Vector3((float)Math.Sin((float)Orientation.X), 0, (float)Math.Cos((float)Orientation.X));
            Vector3 right = new Vector3(-forward.Z, 0, forward.X);

            offset += movement.X * right;
            offset += movement.Z * forward;
            offset.Y += movement.Y;

            offset.NormalizeFast();
            offset = 0.5f * offset;

            Position += offset;
            updateMatrix();
        }

        public void AddRotation(Vector2 rotation)
        {
            Orientation.X = (Orientation.X + rotation.X) % ((float)Math.PI * 2.0f);
            Orientation.Y = Math.Max(Math.Min(Orientation.Y + rotation.Y, (float)Math.PI / 2.0f - 0.1f), (float)-Math.PI / 2.0f + 0.1f);
            updateMatrix();
        }

        #endregion


        private void updateMatrix()
        {
            matrix  = Matrix4.Identity;

            Vector3 lookat = new Vector3();

            lookat.X = (float)(Math.Sin((float)Orientation.X) * Math.Cos((float)Orientation.Y));
            lookat.Y = (float)Math.Sin((float)Orientation.Y);
            lookat.Z = (float)(Math.Cos((float)Orientation.X) * Math.Cos((float)Orientation.Y));

            matrix *= Matrix4.LookAt(Position, Position + lookat, Up);
            matrix *= Matrix4.CreatePerspectiveFieldOfView(1.2f, 1.3f, Near, Far);
        }

    }
}
