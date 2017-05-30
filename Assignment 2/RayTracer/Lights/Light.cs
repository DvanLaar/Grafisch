using OpenTK;
using RayTracer.Primitives;
using System;

namespace RayTracer.Lights
{
    public class Light
    {
        // Color of the light
        public readonly Vector3 intensity;
        // Maximum value of all components
        public readonly float maxIntensity;

        public Light(Vector3 intensity)
        {
            this.intensity = intensity;
            //Used to calculate a maximum distance the light has any major effect on
            maxIntensity = Math.Max(intensity.X, Math.Max(intensity.Y, intensity.Z));
        }

        public virtual Vector3 GetIntensity(Ray ray, Intersection intersection, Scene scene)
        {
            // Take the product of the diffuse color of the surface it intersected with and the color of the light.
            return intersection.primitive.GetDiffuseColor(intersection) * intensity;
        }
    }
}
