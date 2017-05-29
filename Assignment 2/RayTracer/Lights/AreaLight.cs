using OpenTK;
using RayTracer.Primitives;

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
            Vector3 sum = Vector3.Zero;
            for (int i = Utils.AREASAMPLES; i-- > 0;)
            {
                sum += Sample(GenerateSamplePoint(), intersection, scene);
            }
            return sum / Utils.AREASAMPLES;
        }

        private Vector3 GenerateSamplePoint()
        {
            float u = Utils.RandomFloat(), v = Utils.RandomFloat();
            if (u + v >= 1.0f)
            {
                u = 1.0f - u;
                v = 1.0f - v;
            }
            return triangle.pos1 + u * triangle.edge1 + v * triangle.edge2;
        }

        private Vector3 Sample(Vector3 samplepoint, Intersection intersection, Scene scene)
        {
            //Calculate vector from intersection point to light point
            Vector3 lightvec = samplepoint - intersection.location;
            //If the best effect of the lightsource on the intersectionpoint is less than half the least visible difference
            if (lightvec.LengthSquared > maxIntensity * 512)
                return Vector3.Zero;
            Vector3 lightnormal = Vector3.Normalize(lightvec);

            float dot = Vector3.Dot(intersection.normal, lightnormal);
            if (dot <= 0f) return Vector3.Zero;

            // check the intersection as late as possible:
            if (scene.DoesIntersect(new Ray(intersection.location, lightnormal), lightvec.Length))
                return Vector3.Zero;
            return intensity * dot / lightvec.LengthSquared;
        }
    }
}
