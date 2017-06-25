using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace rasterizer
{
    public class Model
    {
        public Mesh mesh;
        public Texture texture, normalMap = null;
        public Shader shader;
        public Matrix4 meshToModel;
        public Vector3 color = new Vector3(1, 1, 1);

        /// <summary>
        /// Normal model that doesn't require a lot of extra features
        /// </summary>
        public Model(Mesh mesh, Texture texture, Shader shader, Matrix4 meshToModel)
        {
            this.mesh = mesh;
            this.texture = texture;
            this.shader = shader;
            this.meshToModel = meshToModel;
        }

        public virtual void Render(Matrix4 ModelToWorld, Matrix4 WorldToScreen)
        {
            if (mesh == null) return;

            mesh.Prepare(shader);
            GL.UseProgram(shader.programID);

            shader.SetTexture(texture);
            shader.SetNormal(normalMap);
            mesh.Render(shader, meshToModel * ModelToWorld, WorldToScreen, color);
        }
    }
}
