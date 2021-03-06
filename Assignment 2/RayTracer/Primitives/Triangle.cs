﻿using OpenTK;
using System;

namespace RayTracer.Primitives
{
    // A triangle which is building block for a Mesh.
    class Triangle : Primitive
    {
        // The position of the firts vertex, and the direction to go to two other vertices.
        // The normal is perpendicular to edge1 and edge2.
        public readonly Vector3 pos1, edge1, edge2, normal;
        // Distance as if this would be a plane.
        public readonly float distance;
        public readonly BoundingBox BB;

        public Triangle(Vector3 pos1, Vector3 pos2, Vector3 pos3, Material material) : base(material)
        {
            this.pos1 = pos1;
            // only remember the relative position
            edge1 = pos2 - pos1;
            edge2 = pos3 - pos1;
            normal = Vector3.Cross(edge1, edge2).Normalized();
            distance = -Vector3.Dot(normal, pos1);
            BB = new BoundingBox(new Vector3[] { pos1, pos1 + edge1, pos1 + edge2 });
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
            if (-Utils.DIST_EPS < det && det < Utils.DIST_EPS) return null; // parallel
            float invdet = 1f / det;

            Vector3 tvec = ray.origin - pos1;
            float u = Vector3.Dot(tvec, pvec) * invdet;
            if (u < 0f || u > 1f) return null; // outside the triangle

            Vector3 qvec = Vector3.Cross(tvec, edge1);
            float v = Vector3.Dot(ray.direction, qvec) * invdet;
            if (v < 0f || u + v > 1f) return null; // outside the triangle

            float t = Vector3.Dot(edge2, qvec) * invdet;
            if (t <= Utils.DIST_EPS) return null; // intersection happens too fast!

            Vector3 newnormal = Vector3.Dot(ray.direction, normal) < 0 ? normal : -normal;
            Vector3 location = ray.origin + t * ray.direction;
            return new BarycentricIntersection(location, new Vector2(u, v), t, this, newnormal);
        }

        public override bool DoesIntersect(Ray ray, float maxValue)
        {
            // Stripped version of Intersect
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
            if (v < 0f || u + v > 1f) return false;

            float t = Vector3.Dot(edge2, qvec) * invdet;
            return Utils.DIST_EPS < t && t < maxValue;
        }
    }
}
