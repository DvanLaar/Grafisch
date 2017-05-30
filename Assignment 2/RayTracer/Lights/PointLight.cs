using OpenTK;
using RayTracer.Primitives;
using System;

namespace RayTracer.Lights
{
    class PointLight : Light
    {
        public readonly Vector3 position;

        public PointLight(Vector3 position, Vector3 intensity) : base(intensity)
        {
            this.position = position;
        }

        public override Vector3 GetIntensity(Ray ray, Intersection intersection, Scene scene)
        {
            float diff = 0f, spec = 0f;
            Material mat = intersection.primitive.material;
            bool visible = false;

            // Calculate vector from intersection point to light point
            Vector3 L = position - intersection.location;
            float dist = L.Length, distSq = L.LengthSquared;
            // If the best effect of the lightsource on the intersectionpoint is less than half the least visible difference
            if (distSq <= maxIntensity * 512)
            {
                L /= dist;
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
                        Intersection fakeintersection = new Intersection(this.position,0,null,Vector3.UnitX);
                        Raytracer.DrawRayDebug(debugray, fakeintersection, 0x0000ff);
                    }

                    // check the intersection as late as possible:
                    visible = !scene.DoesIntersect(new Ray(intersection.location, L), dist);
                    if (visible)
                    {
                        diff = NdotL / distSq;
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
                            visible = !scene.DoesIntersect(new Ray(intersection.location, L), dist);
                        if (visible)
                        {
                            spec = ((float)Math.Pow(NdotH, mat.hardness)) / distSq;
                        }
                    }
                }
            }
            Vector3 diffuse = intersection.primitive.GetDiffuseColor(intersection);
            return intensity * (diffuse * diff + Vector3.One * spec);
        }
    }
}
