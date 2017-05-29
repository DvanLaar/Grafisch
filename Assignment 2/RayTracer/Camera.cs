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
        // public Matrix4 Rotation;

        public Camera() {}

        public Ray GenerateRay(int x, int y)
        {
            Vector3 direction = -Vector3.UnitZ;
            direction += Vector3.UnitX * (x / 256f - 1f);
            direction += Vector3.UnitY * (1f - y / 256f);
            return new Ray(Position, direction.Normalized());
        }

        public Ray getDirection(int screenX, int screenY)
        {
            return GenerateRay(screenX, screenY);
        }

        public void processKeyboard(KeyboardState keyboard)
        {
            float speed = .1f;
            if (keyboard[Key.A]) this.Position -= speed * Vector3.UnitX;
            if (keyboard[Key.D]) this.Position += speed * Vector3.UnitX;
            if (keyboard[Key.S]) this.Position -= speed * Vector3.UnitY;
            if (keyboard[Key.W]) this.Position += speed * Vector3.UnitY;
            if (keyboard[Key.Q]) this.Position -= speed * Vector3.UnitZ;
            if (keyboard[Key.E]) this.Position += speed * Vector3.UnitZ;
        }
    }
}
