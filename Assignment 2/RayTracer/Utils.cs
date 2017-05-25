using OpenTK;
using System.Drawing;
using System;

namespace RayTracer
{
    class Utils
    {
        public static readonly Vector3 WHITE = new Vector3(1f, 1f, 1f);

        public static int GetRGBValue(Vector3 v)
        {
            int r = MathHelper.Clamp((int)(v.X * 255f), 0, 255);
            int g = MathHelper.Clamp((int)(v.Y * 255f), 0, 255);
            int b = MathHelper.Clamp((int)(v.Z * 255f), 0, 255);
            return (r << 16) | (g << 8) | b;
        }

        public static Vector3 ColorToVector(Color c)
        {
            return new Vector3(c.R / 255f, c.G / 255f, c.B / 255f);
        }

        public static int scaleFloat(float f, int maxValue)
        {
            return MathHelper.Clamp((int)(f * maxValue), 0, maxValue);
        }

        internal static double SafeAcos(float y)
        {
            if (y <= -1.0) return MathHelper.PiOver2;
            if (y >= 1.0) return 0;
            return Math.Acos(y);
        }
    }
}
