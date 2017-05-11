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

        private float angle = 0.0f, lookAngle = (float) Math.Cos(.75f * Math.PI);
        private Surface map;
        private float[] vertexData, colorData, normalData;

        // Shader ID's:
        private int programID, vsID, fsID;

        // Variables for the shaders:
        private int attribute_vpos, attribute_vcol, attribute_vnorm;

        // Uniform variables for the shaders:
        private int uniform_mview, uniform_lpoint, uniform_lcolor;
        // VBO's: 
        private int vbo_pos, vbo_col, vbo_norm;

        /**
         * Holds the time of the last time that Tick is called.
         * This is used for time measurement
         */
        private Stopwatch lastDraw;

        /**
         * Location of the lightsource
         */
        private Vector3 LightPoint = new Vector3(0, 0, 1f);

        /**
         * The white color of the light
         */
        private Vector3 LightColor = new Vector3(1f, 1f, .9f);

        // initialize
        public void Init()
        {
            map = new Surface("heightmap.png");

            // To scale the island
            float scale = 1f / 128;
            float hscale = 0.2f;

            // there are 127 * 127 * 2 triangles
            // there are 3 vertices per triangle
            // there are 3 coordinates per vertex

            // initialise position and color VBO's:
            vertexData = new float[127 * 127 * 2 * 3 * 3];
            colorData = new float[127 * 127 * 2 * 3 * 3];
            normalData = new float[127 * 127 * 2 * 3 * 3];

            Random r = new Random();

            Vector3[,] colors = new Vector3[128, 128];
            float[,] height = new float[128, 128];
            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    height[i, j] = 1f / 256 * (map.pixels[i + 128 * j] & 255);
                    // add a bit of randomness to the colors to make the island more beautifull
                    float color = (float)(0.3 * r.NextDouble());
                    if (height[i, j] >= 0.015)
                    {
                        colors[i, j] = new Vector3(0.4f, .9f - color, 0.1f);
                    }
                    else
                    {
                        colors[i, j] = new Vector3(.56f + color, .27f + color, .1f);
                    }
                }
            }

            // Triangle normals
            // 1 think boundary around the real normals to make the normal "averaging" for the real triangles easier (this will only affect the boundary normals)
            Vector3[,] TriangleNormals1 = new Vector3[129, 129];
            Vector3[,] TriangleNormals2 = new Vector3[129, 129];

            for (int i = 0; i < 129; i++)
            {
                for (int j = 0; j < 129; j++)
                {
                    TriangleNormals1[i, j] = Vector3.UnitZ;
                    TriangleNormals2[i, j] = Vector3.UnitZ;
                }
            }

            for (int i = 0; i < 127; i++)
            {
                for (int j = 0; j < 127; j++)
                {
                    // The four corners of the quad
                    Vector3 v1 = new Vector3(i * scale, j * scale, hscale * height[i, j]);
                    Vector3 v2 = new Vector3(i * scale, (j + 1) * scale, hscale * height[i, j + 1]);
                    Vector3 v3 = new Vector3((i + 1) * scale, j * scale, hscale * height[i + 1, j]);
                    Vector3 v4 = new Vector3((i + 1) * scale, (j + 1) * scale, hscale * height[i + 1, j + 1]);
                    // Normal for the bottom-left and top-right triangles
                    TriangleNormals1[i + 1, j + 1] = Vector3.Cross(v3 - v1, v2 - v1).Normalized();
                    TriangleNormals2[i + 1, j + 1] = Vector3.Cross(v4 - v2, v4 - v3).Normalized();
                }
            }

            for (int i = 0; i < 127; i++)
            {
                for (int j = 0; j < 127; j++)
                {
                    // Which vertex is where in the square
                    int[] dx = { 0, 1, 0, 0, 1, 1 };
                    int[] dy = { 0, 0, 1, 1, 0, 1 };
                    for (int k = 0; k < 6; k++)
                    {
                        int x = i + dx[k], y = j + dy[k];
                        int idx = ((i * 127 + j) * 6 + k) * 3;
                        
                        // Positions
                        vertexData[idx + 0] = (x - 63.5f) * scale; // x-coordinate
                        vertexData[idx + 1] = (y - 63.5f) * scale; // y-coordinate
                        vertexData[idx + 2] = hscale * height[x, y]; // z-coordinate
                        
                        // Colors
                        colorData[idx + 0] = colors[x, y].X;
                        colorData[idx + 1] = colors[x, y].Y;
                        colorData[idx + 2] = colors[x, y].Z;

                        // Normals (averaging over the triangles the vertex is connected to)
                        Vector3 sum = new Vector3();
                        sum += TriangleNormals1[x + 1, y + 1];
                        sum += TriangleNormals2[x + 1, y + 1 - 1];
                        sum += TriangleNormals1[x + 1, y + 1 - 1];
                        sum += TriangleNormals2[x + 1 - 1, y + 1 - 1];
                        sum += TriangleNormals1[x + 1 - 1, y + 1];
                        sum += TriangleNormals2[x + 1 - 1, y + 1];
                        sum.Normalize();

                        normalData[idx + 0] = sum.X;
                        normalData[idx + 1] = sum.Y;
                        normalData[idx + 2] = sum.Z;
                    }
                }
            }

            // Load the shaders:
            programID = GL.CreateProgram();
            LoadShader("../../shaders/vs.glsl", ShaderType.VertexShader, programID, out vsID);
            LoadShader("../../shaders/fs.glsl", ShaderType.FragmentShader, programID, out fsID);
            GL.LinkProgram(programID);

            // Connect the shader variable names with our variables:
            attribute_vpos = GL.GetAttribLocation(programID, "vPosition");
            attribute_vcol = GL.GetAttribLocation(programID, "vColor");
            attribute_vnorm = GL.GetAttribLocation(programID, "vNormal");

            uniform_mview = GL.GetUniformLocation(programID, "M");
            uniform_lpoint = GL.GetUniformLocation(programID, "lightpoint");
            uniform_lcolor = GL.GetUniformLocation(programID, "lightcolor");

            // assign the color VBO to the pos attribute
            vbo_pos = CreateVBO(vertexData, attribute_vpos);

            // assign the color VBO to the col attribute
            vbo_col = CreateVBO(colorData, attribute_vcol);

            // assign the normal VBO to the norm attribute
            vbo_norm = CreateVBO(normalData, attribute_vnorm);

        }

        private int CreateVBO(float[] data, int attrID)
        {
            int ID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
            GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(data.Length * 4), data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attrID, 3, VertexAttribPointerType.Float, false, 0, 0);
            return ID;
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
            float timeDiff = 0f;
            if (lastDraw != null)
            {
                lastDraw.Stop();
                timeDiff = lastDraw.ElapsedMilliseconds;
                lastDraw.Restart();
            }
            else lastDraw = new Stopwatch();

            // this is not really keyboard input, but rotate the screen!
            angle += timeDiff * 0.001f;

            KeyboardState keyboard = Keyboard.GetState();

            // Angle of camera
            if (keyboard[Key.Up]) lookAngle += 0.001f * timeDiff;
            if (keyboard[Key.Down]) lookAngle -= 0.001f * timeDiff;

            float rotateSpeed = 0.0025f * timeDiff;

            // Location of the lightsource
            if (keyboard[Key.W]) LightPoint += rotateSpeed * Vector3.UnitZ;
            if (keyboard[Key.S]) LightPoint -= rotateSpeed * Vector3.UnitZ;

            if (keyboard[Key.A]) LightPoint += rotateSpeed * Vector3.UnitX;
            if (keyboard[Key.D]) LightPoint -= rotateSpeed * Vector3.UnitX;

            if (keyboard[Key.Q]) LightPoint += rotateSpeed * Vector3.UnitY;
            if (keyboard[Key.E]) LightPoint -= rotateSpeed * Vector3.UnitY;
        }

        /**
         * renders one frame
         */
        public void Tick()
        {
            KeyboardInput();

            Matrix4 M = Matrix4.CreateFromAxisAngle(new Vector3(0, 0, 1), angle);
            M *= Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), lookAngle);
            M *= Matrix4.CreateTranslation(0, 0, -1.5f);
            M *= Matrix4.CreatePerspectiveFieldOfView(1.6f, 1.3f, .1f, 1000);

            // Set the shader and the uniform variables
            GL.UseProgram(programID);
            GL.UniformMatrix4(uniform_mview, false, ref M);
            GL.Uniform3(uniform_lpoint, ref LightPoint);
            GL.Uniform3(uniform_lcolor, ref LightColor);
        }

        public void RenderGL()
        {
            GL.Disable(EnableCap.Blend);

            // Draw the height map:
            GL.EnableVertexAttribArray(attribute_vpos);
            GL.EnableVertexAttribArray(attribute_vcol);
            GL.EnableVertexAttribArray(attribute_vnorm);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 127 * 127 * 2 * 3);

            // Draw the point of the light source
            // Make the point a round
            GL.Enable(EnableCap.PointSmooth);
            // Draw the point above all else
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
            // Make it a large point so we can actually see it
            GL.PointSize(10f);
            
            GL.Begin(PrimitiveType.Points);
            GL.Vertex3(LightPoint);
            GL.End();
        }
    }
} // namespace Template
