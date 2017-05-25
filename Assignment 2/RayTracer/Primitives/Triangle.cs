using OpenTK;
using System;

namespace RayTracer.Primitives
{
    class Triangle : Primitive
    {
        public Vector3 pos1, edge1, edge2;
        public Vector3 normal;

        public float distance;

        public Triangle(Vector3 pos1, Vector3 pos2, Vector3 pos3, Vector3 color, float diffuse) : base(color, diffuse)
        {
            this.pos1 = pos1;
            edge1 = pos2 - pos1;
            edge2 = pos3 - pos1;
            normal = Vector3.Cross(edge1, edge2).Normalized();
            distance = -Vector3.Dot(normal, pos1);
        }

        /**
         * Use the Möller-Trumbore intersection algorithm as explained in:
         * http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf
         */
        public override Intersection Intersect(Ray ray)
        {
            Vector3 pvec = Vector3.Cross(ray.direction, edge2);
            float det = Vector3.Dot(edge1, pvec);
            if (-Utils.DIST_EPS < det && det < Utils.DIST_EPS) return null;
            float invdet = 1f / det;

            Vector3 tvec = ray.origin - pos1;
            float u = Vector3.Dot(tvec, pvec) * invdet;
            if (u < 0f || u > 1f) return null;

            Vector3 qvec = Vector3.Cross(tvec, edge1);
            float v = Vector3.Dot(ray.direction, qvec) * invdet;
            if (v < 0f || u + v > 1f) return null;

            float t = Vector3.Dot(edge2, qvec) * invdet;

            Intersection intersection = new Intersection(ray.origin + t * ray.direction, t, this, normal);
            intersection.barycentric = new Vector3(1f - u - v, u, v);
            return intersection;
        }
    }
}
