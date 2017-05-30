using OpenTK;

namespace RayTracer.Primitives
{
    class NormalTriangle : Triangle
    {
        public Vector3 nPos, nDirU, nDirV;

        public NormalTriangle(Vector3 pos1, Vector3 pos2, Vector3 pos3,
            Vector3 norm1, Vector3 norm2, Vector3 norm3,
            Material material) : base(pos1, pos2, pos3, material)
        {
            nPos = norm1;
            nDirU = norm2 - norm1;
            nDirV = norm3 - norm1;
        }

        public override Intersection Intersect(Ray ray)
        {
            Intersection intersection = base.Intersect(ray);
            if (intersection == null)
                return null;

            // Construct an interpolated normal:
            Vector2 barycentric = ((BarycentricIntersection)intersection).barycentric;
            intersection.normal = nPos + barycentric.X * nDirU + barycentric.Y * nDirV;
            return intersection;
        }
    }
}
