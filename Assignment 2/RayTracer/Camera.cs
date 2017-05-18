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

        public Vector3 Position = new Vector3(0, 0, 0);
        public Matrix4 Rotation;
        public Vector3 Direction = new Vector3(0, 0, 1);

        //Needs to be made dependent on camera and FOV
        public Vector3 screencorner_topleft = new Vector3(-1f, -1f, 0f) + new Vector3(0, 0, 1f);
        public Vector3 screencorner_topright = new Vector3(1f, -1f, 0f) + new Vector3(0, 0, 1f);
        public Vector3 screencorner_bottomleft = new Vector3(-1f, 1f, 0f) + new Vector3(0, 0, 1f);

        public Camera() {}

        private Ray GenerateRay(int x, int y)
        {
            Vector3 direction = (screencorner_topleft +
                                (x / 512f) * (screencorner_topright - screencorner_topleft) +
                                (y / 512f) * (screencorner_bottomleft - screencorner_topleft)) - this.Position;
            direction.Normalize();
            return new Ray(this.Position, direction);
        }

        public Ray getDirection(int screenX, int screenY)
        {
            return GenerateRay(screenX, screenY);
        }

        public void processKeyboard(KeyboardState keyboard)
        {

        }
    }
}
