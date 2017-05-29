using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace RayTracer.Primitives
{
    public class Sphere : Primitive
    {
        public float radius;
        public Vector3 center;

        public Sphere(Vector3 center, float radius, Vector3 color, float diffuse) : base(color, diffuse)
        {
            this.radius = radius;
            this.center = center;
        }

        public override Intersection Intersect(Ray ray)
        {
            Vector3 c = center - ray.origin;
            float t = Vector3.Dot(c, ray.direction);
            Vector3 q = c - t * ray.direction;
            float p2 = Vector3.Dot(q, q);
            if (p2 > radius * radius)
                return null;
            t -= (float)Math.Sqrt(radius * radius - p2);
            if (t > 0)
            {
                Vector3 location = ray.origin + (t * ray.direction);
                return new Intersection(location, t, this, (location - center).Normalized());
            }
            return null;
        }

        public override Vector3 getNormal(Vector3 posOnPrim)
        {
            return (posOnPrim - this.center).Normalized();
        }

        public override void Debug()
        {
            GL.Begin(PrimitiveType.LineLoop);
            GL.Color3(material.color);
            for (int i = 0; i <= 300; i++)
            {
                float angle = (float)(2 * Math.PI * i) / 300f;
                float x = (float)Math.Cos(angle) * radius;
                float y = (float)Math.Sin(angle) * radius;
                GL.Vertex2(x + center.X, y + center.Z);
            }
            GL.End();
        }
    }
}
