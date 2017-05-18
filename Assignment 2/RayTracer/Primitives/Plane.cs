using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer.Primitives
{
    class Plane : Primitive
    {

        public Vector3 normal;
        public float distance;


        public Plane(Vector3 normal, float distance, Vector3 color, float diffuse) : base(color, diffuse)
        {
            this.normal = normal;
            this.distance = distance;
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

            return new Intersection(t, this, normal);
        }
    }
}
