using OpenTK;

namespace RayTracer.Primitives
{
    public class Intersection
    {
        public float value;
        public Primitive primitive;
        public Vector3 location, normal;

        /**
         * This is only used when the intersection is with a triangle.
         * This contains the barycentric coordinates:
         * Suppose that the intersection is given by the location:
         * location = (1 - u - v) V_0 + u V_1 + v V_2,
         * where V_0, V_1, V_2 are the vertices of a triangle.
         * Then barycentric is Vector3f(1 - u - v, u, v)
         */
        public Vector3 barycentric;

        public Intersection(Vector3 location, float value, Primitive primitive, Vector3 normal)
        {
            this.location = location;
            this.value = value;
            this.primitive = primitive;
            this.normal = normal;
        }
    }
}
