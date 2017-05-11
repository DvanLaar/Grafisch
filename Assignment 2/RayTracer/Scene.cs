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
        public static final EPS = 1e-8;

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
    }
}
