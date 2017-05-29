using OpenTK;
using System.Drawing;
using System;
using System.Globalization;

namespace RayTracer
{
    class Utils
    {
        public static readonly Vector3 WHITE = new Vector3(1f, 1f, 1f);
        public static readonly Vector3 RED = new Vector3(1f, 0f, 0f);
        public static readonly Vector3 GREEN = new Vector3(0f, 1f, 0f);
        public static readonly Vector3 BLUE = new Vector3(0f, 0f, 1f);
        public const float DIST_EPS = 1e-5f; // used for small distances
        public const float SMALL_EPS = 1e-10f; // used for really small rounding errors and such

        public static float Parse(string value)
        {
            return float.Parse(value, CultureInfo.InvariantCulture.NumberFormat);
        }

        // Helper function to convert a HDR to an RGB-color
        public static int GetRGBValue(Vector3 v)
        {
            int r = MathHelper.Clamp((int)(v.X * 255f), 0, 255);
            int g = MathHelper.Clamp((int)(v.Y * 255f), 0, 255);
            int b = MathHelper.Clamp((int)(v.Z * 255f), 0, 255);
            return (r << 16) | (g << 8) | b;
        }

        // Helper function to convert C# Color to a HDR
        public static Vector3 ColorToVector(Color c)
        {
            return new Vector3(c.R / 255f, c.G / 255f, c.B / 255f);
        }

        /**
         * Returns an int in the range [0, ..., maxValue),
         * This assumes that f is a floating value between [0, 1) which needs to be scaled
         */
        public static int scaleFloat(float f, int maxValue)
        {
            // First round the scaling by adding 0.5
            return MathHelper.Clamp((int)(0.5f + f * maxValue), 0, maxValue);
        }

        public static double SafeAcos(float y)
        {
            // This avoids errors when y < -1 or y > 1 due to rounding errors
            if (y <= -1.0) return MathHelper.PiOver2;
            if (y >= 1.0) return 0;
            return Math.Acos(y);
        }
    }
}
