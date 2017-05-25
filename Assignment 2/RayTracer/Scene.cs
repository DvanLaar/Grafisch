using System.Collections.Generic;
using RayTracer.Lights;
using RayTracer.Primitives;

namespace RayTracer
{
    public class Scene
    {
        public List<Primitive> primitives;
        public List<Light> lights;

        public Scene()
        {
            primitives = new List<Primitive>();
            lights = new List<Light>();
        }

        public void AddPrimitive(Primitive primitive)
        {
            primitives.Add(primitive);
        }

        public void AddLight(Light light)
        {
            lights.Add(light);
        }

        /** Used for primary and secondary rays to find an object which this ray hits.
         */
        public Intersection Intersect(Ray ray)
        {
            Intersection ret = null;
            foreach (Primitive primitive in primitives)
            {
                Intersection i = primitive.Intersect(ray);
                if (i != null && (ret == null || (Utils.DIST_EPS < i.value && i.value < ret.value)))
                    ret = i;
            }
            return ret;
        }

        /** Used for directional and non-directional light because this is faster than Intersect since it stops when it hits something
         */
        public bool DoesIntersect(Ray ray, float maxdistance)
        {
            foreach (Primitive primitive in primitives)
                if (primitive.DoesIntersect(ray, maxdistance)) return true;
            return false;
        }
    }
}
