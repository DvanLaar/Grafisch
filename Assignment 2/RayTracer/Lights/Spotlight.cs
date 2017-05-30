using OpenTK;
using RayTracer.Primitives;
using System;

namespace RayTracer.Lights
{
    class Spotlight : Light
    {
        public float cosangle;
        public Vector3 direction;
        public readonly Vector3 position;

        public Spotlight(Vector3 position, Vector3 direction, float angle, Vector3 intensity):base(intensity)
        {
            this.position = position;
            this.direction = direction.Normalized();
            this.cosangle = (float)Math.Cos(angle);
        }

        public override Vector3 GetIntensity(Ray ray, Intersection intersection, Scene scene)
        {
            //Calculate vector from intersection point to light point
            Vector3 lightvec = this.position - intersection.location;
            //If the best effect of the lightsource on the intersectionpoint is less than half the least visible difference
            if (lightvec.LengthSquared > maxIntensity * 512)
                return Vector3.Zero;
            Vector3 lightnormal = Vector3.Normalize(lightvec);
            if (Vector3.Dot(-lightnormal, direction) < cosangle)
                return Vector3.Zero;

            float dot = Vector3.Dot(intersection.normal, lightnormal);
            if (dot <= 0f) return Vector3.Zero;

            // check the intersection as late as possible:
            if (scene.DoesIntersect(new Ray(intersection.location, lightnormal), lightvec.Length))
                return Vector3.Zero;

            if (ray.debug)
            {
                Ray debugray = new Ray(intersection.location, lightnormal);
                debugray.debugSurface = ray.debugSurface;
                debugray.camerapos = ray.camerapos;
                debugray.debugxunit = ray.debugxunit;
                debugray.debugyunit = ray.debugyunit;
                Intersection fakeintersection = new Intersection(this.position, 0, null, Vector3.UnitX);
                Raytracer.DrawRayDebug(debugray, fakeintersection, 0x0000ff);
            }
            return intensity * dot / lightvec.LengthSquared;
        }
    }
}
