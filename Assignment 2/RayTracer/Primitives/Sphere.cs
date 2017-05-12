using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace template.Primitives
{
    public class Sphere : Primitive
    {
        public float radius;
        public Vector3 center;

        public Sphere(Vector3 center, float radius ,Vector3 color, float diffuse) : base(color,diffuse)
        {
            this.radius = radius;
            this.center = center;
        }

        public override Intersection Intersect(Ray ray)
        {
            Vector3 c = center - ray.origin;
            float t = Vector3.Dot(c, ray.direction);
            Vector3 q = c - t * ray.direction;
            float p2 = Vector3.Dot(q, q);
            if (p2 > this.radius * this.radius)
                return null;
            t -= (float)Math.Sqrt(this.radius * this.radius - p2);
            if (t>0)
            {
                Vector3 normal = (ray.origin + (t*ray.direction))-center;
                normal.Normalize();
                Intersection i = new Intersection(t,this,normal);
                return i;
            }
            return null;
        }
    }
}
