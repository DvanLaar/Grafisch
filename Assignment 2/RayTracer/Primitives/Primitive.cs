using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template.Primitives
{
    public class Primitive
    {
        public Material material;

        public Primitive(Vector3 color, float diffuse)
        {
            this.material = new Material(color,diffuse);
        }

        public virtual Intersection Intersect(Ray ray)
        {
            return null;
        }

        /// <summary>
        /// Determines color based on the ray and intersection
        /// </summary>
        public virtual Vector3 GetColor(Ray ray = null, Intersection intersection = null)
        {
            return material.color;
        }

        /// <summary>
        /// Returns the color in int form
        /// </summary>
        public int GetColorInt()
        {
            int r = (int)MathHelper.Clamp(material.color.X * 255f, 0, 255f);
            int g = (int)MathHelper.Clamp(material.color.Y * 255f, 0, 255f);
            int b = (int)MathHelper.Clamp(material.color.Z * 255f, 0, 255f);
            return (r<<16) + (g<<8) + b;
        }
    }
}
