using OpenTK;

namespace RayTracer.Primitives
{
    public class Intersection
    {
        public float value;
        public Primitive primitive;
        public Vector3 location, normal;

        public Intersection(Vector3 location, float value, Primitive primitive, Vector3 normal)
        {
            this.location = location;
            this.value = value;
            this.primitive = primitive;
            this.normal = normal;
        }
    }

    public class BarycentricIntersection : Intersection
    {
        /**
         * This is only used when the intersection is with a triangle.
         * This contains the barycentric coordinates:
         * Suppose that the intersection is given by the location:
         * location = V_0 + u dV_1 + v dV_2,
         * where V_0, dV_1, dV_2 are the vertices of a triangle.
         * Then barycentric is Vector2(u, v)
         */
        public readonly Vector2 barycentric;

        public BarycentricIntersection(Vector3 location, Vector2 barycentric, float value, Primitive primitive, Vector3 normal) : base(location, value, primitive, normal)
        {
            this.barycentric = barycentric;
        }
    }
}
