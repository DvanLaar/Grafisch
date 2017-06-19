using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template_P3;

namespace template_P3
{
    public class postKernelShader : Shader
    {

        public int uniform_pixelwidth, uniform_pixelheight,
                   uniform_kernelwidth, uniform_kernelheight,
                   uniform_horizontal, uniform_vertical;

        public postKernelShader(string vertexShader, string fragmentShader) : base(vertexShader, fragmentShader)
        {
            uniform_pixelwidth = GL.GetUniformLocation(programID, "pixelwidth");
            uniform_pixelheight = GL.GetUniformLocation(programID, "pixelheight");
            uniform_kernelwidth = GL.GetUniformLocation(programID, "kernelwidth");
            uniform_kernelheight = GL.GetUniformLocation(programID, "kernelheight");
            uniform_horizontal = GL.GetUniformLocation(programID, "horizontal");
            uniform_vertical = GL.GetUniformLocation(programID, "vertical");
        }
    }

    public struct Kernel
    {
        public float[] horizontal;
        public float[] vertical;

        public Kernel(float[] hor, float[] ver)
        {
            horizontal = hor;
            vertical = ver;
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
