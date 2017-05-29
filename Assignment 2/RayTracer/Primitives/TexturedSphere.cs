using OpenTK;
using System;

namespace RayTracer.Primitives
{
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

            int texx = Utils.scaleFloat((float)Math.Atan2(dir.Z, dir.X) / MathHelper.TwoPi + 0.5f, texture.Height);
            // int texx = MathHelper.Clamp((int)((Math.Atan2(dir.Z, dir.X) / MathHelper.TwoPi + 0.5f) * (skybox.Width - 1)), 0, skybox.Width - 1);
            int texy = (int)(Utils.SafeAcos(dir.Y) / Math.PI * (texture.Height - 1));
            // return new Vector3(1f * texx / skybox.Height, 1f * texy / skybox.Width, 0f);
            return base.GetDiffuseColor(intersection) * texture.Data[texx, texy];
        }
    }
}
