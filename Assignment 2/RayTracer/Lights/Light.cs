using OpenTK;
using RayTracer.Primitives;
using System;

namespace RayTracer.Lights
{
    public class Light
    {
        public readonly Vector3 intensity;
        public readonly float maxintensity;

        public Light(Vector3 intensity)
        {
            this.intensity = intensity;
            maxintensity = intensity.X;
            maxintensity = Math.Max(maxintensity, intensity.Y);
            maxintensity = Math.Max(maxintensity, intensity.Z);
        }

        public virtual Vector3 GetIntensity(Ray ray, Intersection intersection, Scene scene)
        {
            return intensity;
        }
    }
}
