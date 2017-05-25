using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer.Primitives
{
    class TexturedPlane : Plane
    {
        public Texture texture;
        public Vector3 dirTX, dirTY;
        public float textureScale = 500f;
        private float offsetX, offsetY;

        public TexturedPlane(Texture texture, Vector3 dirTX, Vector3 dirTY, float distance, Vector3 color, float diffuse, float textureScale = 500f)
                : base(Vector3.Cross(dirTX, dirTY).Normalized(), distance, color, diffuse)
        {
            this.texture = texture;
            this.dirTX = Vector3.Multiply(dirTX, 1f / Vector3.Dot(dirTX, dirTX));
            this.dirTY = Vector3.Multiply(dirTY, 1f / Vector3.Dot(dirTY, dirTY));
            this.textureScale = textureScale;

            offsetX = -textureScale * distance * Vector3.Dot(normal, dirTX);
            offsetY = -textureScale * distance * Vector3.Dot(normal, dirTY);
        }

        public override Vector3 GetColor(Intersection intersection)
        {
            int textureX = (int)(textureScale * Vector3.Dot(intersection.location, dirTX) + offsetX) % texture.Width;
            if (textureX < 0) textureX += texture.Width;
            int textureY = (int)(textureScale * Vector3.Dot(intersection.location, dirTY) + offsetY) % texture.Height;
            if (textureY < 0) textureY += texture.Height;
            
            return Vector3.Multiply(base.GetColor(intersection), texture.Data[textureX, textureY]);
        }
    }
}
