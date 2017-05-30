using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RayTracer
{
    class Camera
    {
        private Raytracer rt;
        private int oldValue = -1;

        public const int resolution = 512;
        public const float depth = 1.0f;

        public Vector3 Position;
        public Vector3 cornerTL = new Vector3(-1f, -1f, 1f), cornerTR = new Vector3(1f, -1f, 1f), cornerBL = new Vector3(-1f, 1f, 1f);
        public Quaternion Rotation = Quaternion.Identity;
        public float FOV = 1f;

        public Vector3 Direction
        {
            get
            {
                return Rotation * new Vector3(0f, 0f, -1f);
            }
        }

        public Camera(Raytracer rt, Vector3 position)
        {
            this.rt = rt;
            this.Position = position;
        }

        public Ray getDirection(float x, float y)
        {
            Vector3 direction = new Vector3(FOV * (x - 255.5f) / 256f, FOV * (255.5f - y) / 256f, -1f);
            return new Ray(Position, Rotation * direction.Normalized());
        }

        public void setFOVAngles(float degrees)
        {
            FOV = (float)Math.Atan(MathHelper.DegreesToRadians(degrees));
        }

        // private Thread improver = null;

        private void ImproveView()
        {
            try
            {
                Thread.Sleep(500);
                oldValue = rt.GetSpeedUp();
                while (rt.GetSpeedUp() > 4 && rt.DecreaseSpeedUp())
                {
                    Thread.Sleep(500);
                }
            }
            catch (ThreadInterruptedException) { }
        }

        // This contains all the keys which were pressed the last time
        private KeyboardState lkb;
        private MouseState lm;

        public void processInput(KeyboardState kb, MouseDevice md)
        {
            MouseState m = md.GetState();

            float rotateSpeed = .02f * MathHelper.Pi;
            float mouseRotateSpeed = 0.0005f * MathHelper.Pi;
            float moveSpeed = .1f;
            float fovSpeed = .05f;

            // Modify the anti-aliasing
            if (kb[Key.R] && !lkb[Key.R]) rt.IncreaseAntiAliasing();
            if (kb[Key.F] && !lkb[Key.F]) rt.DecreaseAntiAliasing();

            // Modify the speed up
            if ((kb[Key.KeypadPlus] || kb[Key.Plus]) && !(lkb[Key.KeypadPlus] || lkb[Key.Plus])) rt.IncreaseSpeedUp();
            if ((kb[Key.KeypadMinus] || kb[Key.Minus]) && !(lkb[Key.KeypadMinus] || lkb[Key.Minus])) rt.DecreaseSpeedUp();

            int speedUp = 1;
            for (Key k = Key.Number1; k <= Key.Number9; k++, speedUp *= 2)
            {
                if (!kb[k] || lkb[k]) continue;
                rt.SetSpeedUp(speedUp);
                break;
            }

            // Rotate up, down absolute
            Vector3 dirY = Vector3.UnitY;
            if (kb[Key.Left]) Rotation = Quaternion.FromAxisAngle(dirY, rotateSpeed) * Rotation;
            if (kb[Key.Right]) Rotation = Quaternion.FromAxisAngle(dirY, -rotateSpeed) * Rotation;

            // Rotate left, right relative to the current rotation
            Vector3 dirX = Rotation * Vector3.UnitX;
            if (kb[Key.Up]) Rotation = Quaternion.FromAxisAngle(dirX, rotateSpeed) * Rotation;
            if (kb[Key.Down]) Rotation = Quaternion.FromAxisAngle(dirX, -rotateSpeed) * Rotation;

            Vector3 delta = Vector3.Zero;
            if (kb[Key.A]) delta -= Vector3.UnitX;
            if (kb[Key.D]) delta += Vector3.UnitX;
            if (kb[Key.W]) delta -= Vector3.UnitZ;
            if (kb[Key.S]) delta += Vector3.UnitZ;
            if (kb[Key.Q]) delta -= Vector3.UnitY;
            if (kb[Key.E]) delta += Vector3.UnitY;
            Position += Rotation * delta * moveSpeed;

            if (kb[Key.PageUp]) FOV += fovSpeed;
            if (kb[Key.PageDown]) FOV = Math.Max(FOV - fovSpeed, 0f);
            
            if (m.IsButtonDown(MouseButton.Right)  && !lm.IsButtonDown(MouseButton.Right) && 0 <= md.X && md.X < 512) {
                Ray r = getDirection(md.X, md.Y);
                SetDirection(r.direction);
            } else if (m.IsButtonDown(MouseButton.Left))
            {
                // Rotate proportional to the movement of the mouse
                int dx = m.X - lm.X, dy = m.Y - lm.Y;
                // Rotate up, down absolute
                dirY = Vector3.UnitY;
                Rotation = Quaternion.FromAxisAngle(dirY, mouseRotateSpeed * dx) * Rotation;

                // Rotate left, right relative to the current rotation
                dirX = Rotation * Vector3.UnitX;
                Rotation = Quaternion.FromAxisAngle(dirX, mouseRotateSpeed * dy) * Rotation;
            }

            lkb = kb;
            lm = m;
        }

        /**
         * Updates Rotation such that direction == -Rotation * Vector3.UnitZ
         */
        public void SetDirection(Vector3 direction)
        {
            // First rotate over the y-axis to look in the good horizontal direction
            Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, (float) Math.Atan2(-direction.X, -direction.Z));
            // Adjust the rotation so that is only a direction in the YZ plane
            direction = Rotation.Inverted() * direction;
            // Now add a rotation over the x-axis in order to look at the desired rotation
            Rotation *= Quaternion.FromAxisAngle(Vector3.UnitX, (float)Math.Atan2(direction.Y, -direction.Z));
        }

        public static void DisplayKeyInfo()
        {
            string s = "Behold, the great ray tracer!\n" +
                "First of all, may the light be with you...\n" +
                "This are the used keys:\n" +
                "W, A, S, D, Q, E:    moves the camera\n" +
                "Arrow keys:          rotate the camera\n" +
                "R, F:                adjust antialiasing\n" +
                "1 - 8, -, +:         adjust speed up\n" +
                "PageUp, PageDown:    adjust FOV\n" +
                "\n"+
                "Debug:\n"+
                "Red lines: Primary Rays\n" +
                "Green lines: Secondary Rays\n" + 
                "Blue lines: Shadow Rays\n"+
                "Only the sphere primitives are shown\n"
                ;
            Console.WriteLine(s);
        }
    }
}
