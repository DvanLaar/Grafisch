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
        public interface SpeedUpListener
        {
            int GetSpeedUp();
            void SetSpeedUp(int value);
            bool IncreaseSpeedUp();
            bool DecreaseSpeedUp();
            bool IncreaseAntiAliasing();
            bool DecreaseAntiAliasing();
        }

        private SpeedUpListener listener;
        private int oldValue = -1;

        public const int resolution = 512;
        public const float depth = 1.0f;

        public Vector3 Position = new Vector3(0f, 1.0f, 0f);
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

        public Camera(SpeedUpListener listener)
        {
            this.listener = listener;
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

        private Thread improver = null;

        private void ImproveView()
        {
            try
            {
                Thread.Sleep(500);
                oldValue = listener.GetSpeedUp();
                while (listener.GetSpeedUp() > 4 && listener.DecreaseSpeedUp())
                {
                    Thread.Sleep(500);
                }
            }
            catch (ThreadInterruptedException) { }
        }

        // This contains all the keys which were pressed the last time
        private KeyboardState lkb; //, noKeyPressed = new KeyboardState();

        public void processKeyboard(KeyboardState kb)
        {

            float rotateSpeed = .02f * MathHelper.Pi;
            float moveSpeed = .1f;
            float fovSpeed = .05f;

            // Modify the anti-aliasing
            if (kb[Key.R] && !lkb[Key.R]) listener.IncreaseAntiAliasing();
            if (kb[Key.F] && !lkb[Key.F]) listener.DecreaseAntiAliasing();

            // Modify the speed up
            if ((kb[Key.KeypadPlus] || kb[Key.Plus]) && !(lkb[Key.KeypadPlus] || lkb[Key.Plus])) listener.IncreaseSpeedUp();
            if ((kb[Key.KeypadMinus] || kb[Key.Minus]) && !(lkb[Key.KeypadMinus] || lkb[Key.Minus])) listener.DecreaseSpeedUp();

            int speedUp = 1;
            for (Key k = Key.Number1; k <= Key.Number9; k++, speedUp *= 2)
            {
                if (!kb[k] || lkb[k]) continue;
                listener.SetSpeedUp(speedUp);
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

            /*
            if (kb != noKeyPressed)
            {
                if (oldValue >= 0)
                {
                    // listener.SetSpeedUp(oldValue);
                    oldValue = -1;
                }
                if (improver != null) improver.Interrupt();
                improver = new Thread(ImproveView);
                improver.Start();
            }
            */
            lkb = kb;
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
                "PageUp, PageDown:    adjust FOV\n";
            Console.WriteLine(s);
        }
    }
}
