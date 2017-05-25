using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

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
            float redp2 = radius * radius - (c - t * ray.direction).LengthSquared;
            if (redp2 < 0 || t < 0 || t * t < redp2) return null;
            t -= (float)Math.Sqrt(redp2);
            Vector3 location = ray.origin + (t * ray.direction);
            return new Intersection(location, t, this, (location - center).Normalized());
        }

        public override bool DoesIntersect(Ray ray, float maxValue)
        {
            Vector3 c = center - ray.origin;
            float t = Vector3.Dot(c, ray.direction);
            float redp2 = radius * radius - (c - t * ray.direction).LengthSquared;
            if (redp2 < 0 || t < 0 || t * t < redp2) return false;

            // we can save a calculation of a square root by simplifying:
            // t - Math.sqrt(redp2) < maxvalue
            // t - maxvalue < Math.sqrt(redp2)
            // (t - maxvalue)^2 < redp2
            return t < maxValue || (t - maxValue) * (t - maxValue) < redp2;
        }
    }
}
