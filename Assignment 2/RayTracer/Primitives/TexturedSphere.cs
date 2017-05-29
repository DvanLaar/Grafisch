using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer.Primitives
{
    class TexturedSphere : Sphere
    {
        public Texture texture;

        public TexturedSphere(Vector3 center, float radius, Vector3 color, Texture texture, float diffuse) : base(center, radius, color, diffuse)
        {
            this.texture = texture;
        }

        public override Vector3 GetColor(Intersection intersection)
        {
            Vector3 dir = intersection.location - this.center;
            dir.Normalize();

            int texx = Utils.scaleFloat((float)Math.Atan2(dir.Z, dir.X) / MathHelper.TwoPi + 0.5f, texture.Height);
            // int texx = MathHelper.Clamp((int)((Math.Atan2(dir.Z, dir.X) / MathHelper.TwoPi + 0.5f) * (skybox.Width - 1)), 0, skybox.Width - 1);
            int texy = (int)(Utils.SafeAcos(dir.Y) / Math.PI * (texture.Height - 1));
            // return new Vector3(1f * texx / skybox.Height, 1f * texy / skybox.Width, 0f);
            return base.GetColor(intersection) * texture.Data[texx, texy];
        }
    }
}
