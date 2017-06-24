using System;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace Template_P3
{

    public class Shader
    {
        // data members
        public int programID, vsID, fsID;
        public int attribute_vuvs, attribute_vnrm, attribute_vtan, attribute_vpos;
        public int uniform_modeltoworld, uniform_worldtoscreen, uniform_camerapos, uniform_materialcolor;
        public int[] uniform_lightpos;

        // constructor
        public Shader(String vertexShader, String fragmentShader)
        {
            programID = GL.CreateProgram();

            // compile shaders
            vsID = Load(vertexShader, ShaderType.VertexShader);
            fsID = Load(fragmentShader, ShaderType.FragmentShader);
            GL.LinkProgram(programID);
            string log = GL.GetProgramInfoLog(programID);
            if (log != "")
            {
                Console.WriteLine("Shader log for " + vertexShader + ":\n" + log);
            }

            // get locations of shader parameters
            attribute_vuvs = GL.GetAttribLocation(programID, "vUV");
            attribute_vnrm = GL.GetAttribLocation(programID, "vNormal");
            attribute_vtan = GL.GetAttribLocation(programID, "vTangent");
            attribute_vpos = GL.GetAttribLocation(programID, "vPosition");

            uniform_modeltoworld = GL.GetUniformLocation(programID, "modelToWorld");
            uniform_worldtoscreen = GL.GetUniformLocation(programID, "worldToScreen");
            for (int i = 0; i < 4; i++ )
                uniform_lightpos[i] = GL.GetUniformLocation(programID, "lightpos" + i);
            uniform_camerapos = GL.GetUniformLocation(programID, "camerapos");
            uniform_materialcolor = GL.GetUniformLocation(programID, "materialcolor");
        }

        public static Shader Load(string vertexShader, string fragmentShader)
        {
            return new Shader(
                "../../shaders/" + vertexShader + ".glsl",
                "../../shaders/" + fragmentShader + ".glsl");
        }

        // loading shaders
        int Load(String filename, ShaderType type)
        {
            // source: http://neokabuto.blogspot.nl/2013/03/opentk-tutorial-2-drawing-triangle.html
            int ID = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(filename)) GL.ShaderSource(ID, sr.ReadToEnd());
            GL.CompileShader(ID);
            GL.AttachShader(programID, ID);
            string log = GL.GetShaderInfoLog(ID);
            if (log != "")
            {
                Console.WriteLine("While loading shader " + filename + ":\n" + log);
            }
            return ID;
        }
    }

} // namespace Template_P3
