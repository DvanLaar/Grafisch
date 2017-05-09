using System;
using System.Diagnostics;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Input;

namespace Template
{

    class Game
    {
        // member variables
        public Surface screen;

        private float angle = 0.0f, lookAngle = 1.9f;
        private Surface map;
        // private float[,] h;
        // private float[,] colordif;
        private float[] vertexData, colorData;
        private int VBO;
        private int programID, vsID, fsID, attribute_vpos, attribute_vcol, uniform_mview, vbo_pos, vbo_col;
        private Stopwatch lastDraw;

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

            float scale = 1f / 128;
            float hscale = -0.2f;

            // there are 127 * 127 * 2 triangles
            // there are 3 vertices per triangle
            // there are 3 coordinates per vertex

            // initialise position and color VBO's:
            vertexData = new float[127 * 127 * 2 * 3 * 3];
            colorData = new float[127 * 127 * 2 * 3 * 3];

            Random r = new Random();

            Vector3[,] colors = new Vector3[128, 128];
            for (int i = 0; i < 128; i++)
                for (int j = 0; j < 128; j++)
                {
                    float height = 1f / 215 * (map.pixels[i + 128 * j] & 255);
                    float color = (float)(0.3 * r.NextDouble());
                    if (height >= 0.015)
                    {
                        colors[i, j] = new Vector3(0.4f, .9f - color, 0.1f);
                    }
                    else
                    {
                        colors[i, j] = new Vector3(.56f + color, .27f + color, .1f);
                    }
                }

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
                        float height = 1f / 215 * (map.pixels[x + 128 * y] & 255);

                        int inVertexData = ((i * 127 + j) * 6 + k) * 3;
                        vertexData[inVertexData + 0] = (x - 63.5f) * scale; // x-coordinate
                        vertexData[inVertexData + 1] = (y - 63.5f) * scale; // y-coordinate
                        vertexData[inVertexData + 2] = hscale * height; // z-coordinate

                        colorData[inVertexData + 0] = colors[x, y].X;
                        colorData[inVertexData + 1] = colors[x, y].Y;
                        colorData[inVertexData + 2] = colors[x, y].Z;
                    }
                }
            }
            /*
            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(vertexData.Length * 4), vertexData, BufferUsageHint.StaticDraw);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(3, VertexPointerType.Float, 12, 0);
            */

            // Load the shaders:
            programID = GL.CreateProgram();
            LoadShader("../../shaders/vs.glsl", ShaderType.VertexShader, programID, out vsID);
            LoadShader("../../shaders/fs.glsl", ShaderType.FragmentShader, programID, out fsID);
            GL.LinkProgram(programID);

            // Connect the shader variable names with our variables:
            attribute_vpos = GL.GetAttribLocation(programID, "vPosition");
            attribute_vcol = GL.GetAttribLocation(programID, "vColor");
            uniform_mview = GL.GetUniformLocation(programID, "M");

            // assign the color VBO to the pos attribute
            vbo_pos = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_pos);
            GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(vertexData.Length * 4), vertexData, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0);

            // assign the color VBO to the col attribute
            vbo_col = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_col);
            GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(colorData.Length * 4), colorData, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attribute_vcol, 3, VertexAttribPointerType.Float, false, 0, 0);
        }

        private void LoadShader(String name, ShaderType type, int program, out int ID)
        {
            ID = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(name))
                GL.ShaderSource(ID, sr.ReadToEnd());
            GL.CompileShader(ID);
            GL.AttachShader(program, ID);
            Console.WriteLine(GL.GetShaderInfoLog(ID));
        }

        private void KeyboardInput()
        {
            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard[Key.Up]) lookAngle += 0.01f;
            if (keyboard[Key.Down]) lookAngle -= 0.01f;
        }

        // tick: renders one frame
        public void Tick()
        {
            KeyboardInput();

            if (lastDraw != null)
            {
                lastDraw.Stop();
                angle += (float)(0.001f * lastDraw.Elapsed.TotalMilliseconds);
                lastDraw.Restart();
            }
            else lastDraw = new Stopwatch();

            Matrix4 M = Matrix4.CreateFromAxisAngle(new Vector3(0, 0, 1), angle);
            M *= Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), lookAngle);
            M *= Matrix4.CreateTranslation(0, 0, -1);
            M *= Matrix4.CreatePerspectiveFieldOfView(1.6f, 1.3f, .1f, 1000);

            GL.UseProgram(programID);
            GL.UniformMatrix4(uniform_mview, false, ref M);
        }

        public void RenderGL()
        {
            /*
            var M = Matrix4.CreatePerspectiveFieldOfView(1.6f, 1.3f, 0.1f, 1000);
            GL.LoadMatrix(ref M);
            GL.Translate(0, 0, -1);
            GL.Rotate(110, 1, 0, 0);
            GL.Rotate((a / 100f) * 180 / Math.PI, 0, 0, 1);
            */

            // GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            // GL.Color4(0.0, 1.0, 0.0, 0.5);

            // GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            // GL.DrawArrays(PrimitiveType.Triangles, 0, 127 * 127 * 2 * 3);

            GL.EnableVertexAttribArray(attribute_vpos);
            GL.EnableVertexAttribArray(attribute_vcol);
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
