using OpenTK;
using System;

namespace RayTracer.Primitives
{
    class Quad : Primitive
    {
        public readonly Vector3 pos1, edge1, edge2, normal;
        public readonly float distance;
        public readonly BoundingBox BB;

        public Quad(Vector3 pos1, Vector3 edge1, Vector3 edge2, Vector3 color, float diffuse) : base(color, diffuse)
        {
            this.pos1 = pos1;
            this.edge1 = edge1;
            this.edge2 = edge2;
            normal = Vector3.Cross(edge1, edge2).Normalized();
            distance = -Vector3.Dot(normal, pos1);
            BB = new BoundingBox(new Vector3[] { this.pos1, this.pos1 + edge1, this.pos1 + edge2, this.pos1 + edge1 + edge2 });
        }

        /**
         * Use the Möller-Trumbore intersection algorithm as explained in:
         * http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf
         */
        public override Intersection Intersect(Ray ray)
        {
            if (!BB.boundsWith(ray)) return null;

            Vector3 pvec = Vector3.Cross(ray.direction, edge2);
            float det = Vector3.Dot(edge1, pvec);
            if (-Utils.DIST_EPS < det && det < Utils.DIST_EPS) return null;
            float invdet = 1f / det;

            Vector3 tvec = ray.origin - pos1;
            float u = Vector3.Dot(tvec, pvec) * invdet;
            if (u < 0f || u > 1f) return null;

            Vector3 qvec = Vector3.Cross(tvec, edge1);
            float v = Vector3.Dot(ray.direction, qvec) * invdet;
            if (v < 0f || v > 1f) return null;

            float t = Vector3.Dot(edge2, qvec) * invdet;
            if (t <= Utils.DIST_EPS) return null;

            Vector3 newnormal = Vector3.Dot(ray.direction, normal) < 0 ? normal : -normal;
            Vector3 location = ray.origin + t * ray.direction;
            return new BarycentricIntersection(location, new Vector2(u, v), t, this, newnormal);
        }

        public override bool DoesIntersect(Ray ray, float maxValue)
        {
            if (!BB.boundsWith(ray)) return false;

            Vector3 pvec = Vector3.Cross(ray.direction, edge2);
            float det = Vector3.Dot(edge1, pvec);
            if (-Utils.DIST_EPS < det && det < Utils.DIST_EPS) return false;
            float invdet = 1f / det;

            Vector3 tvec = ray.origin - pos1;
            float u = Vector3.Dot(tvec, pvec) * invdet;
            if (u < 0f || u > 1f) return false;

            Vector3 qvec = Vector3.Cross(tvec, edge1);
            float v = Vector3.Dot(ray.direction, qvec) * invdet;
            if (v < 0f || v > 1f) return false;

            float t = Vector3.Dot(edge2, qvec) * invdet;
            return Utils.DIST_EPS < t && t < maxValue;
        }
    }
}
