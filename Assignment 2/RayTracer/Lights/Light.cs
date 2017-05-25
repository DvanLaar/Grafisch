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
        public readonly Vector3 intensity;

        public Light(Vector3 intensity)
        {

            this.intensity = intensity;
        }

        public virtual Vector3 GetIntensity(Ray ray, Intersection intersection, Scene scene)
        {
            return Utils.WHITE;
        }
    }
}
