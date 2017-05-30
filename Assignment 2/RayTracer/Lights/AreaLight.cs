using OpenTK;
using RayTracer.Primitives;
using System;

namespace RayTracer.Lights
{
    class AreaLight : Light
    {
        public readonly Triangle triangle;

        public AreaLight(Triangle triangle, Vector3 intensity) : base(intensity)
        {
            this.triangle = triangle;
        }

        public override Vector3 GetIntensity(Ray ray, Intersection intersection, Scene scene)
        {
            // Calculates the average of pointlights by sampling random point sources
            // inside this triangle
            Vector3 sum = Vector3.Zero;
            for (int i = Utils.AREASAMPLES; i-- > 0;)
            {
                Vector3 samplepoint = GenerateSamplePoint();
                sum += Sample(samplepoint, intersection, scene);
            }

            return sum / Utils.AREASAMPLES;
        }

        private Vector3 GenerateSamplePoint()
        {

            //Generate random point on the triangle to sample from
            float u = Utils.RandomFloat(), v = Utils.RandomFloat();
            // uniform distribution for (u, v) in [0, 1)x[0, 1)

            if (u + v >= 1.0f)
            {
                u = 1.0f - u;
                v = 1.0f - v;
            }
            // now we have that same distribution but for u + v < 1 which is needed for a triangle
            return triangle.pos1 + u * triangle.edge1 + v * triangle.edge2;
            
        }

        /**
         * See PointLight.GetColor
         */
        private Vector3 Sample(Vector3 position, Intersection intersection, Scene scene)
        {
            //Calculate vector from intersection point to light point
            Vector3 lightvec = position - intersection.location;
            //If the best effect of the lightsource on the intersectionpoint is less than half the least visible difference
            if (lightvec.LengthSquared > maxIntensity * 512)
                return Vector3.Zero;
            Vector3 lightnormal = Vector3.Normalize(lightvec);

            float dot = Vector3.Dot(intersection.normal, lightnormal);
            if (dot <= 0f) return Vector3.Zero;

            // No sampling rays are drawn on the debug view to minimize the ammount of lines 

            // check the intersection as late as possible:
            if (scene.DoesIntersect(new Ray(intersection.location, lightnormal), lightvec.Length))
                return Vector3.Zero;
            return intensity * dot / lightvec.LengthSquared;
        }
    }
}
