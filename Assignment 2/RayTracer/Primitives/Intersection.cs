using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template.Primitives
{
    public class Intersection
    {
        public float value;
        public Primitive primitive;
        public Vector3 normal;

        //Only used with triangles
        public Vector3 barycentric;

        public Intersection(float value, Primitive primitive, Vector3 normal)
        {
            this.value = value;
            this.primitive = primitive;
            this.normal = normal;
        }

    }
}
