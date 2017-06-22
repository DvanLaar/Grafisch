using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template_P3;
using OpenTK.Graphics.OpenGL;

namespace template_P3
{
    class FurModel : Model
    {
        public Texture furtexture;
        public Shader furshader;

        public Vector3 materialcolor = new Vector3(1,1,1);

        public FurModel(Mesh mesh, Texture texture, Texture furtexture, Shader shader, Shader furshader, Matrix4 modeltranform):base(mesh,texture,shader,modeltranform)
        {
            this.furtexture = furtexture;
            this.furshader = furshader;
        }

        public override void Render(Matrix4 ModelToWorld, Matrix4 WorldToScreen)
        {
            Matrix4 mtw = ModelToWorld * modeltransform;
            Matrix4 wts = WorldToScreen;
            mesh.Render(shader, mtw, wts,texture,materialcolor);
            
            for (int i = 0; i < 32; i++)
            {
                mesh.FurRender(furshader, mtw, wts, furtexture, i);
            }
        }

    }
}
