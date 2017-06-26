using OpenTK;
using System.Drawing;
using System.IO;

namespace rasterizer
{
    /// <summary>
    /// Is a mesh which can import a heightmap image to create a nice looking landscape, if normals are nicely handled.
    /// </summary>
    class HeightMap : Mesh
    {
        /// <summary>
        /// When set to true, we will use quads instead of triangles.
        /// However, it looks a bit more transparent in this case...
        /// </summary>
        private const bool MAKE_QUADS = false;

        public HeightMap(string fileName) : base()
        {
            // Load heightmap
            Bitmap bitmap;
            using (Stream imageStream = File.Open(fileName, FileMode.Open))
            {
                Image image = Image.FromStream(imageStream);
                bitmap = new Bitmap(image);
            }
            int h = bitmap.Height, w = bitmap.Width;
            Vector3[,] pos = new Vector3[h, w]; // all the positions
            Vector2[,] tex = new Vector2[h, w]; // all the texture coordinates

            // Read the height position from the image:
            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                {
                    Color col = bitmap.GetPixel(i, j);
                    // 0f < brightness < 1f
                    pos[i, j] = 2 * new Vector3(1f * i / (h - 1), .5f * col.GetBrightness() + .5f, 1f * j / (w - 1)) - Vector3.One;
                    // everything is evenly divided over the texture.
                    tex[i, j] = new Vector2(1f * i / (h - 1), 1f * j / (w - 1));
                }
            // Now we will make the mesh:
            if (MAKE_QUADS)
            {
                vertices = new ObjVertex[4 * (h - 1) * (w - 1)];
                triangles = new ObjTriangle[0];
                quads = new ObjQuad[(h - 1) * (w - 1)];
                for (int i = 0; i + 1 < h; i++)
                    for (int j = 0; j + 1 < w; j++)
                    {
                        int offset = 4 * ((w - 1) * i + j);
                        Vector3[] vv = new Vector3[] { pos[i, j], pos[i, j + 1], pos[i + 1, j], pos[i + 1, j + 1] };
                        Vector2[] uv = new Vector2[] { tex[i, j], tex[i, j + 1], tex[i + 1, j], tex[i + 1, j + 1] };

                        Vector3[] ts = new Vector3[] { Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero };
                        // calculate the tangents as an average over all the possible triangles to make with others of the quad.
                        for (int k = 0; k < 4; k++)
                        {
                            Vector3[] tinc = new Vector3[] { Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero };
                            MeshLoader.GetTangents(vv[(k + 1) % 4], vv[(k + 2) % 4], vv[(k + 3) % 4], uv[(k + 1) % 4], uv[(k + 2) % 4], uv[(k + 3) % 4], out tinc[1], out tinc[2], out tinc[3]);
                            for (int l = 1; l < 4; l++)
                            {
                                ts[(k + l) % 4] += tinc[l] / 3f;
                            }
                        }

                        for (int k = 0; k < 4; k++)
                            vertices[offset + k] = new ObjVertex(uv[k], ts[k], Vector3.UnitY, vv[k]);
                        quads[offset / 4] = new ObjQuad(offset + 0, offset + 1, offset + 2, offset + 3);
                    }
            }
            else
            {
                vertices = new ObjVertex[6 * (h - 1) * (w - 1)];
                triangles = new ObjTriangle[2 * (h - 1) * (w - 1)];
                quads = new ObjQuad[0];
                for (int i = 0; i + 1 < h; i++)
                    for (int j = 0; j + 1 < w; j++)
                        for (int k = 0; k < 2; k++)
                        {
                            Vector3 v0 = pos[i, j], v1 = pos[i + 1, j + 1], v2 = pos[i + k, j + 1 - k];
                            Vector2 uv0 = tex[i, j], uv1 = tex[i + 1, j + 1], uv2 = tex[i + k, j + 1 - k];

                            Vector3 t0, t1, t2;
                            MeshLoader.GetTangents(v0, v1, v2, uv0, uv1, uv2, out t0, out t1, out t2);

                            int offset = 6 * ((w - 1) * i + j) + 3 * k;
                            vertices[offset + 0] = new ObjVertex(uv0, t0, Vector3.UnitY, v0);
                            vertices[offset + 1] = new ObjVertex(uv1, t1, Vector3.UnitY, v1);
                            vertices[offset + 2] = new ObjVertex(uv2, t2, Vector3.UnitY, v2);
                            triangles[offset / 3] = new ObjTriangle(offset + 0, offset + 1, offset + 2);
                        }
            }
        }
    }
}
