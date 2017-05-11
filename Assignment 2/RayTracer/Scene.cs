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
            if (primitives.Count == 0)
                return null;
            Intersection intersect = null;
            foreach(Primitive primitive in primitives)
            {
                Intersection inters = primitive.Intersect(ray);
                if (intersect == null)
                    intersect = inters;
                else if (inters == null)
                    continue;
                else if (inters.value < intersect.value)
                    intersect = inters;
            }
            return intersect;
        }
    }
}
