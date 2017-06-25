using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using rasterizer;
using OpenTK;

namespace rasterizer
{
    public class PostVigAndChromShader : Shader
    {
        private int uniform_vignettingfactor, uniform_center, uniform_ca_factor;

        public PostVigAndChromShader(string vertexShader, string fragmentShader) : base(vertexShader, fragmentShader)
        {
            uniform_vignettingfactor = GL.GetUniformLocation(programID, "vignettingfactor");
            uniform_center = GL.GetUniformLocation(programID, "center");
            uniform_ca_factor = GL.GetUniformLocation(programID, "ca_factor");
        }

        public void KernelRender(int textureID, float vignettingfactor, Vector2 center, Vector3 ca_factor)
        {
            GL.UseProgram(programID);
            SetTexture(textureID);

            // set uniforms
            GL.Uniform1(uniform_vignettingfactor, vignettingfactor);
            GL.Uniform2(uniform_center, center);
            GL.Uniform3(uniform_ca_factor, ca_factor);

            // enable position and uv attributes
            GL.EnableVertexAttribArray(attribute_vpos);
            GL.EnableVertexAttribArray(attribute_vuvs);
        }
    }
}
