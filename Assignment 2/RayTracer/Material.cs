using OpenTK;
using System;

namespace RayTracer
{
    public class Material
    {
        public readonly Vector3 diffuseColor;
        public readonly float specularity, hardness;
        public readonly bool isDiffuse, isSpecular;

        public Material(Vector3 diffuseColor)
        {
            this.diffuseColor = diffuseColor;
            specularity = hardness = 0f;
            isDiffuse = true;
            isSpecular = false;
        }

        public Material(Vector3 diffuseColor, float specularity) : this(diffuseColor, specularity, 0f) { }

        public Material(Vector3 diffuseColor, float specularity, float hardness)
        {
            if (specularity < 0f || specularity > 1f) throw new ArgumentException("Specularity is not in range [0, 1]");

            this.diffuseColor = diffuseColor;
            this.specularity = specularity;
            this.hardness = hardness;
            isDiffuse = 1f - specularity > Utils.DIST_EPS;
            isSpecular = specularity > Utils.DIST_EPS;
        }
    }
}
