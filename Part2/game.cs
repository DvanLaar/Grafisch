using System;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Template
{

    class Game
    {
        // member variables
        public Surface screen;

        public int a = 0;
        private Surface map;
        // private float[,] h;
        // private float[,] colordif;
        private float[] vertexData;
        private int VBO;

        // initialize
        public void Init()
        {
            map = new Surface("heightmap.png");
            /*
            h = new float[128, 128];
            colordif = new float[128, 128];
            Random r = new Random();
            for (int y = 0; y < 128; y++)
                for (int x = 0; x < 128; x++)
                {
                    h[x, y] = ((float)(map.pixels[x + y * 128] & 255)) / 256;
                    colordif[x, y] = 0.5f * (float)r.NextDouble();
                }
            */

            float scale = 0.009f;
            float hscale = -0.2f;
            float xoffset = -(128 * scale) / 2f;
            float yoffset = -(128 * scale) / 2f;

            // there are 127 * 127 * 2 triangles
            // there are 3 vertices per triangle
            // there are 3 coordinates per vertex
            vertexData = new float[127 * 127 * 2 * 3 * 3];
            for (int i = 0; i < 127; i++)
            {
                for (int j = 0; j < 127; j++)
                {
                    // dit geeft aan welke vertex van het vierkant we willen:
                    int[] dx = { 0, 1, 0, 0, 1, 1 };
                    int[] dy = { 0, 0, 1, 1, 0, 1 };
                    for (int k = 0; k < 6; k++)
                    {
                        int x = i + dx[k], y = j + dy[k];

                        int inVertexData = ((i * 127 + j) * 6 + k) * 3;
                        vertexData[inVertexData + 0] = xoffset + x * scale; // x-coordinate
                        vertexData[inVertexData + 1] = yoffset + y * scale; // y-coordinate
                        vertexData[inVertexData + 2] = hscale * (map.pixels[x + y * 128] & 255) / 256; // z-coordinate
                    }
                }
            }
            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData<float>(
                BufferTarget.ArrayBuffer,
                (IntPtr)(vertexData.Length * 4),
                vertexData,
                BufferUsageHint.StaticDraw
            );
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(3, VertexPointerType.Float, 12, 0);
        }
        // tick: renders one frame
        public void Tick()
        {
            a++;
        }

        public void RenderGL()
        {

            var M = Matrix4.CreatePerspectiveFieldOfView(1.6f, 1.3f, 0.1f, 1000);
            GL.LoadMatrix(ref M);
            GL.Translate(0, 0, -1);
            GL.Rotate(110, 1, 0, 0);
            GL.Rotate((a / 100f) * 180 / Math.PI, 0, 0, 1);


            // GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.Color4(0.0, 1.0, 0.0, 0.5);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 127 * 127 * 2 * 3);

            /*
            GL.Begin(PrimitiveType.Quads);
            float scale = 0.009f;
            float hscale = -0.1f;
            float xoffset = -(128 * scale) / 2f;
            float yoffset = -(128 * scale) / 2f;

            for (int y = 0; y < 127; y++)
                for (int x = 0; x < 127; x++)
                {
                    GL.Color3(0, h[x, y] >= 0.015f ? 1f - colordif[x, y] : 0f, h[x, y] < 0.015f ? 1f - colordif[x, y] : 0f);
                    GL.Vertex3(xoffset + x * scale, xoffset + y * scale, hscale * h[x, y]);
                    GL.Vertex3(xoffset + scale + x * scale, xoffset + y * scale, hscale * h[x + 1, y]);
                    GL.Vertex3(xoffset + scale + x * scale, xoffset + scale + y * scale, hscale * h[x + 1, y + 1]);
                    GL.Vertex3(xoffset + x * scale, xoffset + scale + y * scale, hscale * h[x, y + 1]);
                }
            GL.End();
            */
        }
    }

} // namespace Template
