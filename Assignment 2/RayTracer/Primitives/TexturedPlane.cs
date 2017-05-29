using OpenTK;

namespace RayTracer.Primitives
{
    class TexturedPlane : Plane
    {
        public Texture texture;
        public Vector3 dirTX, dirTY;
        private float offsetX, offsetY;

        public TexturedPlane(Texture texture, Vector3 dirTX, Vector3 dirTY,
            float distance, Material material)
                : base(Vector3.Cross(dirTX, dirTY).Normalized(), distance, material)
        {
            this.texture = texture;
            this.dirTX = Vector3.Multiply(dirTX, 1f / dirTX.LengthSquared);
            this.dirTY = Vector3.Multiply(dirTY, 1f / dirTY.LengthSquared);

            offsetX = -texture.Width * distance * Vector3.Dot(normal, dirTX);
            offsetY = -texture.Height * distance * Vector3.Dot(normal, dirTY);
        }

        public override Vector3 GetDiffuseColor(Intersection intersection)
        {
            int textureX = (int)(texture.Width * Vector3.Dot(intersection.location, dirTX) + offsetX) % texture.Width;
            if (textureX < 0) textureX += texture.Width;
            int textureY = (int)(texture.Height * Vector3.Dot(intersection.location, dirTY) + offsetY) % texture.Height;
            if (textureY < 0) textureY += texture.Height;

            return Vector3.Multiply(base.GetDiffuseColor(intersection), texture.Data[textureX, textureY]);
        }
    }
}
