using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RayTracer.Primitives;

namespace RayTracer.Lights
{
    class PointLight : Light
    {
        public Vector3 position;

        public PointLight(Vector3 position, Vector3 intensity) : base(intensity)
        {
            this.position = position;
        }

        public override float GetIntensity(Ray ray, Intersection intersection, Scene scene)
        {
            Vector3 lightvec = this.position - intersection.location;
            Vector3 lightnormal = Vector3.Normalize(lightvec);
            if (!scene.HasIntersectMax(new Ray(intersection.location, lightnormal), lightvec.Length))
                return Vector3.Dot(intersection.normal, lightnormal) * (1f / lightvec.LengthSquared);
            return 0f;
        }
    }
}
