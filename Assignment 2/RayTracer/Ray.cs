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
        //Debug parameters to use for drawing shadow rays
        public bool debug;
        public Surface debugSurface; // The surface on which we draw pixels for the debug view
        public Vector3 camerapos; // The position of the camera.
        /// <summary>
        /// This are the directions in 3D space if we move in the debug screen in X and Y direction.
        /// </summary>
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
