using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer.Primitives
{
    /**
     * Contains all the points X for which X * normal + distance = 0
     */
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
            float dot = Vector3.Dot(normal, ray.direction);
            // In case the ray is parallel to the plane
            if (-Utils.DIST_EPS < dot && dot < Utils.DIST_EPS) return null;

            float t = -(Vector3.Dot(ray.origin, normal) + distance) / dot;
            return t <= 0 ? null : new Intersection(ray.origin + t * ray.direction, t, this, normal);
        }
    }
}
