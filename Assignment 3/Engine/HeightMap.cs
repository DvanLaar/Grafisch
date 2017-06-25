using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template_P3;

namespace template_P3
{
    class HeightMap : Mesh
    {
        private static bool makeQuads = false;

        public HeightMap(string fileName) : base()
        {
            Bitmap bmp;
            using (Stream bmpStream = File.Open(fileName, FileMode.Open))
            {
                Image image = Image.FromStream(bmpStream);
                bmp = new Bitmap(image);
            }
            int h = bmp.Height, w = bmp.Width;
            Vector3[,] pos = new Vector3[h, w];
            Vector2[,] tex = new Vector2[h, w];

            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                {
                    Color col = bmp.GetPixel(i, j);
                    pos[i, j] = new Vector3(1f * i / (h - 1), col.GetBrightness(), 1f * j / (w - 1));
                    pos[i, j] = 2 * pos[i, j] - Vector3.One;
                    tex[i, j] = new Vector2(1f * i / (h - 1), 1f * j / (w - 1));
                }
            if (makeQuads)
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
                        for (int k = 0; k < 4; k++)
                        {
                            Vector3[] tinc = new Vector3[] { Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero };
                            MeshLoader.getTangent(vv[(k + 1) % 4], vv[(k + 2) % 4], vv[(k + 3) % 4], uv[(k + 1) % 4], uv[(k + 2) % 4], uv[(k + 3) % 4], out tinc[1], out tinc[2], out tinc[3]);
                            for (int l = 1; l < 4; l++)
                            {
                                ts[(k + l) % 4] += tinc[l] / 3f;
                            }
                        }

                        for (int k = 0; k < 4; k++)
                            vertices[offset + k] = new ObjVertex(uv[k], ts[k], Vector3.UnitY, vv[k]);
                        quads[offset / 4] = new ObjQuad(offset, offset + 1, offset + 2, offset + 3);
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

                            // Overwrite the data, but now with a tangent
                            Vector3 t0 = ((uv2 - uv0).Y * (v1 - v0) - (uv1 - uv0).Y * (v2 - v0)).Normalized();
                            Vector3 t1 = ((uv0 - uv1).Y * (v2 - v1) - (uv2 - uv1).Y * (v0 - v1)).Normalized();
                            Vector3 t2 = ((uv1 - uv2).Y * (v0 - v2) - (uv0 - uv2).Y * (v1 - v2)).Normalized();

                            t0 = t1 = t2 = Vector3.UnitX;

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
