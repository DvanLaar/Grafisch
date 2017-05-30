using OpenTK;
using RayTracer.Primitives;
using System;

namespace RayTracer.Lights
{
    class DirectionalLight : Light
    {
        // The direction in which you have to look to 'see' this light (if it was drawn)
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

            float NdotL = Vector3.Dot(intersection.normal, L);
            // Is the light direction on the same side as the normal?
            if (NdotL > 0f)
            {
                // check the intersection as late as possible:
                visible = !scene.DoesIntersect(new Ray(intersection.location, L), float.PositiveInfinity);
                if (visible)
                {
                    diff = NdotL;

                    if (ray.debug)
                    {
                        // Add a shadow ray to the debug screen
                        Ray debugray = new Ray(intersection.location, L);
                        debugray.debugSurface = ray.debugSurface;
                        debugray.camerapos = ray.camerapos;
                        debugray.debugxunit = ray.debugxunit;
                        debugray.debugyunit = ray.debugyunit;
                        Raytracer.DrawRayDebug(debugray, null, 0x0000ff);
                    }
                }
            }

            // Add some specularity
            if (mat.isSpecular)
            {
                Vector3 viewDir = -ray.direction;
                Vector3 halfway = (L + viewDir).Normalized();
                float NdotH = Vector3.Dot(intersection.normal, halfway);
                // Are we on the good side of the normal?
                if (NdotH > 0f)
                {
                    // Check if this light is visible from the intersection (calculate if not calculated already)
                    if (NdotL <= 0f)
                        visible = !scene.DoesIntersect(new Ray(intersection.location, L), float.PositiveInfinity);
                    if (visible)
                    {
                        // Use a power law to let this hardness have on effect on the decay of intensity of the white effect
                        spec = (float)Math.Pow(NdotH, mat.hardness);
                    }
                }
            }
            // mix the colors, specularity (from light, not reflection) is always white.
            Vector3 diffuse = intersection.primitive.GetDiffuseColor(intersection);
            return intensity * (diffuse * diff + Vector3.One * spec);
        }
    }
}
