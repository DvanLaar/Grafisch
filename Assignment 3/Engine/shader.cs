using System;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace Template_P3
{

    public class Shader
    {
        // data members
        public int programID, vsID, fsID;
        public int attribute_vpos, attribute_vnrm, attribute_vuvs;
        public int uniform_modeltoworld, uniform_worldtoscreen, uniform_camerapos, uniform_materialcolor;

        // constructor
        public Shader(String vertexShader, String fragmentShader)
        {
            // compile shaders
            programID = GL.CreateProgram();
            Load(vertexShader, ShaderType.VertexShader, programID, out vsID);
            Load(fragmentShader, ShaderType.FragmentShader, programID, out fsID);
            GL.LinkProgram(programID);
            string log = GL.GetProgramInfoLog(programID);
            if (log != "")
            {
                Console.WriteLine("Shader log for " + vertexShader + ":\n" + log);
            }

            // get locations of shader parameters
            attribute_vpos = GL.GetAttribLocation(programID, "vPosition");
            attribute_vnrm = GL.GetAttribLocation(programID, "vNormal");
            attribute_vuvs = GL.GetAttribLocation(programID, "vUV");

            uniform_modeltoworld = GL.GetUniformLocation(programID, "modelToWorld");
            uniform_worldtoscreen = GL.GetUniformLocation(programID, "worldToScreen");
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
        void Load(String filename, ShaderType type, int program, out int ID)
        {
            // source: http://neokabuto.blogspot.nl/2013/03/opentk-tutorial-2-drawing-triangle.html
            ID = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(filename)) GL.ShaderSource(ID, sr.ReadToEnd());
            GL.CompileShader(ID);
            GL.AttachShader(program, ID);
            string log = GL.GetShaderInfoLog(ID);
            if (log != "")
            {
                Console.WriteLine("While loading shader " + filename + ":\n" + log);
            }
        }
    }

} // namespace Template_P3
