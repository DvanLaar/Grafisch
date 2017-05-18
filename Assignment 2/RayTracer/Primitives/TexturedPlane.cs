using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template.Primitives
{
    class TexturedPlane : Plane
    {

        public Texture texture;
        public Vector3 textureDirection;
        public Vector3 textureDirectionPerp;
        public float textureScale = 500f;

        public TexturedPlane(Texture texture, Vector3 textureDirection, Vector3 normal, float distance, Vector3 color, float diffuse, float textureScale = 500f) : base(normal, distance, color, diffuse)
        {
            this.texture = texture;
            this.textureDirection = textureDirection.Normalized();
            this.textureDirectionPerp = Vector3.Cross(normal, textureDirection).Normalized();
            this.textureScale = textureScale;
        }

        public override Vector3 GetColor(Ray ray = null, Intersection intersection = null)
        {
            if (ray == null || intersection == null)
                throw new Exception("IMPOSSIBRU!");
            Vector3 intersectionPoint = ray.origin + (intersection.value * ray.direction);
            int textureX = (int)(textureScale * Math.Abs(Vector3.Dot(intersectionPoint - (normal * distance), textureDirection))) % texture.Width;
            int textureY = (int)(textureScale * Math.Abs(Vector3.Dot(intersectionPoint - (normal * distance), textureDirectionPerp))) % texture.Height;
            Vector3 textureColor = texture.Data[textureX, textureY];

            return new Vector3(material.color.X * textureColor.X, material.color.Y * textureColor.Y, material.color.Z * textureColor.Z);
        }
    }
}
