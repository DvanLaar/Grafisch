using System;
using OpenTK;

namespace RayTracer.Primitives
{
    // Class for a sphere of radius 'radius' and center 'center' (pretty straight-forward)
    public class Sphere : Primitive
    {
        public readonly float radius, radiusSq;
        public readonly Vector3 center;
        public readonly BoundingBox BB;

        public Sphere(Vector3 center, float radius, Material material) : base(material)
        {
            this.radius = radius;
            this.radiusSq = radius * radius;
            this.center = center;

            BB = new BoundingBox(center.X - radius, center.X + radius, center.Y - radius, center.Y + radius, center.Z - radius, center.Z + radius);
        }

        public override Intersection Intersect(Ray ray)
        {
            // Modified from the lecture slides:
            if (!BB.boundsWith(ray)) return null;

            Vector3 c = center - ray.origin;
            float t = Vector3.Dot(c, ray.direction);
            float redp2 = radiusSq - (c - t * ray.direction).LengthSquared;

            // No intersection in this case or for negative t
            if (redp2 < 0 || t < 0 || t * t < redp2) return null;
            // Take the first intersection point:
            t -= (float)Math.Sqrt(redp2);
            Vector3 location = ray.origin + (t * ray.direction);
            return new Intersection(location, t, this, (location - center).Normalized());
        }

        public override bool DoesIntersect(Ray ray, float maxValue)
        {
            if (!BB.boundsWith(ray)) return false;

            Vector3 c = center - ray.origin;
            float t = Vector3.Dot(c, ray.direction);
            float redp2 = radiusSq - (c - t * ray.direction).LengthSquared;

            // we can save a calculation of a square root by simplifying:
            // t - Math.sqrt(redp2) < maxvalue
            // t - maxvalue < Math.sqrt(redp2)
            // (t - maxvalue)^2 < redp2

            if (redp2 < 0 || t < 0 || t * t < redp2) return false;
            return t < maxValue || (t - maxValue) * (t - maxValue) < redp2;
        }
    }
}
