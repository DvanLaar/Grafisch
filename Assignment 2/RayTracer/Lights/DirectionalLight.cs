using OpenTK;
using RayTracer.Primitives;
using System;

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
            float diff = 0f, spec = 0f;
            Material mat = intersection.primitive.material;
            bool visible = false;

            // Calculate vector from intersection point to light point
            Vector3 L = direction;
            // If the best effect of the lightsource on the intersectionpoint is less than half the least visible difference

            float NdotL = Vector3.Dot(intersection.normal, L);
            if (NdotL > 0f)
            {
                if (ray.debug)
                {
                    Ray debugray = new Ray(intersection.location, L);
                    debugray.debugSurface = ray.debugSurface;
                    debugray.camerapos = ray.camerapos;
                    debugray.debugxunit = ray.debugxunit;
                    debugray.debugyunit = ray.debugyunit;
                    Raytracer.DrawRayDebug(debugray, null,0x0000ff);
                }

                // check the intersection as late as possible:
                visible = !scene.DoesIntersect(new Ray(intersection.location, L), float.PositiveInfinity);
                if (visible)
                {
                    diff = NdotL;
                }
            }

            if (mat.isSpecular)
            {
                Vector3 viewDir = -ray.direction;
                Vector3 halfway = (L + viewDir).Normalized();
                float NdotH = Vector3.Dot(intersection.normal, halfway);
                if (NdotH > 0f)
                {
                    if (NdotL <= 0f)
                        visible = !scene.DoesIntersect(new Ray(intersection.location, L), float.PositiveInfinity);
                    if (visible)
                    {
                        spec = (float) Math.Pow(NdotH, mat.hardness);
                        // Console.WriteLine("specularity: " + spec);
                    }
                }
            }
            Vector3 diffuse = intersection.primitive.GetDiffuseColor(intersection);
            return intensity * (diffuse * diff + mat.specular * spec);
        }
    }
}
