using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template.Primitives
{
    class NormalTriangle : Triangle
    {
        public Vector3 norm1, norm2, norm3;

        public NormalTriangle(Vector3 pos1, Vector3 pos2, Vector3 pos3, Vector3 norm1, Vector3 norm2, Vector3 norm3, Vector3 color, float diffuse) : base(pos1, pos2, pos3, color, diffuse)
        {
            this.norm1 = norm1;
            this.norm2 = norm2;
            this.norm3 = norm3;
        }

        public override Intersection Intersect(Ray ray)
        {
            Intersection intersection = base.Intersect(ray);
            if (intersection == null)
                return null;
            Vector3 interpolatednormal = (intersection.barycentric.X*norm1)+ (intersection.barycentric.Y * norm2) + (intersection.barycentric.Z * norm3);
            intersection.normal = interpolatednormal;
            return intersection;
        }
    }
}
