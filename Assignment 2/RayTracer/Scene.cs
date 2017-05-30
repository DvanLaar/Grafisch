using System.Collections.Generic;
using RayTracer.Lights;
using RayTracer.Primitives;

namespace RayTracer
{
    public class Scene
    {
        /// <summary>
        /// A list of all objects which are present in the scene
        /// </summary>
        public List<Primitive> primitives;
        /// <summary>
        /// A list of all objects which cast light on objects
        /// </summary>
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
            // Return the intersection with the lowest t value, higher than the epsilon value.
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
            // Returns the first intersection to make things faster
            foreach (Primitive primitive in primitives)
                if (primitive.DoesIntersect(ray, maxdistance)) return true;
            return false;
        }
    }
}
