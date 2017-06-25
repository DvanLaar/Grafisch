using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using Template_P3;
using OpenTK;

namespace template_P3
{
    public class PostVigAndChromShader : Shader
    {
        private int uniform_vignettingfactor,
                    uniform_center,
                    uniform_ca_factor;

        public PostVigAndChromShader(string vertexShader, string fragmentShader) : base(vertexShader, fragmentShader)
        {
            uniform_vignettingfactor = GL.GetUniformLocation(programID, "vignettingfactor");
            uniform_center = GL.GetUniformLocation(programID, "center");
            uniform_ca_factor = GL.GetUniformLocation(programID, "ca_factor");
        }

        public void KernelRender(int textureID, float vignettingfactor, Vector2 center, Vector3 ca_factor)
        {
            // enable texture
            int texLoc = GL.GetUniformLocation(programID, "pixels");
            GL.Uniform1(texLoc, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            // enable shader
            GL.UseProgram(programID);

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
