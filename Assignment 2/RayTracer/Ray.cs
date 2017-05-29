using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer
{
    public class Ray
    {
        public Vector3 origin, direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        public Vector3 mirror(Vector3 normal)
        {
            Vector3 hulpVect = Vector3.Dot(direction, normal) * normal;
            return direction - 2 * hulpVect;
        }
    }
}
