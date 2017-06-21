using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template_P3;

namespace template_P3
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
            // enable texture
            int texLoc = GL.GetUniformLocation(programID, "pixels");
            GL.Uniform1(texLoc, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            // enable shader
            GL.UseProgram(programID);

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

        public static Kernel Uniform(int width, int height, float sneakyval = 2f)
        {
            if(width > 32 || height > 32)
            {
                Console.WriteLine("The width or height of the requested Uniform kernel exceeds 32 (limited as declared in the shader)");
                return Identity;
            }
            float val = sneakyval / (float)(width*height);
            float[] hor = new float[width];
            for (int x = 0; x < width; x++)
                hor[x] = val;

            float[] ver = new float[height];
            for (int y = 0; y < height; y++)
                ver[y] = val;

            return new Kernel(hor,ver);
        }

        public static Kernel Identity
        {
            get
            {
                return new Kernel(new float[1] {1}, new float[1] { 1 });
            }
        }

        public static Kernel BoxBlur
        {
            get
            {
                return new Kernel(new float[3] { 1/3f,1/3f,1/3f }, new float[3] { 1/3f,1/3f,1/3f });
            }
        }

        public static Kernel SmallGaussianBlur
        {
            get
            {
                return new Kernel(new float[3] { 1f / 4f, 2f / 4f, 1f / 4f }, new float[3] { 1f / 4f, 2f / 4f, 1f / 4f });
            }
        }


        public static Kernel EdgeDetection
        {
            get
            {
                return new Kernel(new float[3] { 1f, 0, -1f }, new float[3] { 1f, 0, -1f });
            }
        }

        public static Kernel SobelHorizontal
        {
            get
            {
                return new Kernel(new float[3] { 1f, 0, -1f }, new float[3] { 1f, 2f, 1f });
            }
        }

        public static Kernel SobelVertical
        {
            get
            {
                return new Kernel(new float[3] { 1f, 2f, 1f }, new float[3] { 1f, 0f, -1f });
            }
        }

        public static Kernel PrewittHorizontal
        {
            get
            {
                return new Kernel(new float[3] { 1f, 0, -1f }, new float[3] { 1f, 1f, 1f });
            }
        }

        public static Kernel PrewittVertical
        {
            get
            {
                return new Kernel(new float[3] { 1f, 1f, 1f }, new float[3] { 1f, 0f, -1f });
            }
        }
    }
}
