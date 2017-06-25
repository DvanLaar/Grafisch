using System;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace rasterizer
{
    public class Shader
    {
        // data members
        public int programID, vsID, fsID;
        public int attribute_vuvs, attribute_vnrm, attribute_vtan, attribute_vpos;
        public int uniform_modeltoworld, uniform_worldtoscreen, uniform_camerapos, uniform_materialcolor;
        public int uniform_nlights, uniform_lightpos;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="vertexShader"></param>
        /// <param name="fragmentShader"></param>
        public Shader(String vertexShader, String fragmentShader)
        {
            programID = GL.CreateProgram();

            // compile shaders
            vsID = LoadShader(vertexShader, ShaderType.VertexShader);
            fsID = LoadShader(fragmentShader, ShaderType.FragmentShader);
            GL.LinkProgram(programID);

            string infoLog = GL.GetProgramInfoLog(programID);
            if (infoLog != "")
                Console.WriteLine("Shader log for " + vertexShader + ":\n" + infoLog);

            // get locations of shader parameters
            attribute_vuvs = GL.GetAttribLocation(programID, "vUV");
            attribute_vnrm = GL.GetAttribLocation(programID, "vNormal");
            attribute_vtan = GL.GetAttribLocation(programID, "vTangent");
            attribute_vpos = GL.GetAttribLocation(programID, "vPosition");

            // get locations of most common used uniforms
            uniform_modeltoworld = GL.GetUniformLocation(programID, "modelToWorld");
            uniform_worldtoscreen = GL.GetUniformLocation(programID, "worldToScreen");
            uniform_camerapos = GL.GetUniformLocation(programID, "camerapos");
            uniform_materialcolor = GL.GetUniformLocation(programID, "materialcolor");

            uniform_nlights = GL.GetUniformLocation(programID, "nlights");
            uniform_lightpos = GL.GetUniformLocation(programID, "lightpos");
        }

        public static Shader Load(string vertexShader, string fragmentShader)
        {
            return new Shader(
                "../../shaders/" + vertexShader + ".glsl",
                "../../shaders/" + fragmentShader + ".glsl");
        }

        // loading shaders
        private int LoadShader(String fileName, ShaderType type)
        {
            // source: http://neokabuto.blogspot.nl/2013/03/opentk-tutorial-2-drawing-triangle.html
            int ID = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(fileName))
            {
                GL.ShaderSource(ID, sr.ReadToEnd());
            }
            GL.CompileShader(ID);
            GL.AttachShader(programID, ID);

            string infoLog = GL.GetShaderInfoLog(ID);
            if (infoLog != "")
                Console.WriteLine("While loading shader " + fileName + ":\n" + infoLog);

            return ID;
        }

        /// <summary>
        /// Loads three uniforms to the GPU since these are used almost everywhere.
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="meshToWorld"></param>
        /// <param name="worldToScreen"></param>
        public void SetUniforms(Matrix4 meshToWorld, Matrix4 worldToScreen)
        {
            GL.UniformMatrix4(uniform_modeltoworld, false, ref meshToWorld);
            GL.UniformMatrix4(uniform_worldtoscreen, false, ref worldToScreen);
            GL.Uniform3(uniform_camerapos, ref Game.cameraPosition);
        }

        /// <summary>
        /// Loads a 2D-texture to a specific place on the GPU specified by unit.
        /// </summary>
        /// <param name="texture">the texture to apply</param>
        /// <param name="programID">the used shader</param>
        /// <param name="varName">variable name of the sampler2D in the fragment shader</param>
        /// <param name="unit">the location of the texture to put it in</param>
        public void Load2DTexture(int textureID, string varName, TextureUnit unit)
        {
            GL.Uniform1(GL.GetUniformLocation(programID, varName), unit - TextureUnit.Texture0);
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
        }

        public void Load2DTexture(Texture texture, string varName, TextureUnit unit)
        {
            Load2DTexture(texture == null ? -1 : texture.id, varName, unit);
        }

        /// <summary>
        /// Loads a texture of this mesh to the GPU
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="programID"></param>
        public void SetTexture(Texture texture)
        {
            Load2DTexture(texture, "pixels", TextureUnit.Texture0);
        }

        public void SetTexture(int textureID)
        {
            Load2DTexture(textureID, "pixels", TextureUnit.Texture0);
        }

        /// <summary>
        /// Loads the normal map to the GPU
        /// </summary>
        /// <param name="normalMap"></param>
        /// <param name="programID"></param>
        public void SetNormal(Texture normalMap)
        {
            Load2DTexture(normalMap, "normals", TextureUnit.Texture1);
        }
    }

} // namespace Template_P3
