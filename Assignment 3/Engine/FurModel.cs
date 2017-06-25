using OpenTK;
using rasterizer;
using OpenTK.Graphics.OpenGL;

namespace rasterizer
{
    class FurModel : Model
    {
        private const int numLayers = 32;

        private Texture furTexture;
        private FurShader furShader;

        public FurModel(Mesh mesh, Texture texture, Texture furTexture, Shader shader, FurShader furShader, Matrix4 modelTransformation)
            : base(mesh, texture, shader, modelTransformation)
        {
            this.furTexture = furTexture;
            this.furShader = furShader;
        }

        public override void Render(Matrix4 modelToWorld, Matrix4 worldToScreen)
        {
            Matrix4 meshToWorld = meshToModel * modelToWorld;

            // enable shader
            mesh.Prepare(shader);
            GL.UseProgram(shader.programID);
            mesh.Render(shader, meshToWorld, worldToScreen, color);

            // Render 32 layers of basic short fur
            for (int i = 0; i < numLayers; i++)
                mesh.FurRender(furShader, meshToWorld, worldToScreen, furTexture, i);
        }

    }
}
