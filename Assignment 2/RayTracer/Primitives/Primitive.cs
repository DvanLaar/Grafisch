using OpenTK;

namespace RayTracer.Primitives
{
    // Abstract class for a drawable object
    public abstract class Primitive
    {
        public Material material;

        public Primitive(Material material)
        {
            this.material = material;
        }

        // Returns the data associated with a intersection if the ray intersects the object, or else null
        public abstract Intersection Intersect(Ray ray);

        // Should return if Intersect(ray) != null and Intersect(ray).value < maxValue
        // But implementations do this in an other way to make it faster
        public abstract bool DoesIntersect(Ray ray, float maxValue);

        /// <summary>
        /// Determines color based on the ray and intersection
        /// </summary>
        public virtual Vector3 GetDiffuseColor(Intersection intersection)
        {
            return material.diffuseColor;
        }
    }
}
