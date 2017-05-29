using OpenTK;
using RayTracer.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer.Lights
{
    class AreaLight : Light
    {
        public Triangle triangle;

        public AreaLight(Triangle triangle, Vector3 intensity) : base(intensity)
        {
            this.triangle = triangle;
        }

        public override Vector3 GetIntensity(Ray ray, Intersection intersection, Scene scene)
        {
            Vector3 sum = Vector3.Zero;
            for (int i = 0; i < Utils.AREASAMPLES; i++)
            {
                sum += Sample(GenerateSamplePoint(),intersection,scene);
            }
            return sum / Utils.AREASAMPLES;
        }

        private Vector3 GenerateSamplePoint()
        {
            Vector3 point = new Vector3(Utils.RandomFloat(), Utils.RandomFloat(), Utils.RandomFloat());
            //Make sure sum of components is 1, to use this point as convex combination parameters
            point = point / (point.X + point.Y + point.Z);
            return (triangle.pos1*point.X)+((triangle.pos1+triangle.edge1)*point.Y)+((triangle.pos1 + triangle.edge2) * point.Z);
        }

        private Vector3 Sample(Vector3 samplepoint, Intersection intersection, Scene scene)
        {
            //Calculate vector from intersection point to light point
            Vector3 lightvec = samplepoint - intersection.location;
            //If the best effect of the lightsource on the intersectionpoint is less than half the least visible difference
            if (lightvec.LengthSquared > maxintensity * 510)
                return Vector3.Zero;
            Vector3 lightnormal = Vector3.Normalize(lightvec);
            if (scene.DoesIntersect(new Ray(intersection.location, lightnormal), lightvec.Length))
                return new Vector3();
            float dot = Vector3.Dot(intersection.normal, lightnormal);
            if (dot <= 0f) return new Vector3();
            return intensity * dot / lightvec.LengthSquared;
        }
    }
}
