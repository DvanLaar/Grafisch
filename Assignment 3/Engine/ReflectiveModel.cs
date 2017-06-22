using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template_P3;

namespace template_P3
{
    class ReflectiveModel : Model
    {

        public Shader shader;

        public Vector3 materialcolor = new Vector3(1, 1, 1);
        public CubeTexture skybox;

        public ReflectiveModel(Mesh mesh,Shader shader,Matrix4 modeltranform, CubeTexture skybox) : base(mesh, null, shader, modeltranform)
        {
            this.shader = shader;
            this.skybox = skybox;
        }

        public override void Render(Matrix4 ModelToWorld, Matrix4 WorldToScreen)
        {
            Matrix4 mtw = ModelToWorld * modeltransform;
            Matrix4 wts = WorldToScreen;
            mesh.ReflectiveRender(shader, mtw, wts, skybox, materialcolor);

        }

    }
}