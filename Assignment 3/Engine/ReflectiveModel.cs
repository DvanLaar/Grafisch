using OpenTK;

namespace rasterizer
{
    class ReflectiveModel : Model
    {
        private CubeTexture skyBox;

        public ReflectiveModel(Mesh mesh, Shader shader, Matrix4 transformation, CubeTexture skybox) : base(mesh, null, shader, transformation)
        {
            this.skyBox = skybox;
        }

        public override void Render(Matrix4 modelToWorld, Matrix4 worldToScreen)
        {
            if (mesh == null) return;
            mesh.ReflectiveRender(shader, modelToWorld * meshToModel, worldToScreen, skyBox, color);
        }
    }
}