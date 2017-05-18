using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
{
    public class Ray
    {

        public Vector3 origin;
        public Vector3 direction;

        public int depth;

        public Ray(Vector3 origin, Vector3 direction, int depth = 0)
        {
            this.origin = origin;
            this.direction = direction;
            this.depth = depth;
        }
    }
}
