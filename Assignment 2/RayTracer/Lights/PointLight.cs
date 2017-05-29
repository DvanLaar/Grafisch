﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RayTracer.Primitives;

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
            //Calculate vector from intersection point to light point
            Vector3 lightvec = this.position - intersection.location;
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
