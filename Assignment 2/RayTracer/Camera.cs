using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
            bool Increase();
            bool Decrease();
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
            Vector3 direction = new Vector3(FOV * (x / 256f - 1f), FOV * (1f - y / 256f), -1f);
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
                //while (listener.GetSpeedUp() > 4 && listener.Decrease())
                //{
                //    Thread.Sleep(500);
                //}
            }
            catch (ThreadInterruptedException) { }
        }

        // This contains all the keys which were pressed the last time
        private KeyboardState lkb;

        public void processKeyboard(KeyboardState kb)
        {
            bool hasMovement = false;

            float rotateSpeed = .02f * MathHelper.Pi;
            float moveSpeed = .1f;
            float fovSpeed = .05f;

            // Modify the speed up
            hasMovement |= kb[Key.R] || kb[Key.F] ;
            if (kb[Key.KeypadPlus] && !lkb[Key.KeypadPlus]) listener.Increase();
            if (kb[Key.KeypadMinus] && !lkb[Key.KeypadMinus]) listener.Decrease();
            if (kb[Key.Number1] && !lkb[Key.Number1]) listener.SetSpeedUp(1);
            if (kb[Key.Number2] && !lkb[Key.Number2]) listener.SetSpeedUp(2);
            if (kb[Key.Number3] && !lkb[Key.Number3]) listener.SetSpeedUp(4);
            if (kb[Key.Number4] && !lkb[Key.Number4]) listener.SetSpeedUp(8);
            if (kb[Key.Number5] && !lkb[Key.Number5]) listener.SetSpeedUp(16);
            if (kb[Key.Number6] && !lkb[Key.Number6]) listener.SetSpeedUp(32);
            if (kb[Key.Number7] && !lkb[Key.Number7]) listener.SetSpeedUp(64);
            if (kb[Key.Number8] && !lkb[Key.Number8]) listener.SetSpeedUp(128);

            // Rotate up, down absolute
            Vector3 dirY = Vector3.UnitY;
            hasMovement |= kb[Key.Left] || kb[Key.Right];
            if (kb[Key.Left]) Rotation = Quaternion.FromAxisAngle(dirY, rotateSpeed) * Rotation;
            if (kb[Key.Right]) Rotation = Quaternion.FromAxisAngle(dirY, -rotateSpeed) * Rotation;

            // Rotate left, right relative to the current rotation
            Vector3 dirX = Rotation * Vector3.UnitX;
            hasMovement |= kb[Key.Up] || kb[Key.Down];
            if (kb[Key.Up]) Rotation = Quaternion.FromAxisAngle(dirX, rotateSpeed) * Rotation;
            if (kb[Key.Down]) Rotation = Quaternion.FromAxisAngle(dirX, -rotateSpeed) * Rotation;

            Vector3 delta = new Vector3();
            hasMovement |= kb[Key.A] || kb[Key.D] || kb[Key.W] || kb[Key.S] || kb[Key.Q] || kb[Key.E];
            if (kb[Key.A]) delta -= Vector3.UnitX;
            if (kb[Key.D]) delta += Vector3.UnitX;
            if (kb[Key.W]) delta -= Vector3.UnitZ;
            if (kb[Key.S]) delta += Vector3.UnitZ;
            if (kb[Key.Q]) delta -= Vector3.UnitY;
            if (kb[Key.E]) delta += Vector3.UnitY;
            Position += Rotation * delta * moveSpeed;

            hasMovement |= kb[Key.PageUp] || kb[Key.PageDown];
            if (kb[Key.PageUp]) FOV += fovSpeed;
            if (kb[Key.PageDown]) FOV = Math.Max(FOV - fovSpeed, 0f);

            if (hasMovement)
            {
                if (oldValue >= 0)
                {
                    //listener.SetSpeedUp(oldValue);
                    oldValue = -1;
                }
                if (improver != null) improver.Interrupt();
                improver = new Thread(ImproveView);
                improver.Start();
            }
            lkb = kb;
        }
    }
}
