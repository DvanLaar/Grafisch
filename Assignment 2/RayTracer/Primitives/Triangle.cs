using OpenTK;
using System;

namespace RayTracer.Primitives
{
    class Triangle : Primitive
    {
        public Vector3 pos1, pos2, pos3;
        public Vector3 normal;

        public float distance;

        public Triangle(Vector3 pos1, Vector3 pos2, Vector3 pos3, Vector3 color, float diffuse) : base(color, diffuse)
        {
            this.pos1 = pos1;
            this.pos2 = pos2;
            this.pos3 = pos3;
            normal = Vector3.Cross(pos2 - pos1, pos3 - pos1).Normalized();
            distance = -Vector3.Dot(normal, pos1);
        }

        public override Intersection Intersect(Ray ray)
        {
            // In case the ray is parallel to the plane
            if (Math.Abs(Vector3.Dot(normal, ray.direction)) < Utils.SMALL_EPS)
                return null;

            float t = -(Vector3.Dot(ray.origin, normal) + distance) / Vector3.Dot(ray.direction, normal);
            if (t <= Utils.DIST_EPS)
                return null;

            // Calculate if in triangle
            float area = 0.5f * (Vector3.Cross(pos2 - pos1, pos3 - pos1)).Length;

            Vector3 edge1 = pos2 - pos1;
            Vector3 edge2 = pos3 - pos2;
            Vector3 edge3 = pos1 - pos3;

            Vector3 location = ray.origin + t * ray.direction;
            Vector3 p1 = location - pos1;
            Vector3 p2 = location - pos2;
            Vector3 p3 = location - pos3;

            if ((Vector3.Dot(normal, Vector3.Cross(edge1, p1)) < 0) ||
                (Vector3.Dot(normal, Vector3.Cross(edge2, p2)) < 0) ||
                (Vector3.Dot(normal, Vector3.Cross(edge3, p3)) < 0))
                return null;

            float alpha = 0.5f * (1 / area) * (Vector3.Cross(edge2, p2).Length);
            float beta = 0.5f * (1 / area) * (Vector3.Cross(edge3, p3).Length);

            Intersection intersect = new Intersection(location, t, this, normal);
            intersect.barycentric = new Vector3(alpha, beta, 1f - alpha - beta);
            return intersect;
        }

    }
}
