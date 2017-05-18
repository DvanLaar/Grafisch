using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RayTracer.Primitives;

namespace RayTracer.Lights
{
    public class Light
    {

        public Vector3 intensity;

        public Light(Vector3 intensity)
        {

            this.intensity = intensity;
        }

        public virtual float GetIntensity(Ray ray, Intersection intersection, Scene scene)
        {
            return 1f;
        }
    }
}
