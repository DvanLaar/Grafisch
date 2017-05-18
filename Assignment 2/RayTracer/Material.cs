using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RayTracer.Primitives;

namespace RayTracer
{
    public class Material
    {
        public Vector3 color;
        public float diffuse;
        public float specular;

        public Material(Vector3 color, float diffuse)
        {
            this.color = color;
            this.diffuse = diffuse;
            this.specular = 1f - diffuse;
        }

    }
}
