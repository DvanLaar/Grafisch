using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer.Primitives
{
    public class Intersection
    {
        public float value;
        public Primitive primitive;
        public Vector3 location, normal;

        // Only used with triangles
        public Vector3 barycentric;

        public Intersection(Vector3 location, float value, Primitive primitive, Vector3 normal)
        {
            this.location = location;
            this.value = value;
            this.primitive = primitive;
            this.normal = normal;
        }
    }
}
