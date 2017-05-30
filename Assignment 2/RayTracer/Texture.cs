using OpenTK;
using System.Drawing;

namespace RayTracer
{
    /// <summary>
    /// A class which contains all the information needed about a texture.
    /// Supports only bitmaps.
    /// It contains all the pixels as HDR pixels.
    /// However, the colors are all in the 0f - 1f range when loaded from a file.
    /// </summary>
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
                    Data[x, y] = Utils.ColorToVector(map.GetPixel(x, y));
        }
    }
}
