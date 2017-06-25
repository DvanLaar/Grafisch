using rasterizer;
using OpenTK.Graphics.OpenGL;

namespace rasterizer
{
    public class FurShader : Shader
    {
        public int uniform_furoffset;

        public FurShader(string vertexShader, string fragmentShader) : base(vertexShader, fragmentShader)
        {
            uniform_furoffset = GL.GetUniformLocation(programID, "furoffset");
        }
    }
}
