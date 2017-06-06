using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template_P3;

namespace template_P3
{
    public class Model
    {
        public Mesh mesh;
        public Texture texture;
        public Shader shader;
        public Matrix4 modeltransform;

        public Model(Mesh mesh, Texture texture, Shader shader, Matrix4 modeltransform)
        {
            this.mesh = mesh;
            this.texture = texture;
            this.shader = shader;
            this.modeltransform = modeltransform;
        }

        public virtual void Render(Matrix4 ModelToWorld, Matrix4 WorldToScreen)
        {
            mesh.Render(shader, ModelToWorld * modeltransform, WorldToScreen * modeltransform, texture);
        }
    }
}
