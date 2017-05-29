using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer.Primitives
{
    public struct BoundingBox
    {
        public float minX, maxX, minY, maxY, minZ, maxZ;

        public BoundingBox(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
            this.minZ = minZ;
            this.maxZ = maxZ;
        }

        public BoundingBox(IEnumerable<Vector3> vertices) : this(
            float.PositiveInfinity, float.NegativeInfinity,
            float.PositiveInfinity, float.NegativeInfinity,
            float.PositiveInfinity, float.NegativeInfinity)
        {
            foreach (Vector3 v in vertices)
            {
                if (v.X < minX) minX = v.X;
                if (v.X > maxX) maxX = v.X;
                if (v.Y < minY) minY = v.Y;
                if (v.Y > maxY) maxY = v.Y;
                if (v.Z < minZ) minZ = v.Z;
                if (v.Z > maxZ) maxZ = v.Z;
            }
        }

        public bool boundsWith(Ray ray)
        {
            float tmin = float.NegativeInfinity;
            float tmax = float.PositiveInfinity;
            if (ray.direction.X < -Utils.SMALL_EPS || ray.direction.X > Utils.SMALL_EPS)
            {
                float tx1 = (minX - ray.origin.X) / ray.direction.X;
                float tx2 = (maxX - ray.origin.X) / ray.direction.X;

                tmin = Math.Max(tmin, Math.Min(tx1, tx2));
                tmax = Math.Min(tmax, Math.Max(tx1, tx2));
            }
            if (-Utils.SMALL_EPS > ray.direction.Y || ray.direction.Y > Utils.SMALL_EPS)
            {
                float ty1 = (minY - ray.origin.Y) / ray.direction.Y;
                float ty2 = (maxY - ray.origin.Y) / ray.direction.Y;

                tmin = Math.Max(tmin, Math.Min(ty1, ty2));
                tmax = Math.Min(tmax, Math.Max(ty1, ty2));
            }
            if (-Utils.SMALL_EPS > ray.direction.Z || ray.direction.Z > Utils.SMALL_EPS)
            {
                float tz1 = (minZ - ray.origin.Z) / ray.direction.Z;
                float tz2 = (maxZ - ray.origin.Z) / ray.direction.Z;

                tmin = Math.Max(tmin, Math.Min(tz1, tz2));
                tmax = Math.Min(tmax, Math.Max(tz1, tz2));
            }

            // intersection is non-empty
            return 0 <= tmin && tmin <= tmax;
        }
    }
}
