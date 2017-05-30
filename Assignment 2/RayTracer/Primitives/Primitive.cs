using OpenTK;

namespace RayTracer.Primitives
{
    public abstract class Primitive
    {
        public Material material;

        public Primitive(Material material)
        {
            this.material = material;
        }

        public abstract Intersection Intersect(Ray ray);
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
