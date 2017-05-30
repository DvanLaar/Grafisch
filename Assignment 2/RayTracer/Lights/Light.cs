﻿using OpenTK;
using RayTracer.Primitives;
using System;

namespace RayTracer.Lights
{
    public class Light
    {
        public readonly Vector3 intensity;
        public readonly float maxIntensity;

        public Light(Vector3 intensity)
        {
            this.intensity = intensity;
            //Used to calculate a maximum distance the light has any major effect on
            maxIntensity = Math.Max(intensity.X, Math.Max(intensity.Y, intensity.Z));
        }

        public virtual Vector3 GetIntensity(Ray ray, Intersection intersection, Scene scene)
        {
            return intersection.primitive.GetDiffuseColor(intersection) * intensity;
        }
    }
}
