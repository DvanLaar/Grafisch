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
        public readonly Vector3 direction;

        public DirectionalLight(Vector3 direction, Vector3 intensity) : base(intensity)
        {
            // we are actually interested in the direction TO the light.
            this.direction = -direction.Normalized();
        }

        public override Vector3 GetIntensity(Ray ray, Intersection intersection, Scene scene)
        {
            if (scene.HasIntersect(new Ray(intersection.location, direction)))
                return new Vector3();
            float dot = Vector3.Dot(intersection.normal, direction);
            if (dot <= 0f) return new Vector3();
            return intensity * dot;
        }
    }
}
