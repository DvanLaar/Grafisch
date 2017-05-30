using OpenTK;

namespace RayTracer.Primitives
{
    /**
     * Contains all the points X for which X * normal + distance = 0
     * does not use a bounding box
     */
    class Plane : Primitive
    {
        public Vector3 normal;
        public float distance;
        
        public Plane(Vector3 normal, float distance, Material material) : base(material)
        {
            this.normal = normal;
            this.distance = distance;
        }

        public override Intersection Intersect(Ray ray)
        {
            float dot = Vector3.Dot(normal, ray.direction);
            // In case the ray is parallel to the plane
            if (-Utils.DIST_EPS < dot && dot < Utils.DIST_EPS) return null;

            float dotOrigin = Vector3.Dot(ray.origin, normal);
            float t = -(dotOrigin + distance) / dot;
            // Return the opposite normal if we intersect from the other side
            Vector3 intersectionNormal = dotOrigin < 0f ? -normal : normal;
            return t <= 0 ? null : new Intersection(ray.origin + t * ray.direction, t, this, intersectionNormal);
        }

        public override bool DoesIntersect(Ray ray, float maxValue)
        {
            float dot = Vector3.Dot(normal, ray.direction);
            // In case the ray is parallel to the plane
            if (-Utils.DIST_EPS < dot && dot < Utils.DIST_EPS) return false;

            float t = -(Vector3.Dot(ray.origin, normal) + distance) / dot;
            return Utils.DIST_EPS < t && t < maxValue;
        }
    }
}
