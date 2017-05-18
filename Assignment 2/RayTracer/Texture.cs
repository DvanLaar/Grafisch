using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template;

namespace RayTracer
{
    public class Texture
    {

        public int Width, Height;
        public Vector3[,] Data;

        public Texture(string path)
        {
            Bitmap map = new Bitmap(path);
            Width = map.Width;
            Height = map.Height;
            Data = new Vector3[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    Data[x, y] = ColorToV3(map.GetPixel(x, y));
                }
        }

        private Vector3 ColorToV3(Color c)
        {
            float r = (float)c.R / 255f;
            float g = (float)c.G / 255f;
            float b = (float)c.B / 255f;
            return new Vector3(r, g, b);
        }
    }
}
