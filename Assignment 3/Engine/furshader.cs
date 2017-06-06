using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template_P3;
using OpenTK.Graphics.OpenGL;

namespace template_P3
{
    public class FurShader : Shader 
    {


        public int uniform_furoffset;

        public FurShader(string vertexShader,string fragmentShader):base(vertexShader,fragmentShader)
        {
            uniform_furoffset = GL.GetUniformLocation(programID, "furoffset");
        }
    }
}
