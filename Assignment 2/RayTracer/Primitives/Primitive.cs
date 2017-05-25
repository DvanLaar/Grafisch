using OpenTK;

namespace RayTracer.Primitives
{
    public class Primitive
    {
        public Material material;

        public Primitive(Vector3 color, float diffuse)
        {
            this.material = new Material(color, diffuse);
        }

        public virtual Intersection Intersect(Ray ray)
        {
            return null;
        }

        /// <summary>
        /// Determines color based on the ray and intersection
        /// </summary>
        public virtual Vector3 GetColor(Intersection intersection = null)
        {
            return material.color;
        }

        /// <summary>
        /// Returns the color in int form
        /// </summary>
        public int GetColorInt()
        {
            return Utils.GetRGBValue(material.color);
        }
    }
}
