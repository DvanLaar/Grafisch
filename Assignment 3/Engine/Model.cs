﻿using OpenTK;
using OpenTK.Graphics.OpenGL;
using Template_P3;

namespace template_P3
{
    public class Model
    {
        public Mesh mesh;
        public Texture texture, NormalMap = null;
        public Shader shader;
        public Matrix4 meshToModel;
        public Vector3 MaterialColor = new Vector3(1, 1, 1);

        /// <summary>
        /// Normal model that doesn't require a lot of extra features
        /// </summary>
        public Model(Mesh mesh, Texture texture, Shader shader, Matrix4 modeltransform)
        {
            this.mesh = mesh;
            this.texture = texture;
            this.shader = shader;
            this.meshToModel = modeltransform;
        }

        public virtual void Render(Matrix4 ModelToWorld, Matrix4 WorldToScreen)
        {
            if (mesh == null) return;
            mesh.Prepare(shader);
            // enable shader
            GL.UseProgram(shader.programID);

            mesh.SetTexture(texture, shader.programID);
            mesh.SetNormal(NormalMap, shader.programID);
            mesh.Render(shader, meshToModel * ModelToWorld, WorldToScreen, MaterialColor);
        }
    }
}
