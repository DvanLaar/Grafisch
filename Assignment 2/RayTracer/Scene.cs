using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using template.Lights;
using template.Primitives;

namespace template
{
    public class Scene
    {
        public static float EPS = 1e-4f;

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

        //Used for primary and secondary rays
        public Intersection Intersect(Ray ray)
        {
            Intersection intersect = null;
            foreach(Primitive primitive in primitives)
            {
                Intersection inters = primitive.Intersect(ray);
                if (inters == null) continue;
                if (intersect == null || (EPS < inters.value && inters.value < intersect.value))
                    intersect = inters;
            }
            return intersect;
        }

        //Used for non-directional light
        public bool HasIntersectMax(Ray ray, float maxdistance)
        {
            foreach (Primitive primitive in primitives)
            {
                Intersection intersect = primitive.Intersect(ray);
                if (intersect != null && EPS < intersect.value && intersect.value < maxdistance - EPS)
                    return true;
            }
            return false;
        }

        //Used for directional light
        public bool HasIntersect(Ray ray)
        {
            foreach (Primitive primitive in primitives)
            {
                Intersection intersect = primitive.Intersect(ray);
                if (intersect != null && EPS < intersect.value)
                    return true;
            }
            return false;
        }

    }
}
