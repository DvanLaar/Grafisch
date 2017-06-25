using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rasterizer;

namespace rasterizer
{
    public class PostKernelShader : Shader
    {
        private int uniform_pixelwidth, uniform_pixelheight,
                   uniform_kernelwidth, uniform_kernelheight,
                   uniform_centerx, uniform_centery,
                   uniform_horizontal, uniform_vertical;

        public PostKernelShader(string vertexShader, string fragmentShader) : base(vertexShader, fragmentShader)
        {
            uniform_pixelwidth = GL.GetUniformLocation(programID, "pixelwidth");
            uniform_pixelheight = GL.GetUniformLocation(programID, "pixelheight");
            uniform_kernelwidth = GL.GetUniformLocation(programID, "kernelwidth");
            uniform_kernelheight = GL.GetUniformLocation(programID, "kernelheight");
            uniform_centerx = GL.GetUniformLocation(programID, "centerx");
            uniform_centery = GL.GetUniformLocation(programID, "centery");
            uniform_horizontal = GL.GetUniformLocation(programID, "horizontal");
            uniform_vertical = GL.GetUniformLocation(programID, "vertical");
        }

        public void KernelRender(int textureID, float textureWidth, float textureHeight, Kernel kernel)
        {
            GL.UseProgram(programID);
            SetTexture(textureID);

            // set uniforms
            GL.Uniform1(uniform_pixelwidth, 1f / textureWidth);
            GL.Uniform1(uniform_pixelheight, 1f / textureHeight);
            GL.Uniform1(uniform_kernelwidth, kernel.horizontal.Length);
            GL.Uniform1(uniform_kernelheight, kernel.vertical.Length);
            GL.Uniform1(uniform_centerx, Math.Floor(kernel.horizontal.Length / 2f));
            GL.Uniform1(uniform_centery, Math.Floor(kernel.vertical.Length / 2f));
            GL.Uniform1(uniform_horizontal, kernel.horizontal.Length, kernel.horizontal);
            GL.Uniform1(uniform_vertical, kernel.vertical.Length, kernel.vertical);

            // enable position and uv attributes
            GL.EnableVertexAttribArray(attribute_vpos);
            GL.EnableVertexAttribArray(attribute_vuvs);
        }
    }

    public struct Kernel
    {
        public float[] horizontal, vertical;

        public Kernel(float[] hor, float[] ver)
        {
            horizontal = hor;
            vertical = ver;
        }

        /// <summary>
        /// Uniform kernel of variable size with little sneaky value to make it not so uniform
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="desiredSum"></param>
        /// <returns></returns>
        public static Kernel Uniform(int width, int height, float desiredSum = 2f)
        {
            if (width > 32 || height > 32)
            {
                Console.WriteLine("The width or height of the requested Uniform kernel exceeds 32 (limited as declared in the shader)");
                return Identity;
            }
            float val = desiredSum / (width * height);

            float[] hor = new float[width], ver = new float[height];
            for (int i = 0; i < width; i++)
                hor[i] = val;
            for (int i = 0; i < height; i++)
                ver[i] = val;
            return new Kernel(hor, ver);
        }

        /// <summary>
        /// Identity Kernel [1]
        /// </summary>
        public static readonly Kernel Identity = new Kernel(new float[1] { 1 }, new float[1] { 1 });

        /// <summary>
        /// Basic uniform 3x3
        /// </summary>
        public static readonly Kernel BoxBlur = new Kernel(new float[3] { 1 / 3f, 1 / 3f, 1 / 3f }, new float[3] { 1 / 3f, 1 / 3f, 1 / 3f });

        /// <summary>
        /// Small (3x3) approximation of Gaussian blur
        /// </summary>
        public static readonly Kernel SmallGaussianBlur = new Kernel(new float[3] { 1f / 4f, 2f / 4f, 1f / 4f }, new float[3] { 1f / 4f, 2f / 4f, 1f / 4f });

        /// <summary>
        /// Simple 3x3 edge detection kernel
        /// </summary>
        public static readonly Kernel EdgeDetection = new Kernel(new float[3] { 1f, 0, -1f }, new float[3] { 1f, 0, -1f });

        /// <summary>
        /// Horizontal Sobel Operator
        /// </summary>
        public static readonly Kernel SobelHorizontal = new Kernel(new float[3] { 1f, 0, -1f }, new float[3] { 1f, 2f, 1f });

        /// <summary>
        /// Vertical Sobel Operator
        /// </summary>
        public static readonly Kernel SobelVertical = new Kernel(new float[3] { 1f, 2f, 1f }, new float[3] { 1f, 0f, -1f });

        /// <summary>
        /// Horizontal Prewitt Operator
        /// </summary>
        public static readonly Kernel PrewittHorizontal = new Kernel(new float[3] { 1f, 0, -1f }, new float[3] { 1f, 1f, 1f });

        /// <summary>
        /// Vertical Prewitt Operator
        /// </summary>
        public static readonly Kernel PrewittVertical = new Kernel(new float[3] { 1f, 1f, 1f }, new float[3] { 1f, 0f, -1f });
    }
}
