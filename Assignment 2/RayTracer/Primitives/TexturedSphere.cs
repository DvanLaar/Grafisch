using OpenTK;
using System;

namespace RayTracer.Primitives
{
    // A textured sphere, where we project the square of the texture on a sphere.
    class TexturedSphere : Sphere
    {
        public Texture texture;

        public TexturedSphere(Vector3 center, float radius,
            Texture texture, Material material) : base(center, radius, material)
        {
            this.texture = texture;
        }

        public override Vector3 GetDiffuseColor(Intersection intersection)
        {
            Vector3 dir = intersection.location - this.center;
            dir.Normalize();

            //Same code as used for the skybox
            int texx = Utils.scaleFloat((float)Math.Atan2(dir.Z, dir.X) / MathHelper.TwoPi + 0.5f, texture.Height);
            int texy = (int)(Utils.SafeAcos(dir.Y) / Math.PI * (texture.Height - 1));
            return base.GetDiffuseColor(intersection) * texture.Data[texx, texy];
        }
    }
}
