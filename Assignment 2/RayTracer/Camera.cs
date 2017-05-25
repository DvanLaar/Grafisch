using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer
{
    class Camera
    {
        public const int resolution = 512;
        public const float depth = 1.0f;

        public Vector3 Position = new Vector3(0f, 1.0f, 0f);
        public Vector3 cornerTL = new Vector3(-1f, -1f, 1f), cornerTR = new Vector3(1f, -1f, 1f), cornerBL = new Vector3(-1f, 1f, 1f);
        public Quaternion Rotation = Quaternion.Identity;

        public Camera() { }

        private Ray GenerateRay(int x, int y)
        {
            Vector3 direction = -Vector3.UnitZ;

            direction += Vector3.UnitX * (x / 256f - 1f);
            direction += Vector3.UnitY * (1f - y / 256f);
            return new Ray(Position, Rotation * direction.Normalized());
        }

        public Ray getDirection(int screenX, int screenY)
        {
            return GenerateRay(screenX, screenY);
        }

        public void processKeyboard(KeyboardState keyboard)
        {
            float rotateSpeed = .02f * MathHelper.Pi;
            float moveSpeed = .1f;

            // Rotate left, right relative to the current rotation
            Vector3 dirX = Rotation * Vector3.UnitX;
            // Rotate up, down absolute
            Vector3 dirY = Vector3.UnitY;
            if (keyboard[Key.Left]) Rotation = Quaternion.FromAxisAngle(dirY, rotateSpeed) * Rotation;
            if (keyboard[Key.Right]) Rotation = Quaternion.FromAxisAngle(dirY, -rotateSpeed) * Rotation;
            if (keyboard[Key.Up]) Rotation = Quaternion.FromAxisAngle(dirX, rotateSpeed) * Rotation;
            if (keyboard[Key.Down]) Rotation = Quaternion.FromAxisAngle(dirX, -rotateSpeed) * Rotation;

            Vector3 delta = new Vector3();
            if (keyboard[Key.A]) delta -= Vector3.UnitX;
            if (keyboard[Key.D]) delta += Vector3.UnitX;
            if (keyboard[Key.W]) delta -= Vector3.UnitZ;
            if (keyboard[Key.S]) delta += Vector3.UnitZ;
            if (keyboard[Key.Q]) delta -= Vector3.UnitY;
            if (keyboard[Key.E]) delta += Vector3.UnitY;
            Position += Rotation * delta * moveSpeed;
        }
    }
}
