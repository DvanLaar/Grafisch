﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template.Primitives
{
    class Triangle : Primitive
    {
        public Vector3 pos1, pos2, pos3;
        public Vector3 normal;

        public float distance;

        public Triangle(Vector3 pos1, Vector3 pos2, Vector3 pos3 ,Vector3 color): base(color)
        {
            this.pos1 = pos1;
            this.pos2 = pos2;
            this.pos3 = pos3;
            this.normal = Vector3.Cross(pos2 - pos1, pos3 - pos1).Normalized();
            distance = -Vector3.Dot(normal,pos1);
        }

        public override Intersection Intersect(Ray ray)
        {


            //In case the ray is paralell to the plane
            float epsilon = 0.0001f;
            if (Math.Abs(Vector3.Dot(normal, ray.direction)) < epsilon)
                return null;

            float t = -(
                (Vector3.Dot(ray.origin, normal) + distance) /
                (Vector3.Dot(ray.direction, normal)));
            if (t <= 0)
                return null;

            //Calculate if in triangle

            float area = 0.5f * (Vector3.Cross(pos2-pos1,pos3-pos1)).Length;

            Vector3 edge1 = pos2 - pos1;
            Vector3 edge2 = pos3 - pos2;
            Vector3 edge3 = pos1 - pos3;

            Vector3 intersectionpoint = ray.origin + t * ray.direction;

            Vector3 p1 = intersectionpoint - pos1;
            Vector3 p2 = intersectionpoint - pos2;
            Vector3 p3 = intersectionpoint - pos3;
            if ((Vector3.Dot(normal, Vector3.Cross(edge1, p1)) <0) ||
                (Vector3.Dot(normal, Vector3.Cross(edge2, p2)) < 0) ||
                (Vector3.Dot(normal, Vector3.Cross(edge3, p3)) < 0))
                return null;

            float alpha = 0.5f * (1 / area) * (Vector3.Cross(edge2, p2).Length);
            float beta  = 0.5f * (1 / area) * (Vector3.Cross(edge3, p3).Length);

            Intersection intersect = new Intersection(t, this, normal);
            intersect.barycentric = new Vector3(alpha,beta,1f-alpha-beta);
            return intersect;
        }

    }
}