using OpenTK;
using Template_P3;

namespace template_P3
{
    class ReflectiveModel : Model
    {
        private CubeTexture skybox;

        public ReflectiveModel(Mesh mesh, Shader shader, Matrix4 modeltranform, CubeTexture skybox) : base(mesh, null, shader, modeltranform)
        {
            this.skybox = skybox;
        }

        public override void Render(Matrix4 ModelToWorld, Matrix4 WorldToScreen)
        {
            if (mesh == null) return;
            Matrix4 mtw = ModelToWorld * meshToModel;
            Matrix4 wts = WorldToScreen;
            mesh.ReflectiveRender(base.shader, mtw, wts, skybox, MaterialColor);
        }
    }
}