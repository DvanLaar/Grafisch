using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template;

namespace RayTracer
{
    public struct Ray
    {
        public Vector3 origin, direction;
        public bool debug;
        public Surface debugSurface;
        public Vector3 camerapos;
        public Vector3 debugxunit, debugyunit;

        public Ray(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction;
            debug = false;
            debugSurface = null;
            camerapos = Vector3.Zero;
            debugxunit = Vector3.Zero;
            debugyunit = Vector3.Zero;
        }
    }
}
