using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template.Primitives
{
    class TexturedTriangle : Triangle
    {

        public Texture texture;
        public Vector2 tex1, tex2, tex3;

        public TexturedTriangle(Vector3 pos1, Vector3 pos2, Vector3 pos3, Texture texture, Vector2 tex1, Vector2 tex2, Vector2 tex3, Vector3 color) : base(pos1,pos2,pos3,color)
        {
            this.texture = texture;
            this.tex1 = tex1;
            this.tex2 = tex2;
            this.tex3 = tex3;
        }

        public override Vector3 GetColor(Ray ray = null, Intersection intersection = null)
        {
            if (ray == null || intersection == null)
                throw new Exception("IMPOSSIBRU!");
            Vector3 barycord = intersection.barycentric;
            Vector2 texcord = (barycord.X * tex1) + (barycord.Y * tex2) + (barycord.Z * tex3);
            Vector3 textureColor = texture.Data[(int)(MathHelper.Clamp(texcord.X*texture.Width,0,texture.Width-1)), (int)(MathHelper.Clamp(texcord.Y * texture.Height,0f,texture.Height-1))];
            return new Vector3(color.X * textureColor.X, color.Y * textureColor.Y, color.Z * textureColor.Z);
        }
    }
}
