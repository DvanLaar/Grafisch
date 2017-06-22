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
        public Texture texture, NormalMap = null;
        public Shader shader;
        public Matrix4 modeltransform;
        public Vector3 MaterialColor = new Vector3(1, 1, 1);

        public Model(Mesh mesh, Texture texture, Shader shader, Matrix4 modeltransform)
        {
            this.mesh = mesh;
            this.texture = texture;
            this.shader = shader;
            this.modeltransform = modeltransform;
        }

        public virtual void Render(Matrix4 ModelToWorld, Matrix4 WorldToScreen)
        {
            mesh.Prepare(shader);
            mesh.SetTexture(texture, shader.programID);
            if (NormalMap != null)
                mesh.SetNormal(NormalMap, shader.programID);
            mesh.Render(shader, ModelToWorld * modeltransform, WorldToScreen, MaterialColor);
        }
    }
}
