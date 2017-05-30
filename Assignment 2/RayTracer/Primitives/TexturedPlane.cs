using OpenTK;

namespace RayTracer.Primitives
{
    // A plane with a texture
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

            // The longer the direction vector is,
            // the bigger the dot products should be before
            // we begin with a new repetition of the texture,
            // so we want to divide by the length squared,
            // so if we have a intersection at a distance of the original length, 
            // the dot product returns actually 1.
            this.dirTX = Vector3.Multiply(dirTX, 1f / dirTX.LengthSquared);
            this.dirTY = Vector3.Multiply(dirTY, 1f / dirTY.LengthSquared);

            // This is the offset due to the distance to the plane.
            // Precalculate this to make it faster:
            offsetX = -texture.Width * distance * Vector3.Dot(normal, dirTX);
            offsetY = -texture.Height * distance * Vector3.Dot(normal, dirTY);
        }

        public override Vector3 GetDiffuseColor(Intersection intersection)
        {
            // Calculate texture coordinate
            int textureX = (int)(texture.Width * Vector3.Dot(intersection.location, dirTX) + offsetX) % texture.Width;
            int textureY = (int)(texture.Height * Vector3.Dot(intersection.location, dirTY) + offsetY) % texture.Height;
            
            // Since modulo is not perfect for negative numbers:
            if (textureX < 0) textureX += texture.Width;
            if (textureY < 0) textureY += texture.Height;

            return Vector3.Multiply(base.GetDiffuseColor(intersection), texture.Data[textureX, textureY]);
        }
    }
}
