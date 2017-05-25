using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer.Primitives
{
    class TexturedTriangle : Triangle
    {

        public Texture texture;
        public Vector2 tex1, tex2, tex3;

        public TexturedTriangle(Vector3 pos1, Vector3 pos2, Vector3 pos3, Texture texture, Vector2 tex1, Vector2 tex2, Vector2 tex3, Vector3 color, float diffuse) : base(pos1, pos2, pos3, color, diffuse)
        {
            this.texture = texture;
            this.tex1 = tex1;
            this.tex2 = tex2;
            this.tex3 = tex3;
        }

        public override Vector3 GetColor(Intersection intersection)
        {
            Vector3 barycord = intersection.barycentric;
            Vector2 texcord = (barycord.X * tex1) + (barycord.Y * tex2) + (barycord.Z * tex3);

            int tx = Utils.scaleFloat(texcord.X, texture.Width - 1);
            int ty = Utils.scaleFloat(texcord.Y, texture.Height - 1);
            return material.color * texture.Data[tx, ty];
        }
    }
}
