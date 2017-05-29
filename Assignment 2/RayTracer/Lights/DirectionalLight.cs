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

        public DirectionalLight(Vector3 direction, Vector3 intensity, Vector3 location) : base(intensity, location)
        {
            this.direction = -direction.Normalized();
        }

        public override float GetIntensity(Ray ray, Intersection intersection, Scene scene)
        {
            if (scene.HasIntersect(new Ray(intersection.location, direction)))
                return 0f;
            return Math.Max(0f, Vector3.Dot(intersection.normal, direction));
        }
    }
}
