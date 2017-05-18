using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RayTracer.Primitives;

namespace RayTracer.Lights
{
    class DirectionalLight : Light
    {
        public Vector3 direction;

        public DirectionalLight(Vector3 direction, Vector3 intensity) : base(intensity)
        {
            this.direction = direction;
        }

        public override float GetIntensity(Ray ray, Intersection intersection, Scene scene)
        {
            Vector3 normal = intersection.normal;
            Vector3 intersectionpos = ray.origin + (intersection.value * ray.direction);
            Vector3 lightnormal = Vector3.Normalize(-direction);
            if (!scene.HasIntersect(new Ray(intersectionpos, lightnormal)))
                return Vector3.Dot(normal, lightnormal);
            return 0f;
        }
    }
}
