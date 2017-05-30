using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RayTracer
{
    // A class for the camera which shows the objects in the scene
    class Camera
    {
        // the raytracer with which we need to communicate
        private Raytracer rt;

        // The resolution of the 3D screen. We assume a 1/1 ratio of height/width
        public const int resolution = 512;

        // The location of the camera
        public Vector3 Position;

        /// The rotation applied to the camera to get this view.
        /// The default view of the camera is this:
        /// The camera looks in the -Vector3.UnitZ direction,
        /// the direction to the right is Vector3.UnitX
        /// the direction to the top is Vector3.UnitY
        public Quaternion Rotation = Quaternion.Identity;
        // The field of view: FOV is (in our program) the ratio between Z and X for the outer most pixels of the screen.
        public float FOV = 1f;

        /// This is in which the camera looks.
        /// If the rotation is the identity, this will be -Vector3.UnitZ
        public Vector3 Direction
        {
            get
            {
                return Rotation * new Vector3(0f, 0f, -1f);
            }
            set
            {
                // Sets the view direction to this 'value' but if we talk in pitch, yaw, roll,
                // the roll is kept at zero, which makes the needed rotation unique.

                // Updates Rotation such that direction == -Rotation * Vector3.UnitZ
                // First rotate over the y-axis to look in the good horizontal direction
                Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, (float)Math.Atan2(-value.X, -value.Z));
                // Adjust the rotation so that is only a direction in the YZ plane
                value = Rotation.Inverted() * value;
                // Now add a rotation over the x-axis in order to look at the desired rotation
                Rotation *= Quaternion.FromAxisAngle(Vector3.UnitX, (float)Math.Atan2(value.Y, -value.Z));
            }
        }

        public Camera(Raytracer rt, Vector3 position)
        {
            this.rt = rt;
            this.Position = position;
        }

        /// <summary>
        /// Returns a ray in which the camera looks for a certain pixel (x, y) of the screen.
        /// We use floats for anti-aliasing
        /// </summary>
        /// <param name="x">x position on the screen</param>
        /// <param name="y">y position on the screen</param>
        /// <returns>the direction in which the camera looks</returns>
        public Ray getDirection(float x, float y)
        {
            Vector3 direction = new Vector3(FOV * (x - 255.5f) / 256f, FOV * (255.5f - y) / 256f, -1f);
            // Subtract 255.5f in order to have the middle of the view between pixel 255 and 256,
            // since pixels range from 0 to 511 (inclusive).
            return new Ray(Position, Rotation * direction.Normalized());
        }

        /// <summary>
        /// This was as requested. You can give the angle between z and x direction to specify the FOV
        /// </summary>
        /// <param name="degrees"></param>
        public void setFOVAngles(float degrees)
        {
            FOV = (float)Math.Atan(MathHelper.DegreesToRadians(degrees));
        }

        // This contains all the keys which were pressed the last time that we rendered the screen.
        private KeyboardState lkb;
        // This contains the mouse position etc.
        private MouseState lm;

        public void processInput(KeyboardState kb, MouseDevice md)
        {
            MouseState m = md.GetState();

            // Regulates the speed of which key/mouse presses have influence on rotation, position or FOV of the camera.
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

            // Set the speed up to a specific value: number n -> SpeedUp = 2^{n-1}
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

            // Move the location of the camera
            Vector3 delta = Vector3.Zero;
            if (kb[Key.A]) delta -= Vector3.UnitX;
            if (kb[Key.D]) delta += Vector3.UnitX;
            if (kb[Key.W]) delta -= Vector3.UnitZ;
            if (kb[Key.S]) delta += Vector3.UnitZ;
            if (kb[Key.Q]) delta -= Vector3.UnitY;
            if (kb[Key.E]) delta += Vector3.UnitY;
            // Adjust to the basis of the camera
            Position += Rotation * delta * moveSpeed;

            // Adjust FOV
            if (kb[Key.PageUp]) FOV += fovSpeed;
            if (kb[Key.PageDown]) FOV = Math.Max(FOV - fovSpeed, 0f);

            // Let the camera point in a direction determined by the position of the click:
            if (m.IsButtonDown(MouseButton.Right) && !lm.IsButtonDown(MouseButton.Right) && 0 <= md.X && md.X < 512)
            {
                Ray r = getDirection(md.X, md.Y);
                // Call the setter:
                this.Direction = r.direction;
            }
            else if (m.IsButtonDown(MouseButton.Left) && lm.IsButtonDown(MouseButton.Left))
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

            // Set the state to the last state so we can compare with this state the next time:
            lkb = kb;
            lm = m;
        }

        /// <summary>
        /// Dislays the info of all commands in the console.
        /// </summary>
        public static void DisplayKeyInfo()
        {
            string s = "Behold, the great ray tracer!\n" +
                "First of all, may the light be with you...\n" +
                "This are the used keys:\n" +
                "KEYBOARD:\n" +
                "W, A, S, D, Q, E:    moves the camera\n" +
                "Arrow keys:          rotate the camera\n" +
                "R, F:                adjust antialiasing\n" +
                "1 - 8, -, +:         adjust speed up\n" +
                "PageUp, PageDown:    adjust FOV\n" +
                "\n" +
                "MOUSE:\n" +
                "Left button + movement: rotate the camera around\n" +
                "Right button + click:   center to a certain point\n" +
                "\n" +
                "DEBUG EXPLANATION:\n" +
                "Red lines: Primary Rays\n" +
                "Green lines: Secondary Rays\n" +
                "Blue lines: Shadow Rays\n" +
                "Only the sphere primitives are shown\n"
                ;
            Console.WriteLine(s);
        }
    }
}
