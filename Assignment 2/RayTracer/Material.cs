using OpenTK;
using System;

namespace RayTracer
{
    /// <summary>
    /// Material specifies what the primitives look like.
    /// </summary>
    public class Material
    {
        // The color when white light hits this object.
        public readonly Vector3 diffuseColor;
        /// <summary>
        /// Specularity specifies the portion of how 'reflective'/specular this object is.
        ///
        /// The hardness is a parameter to specify how fast the specular white
        /// of light goes away if we look from a different angle
        /// </summary>
        public readonly float specularity, hardness;
        /// <summary>
        /// isDiffuse is a boolean indicating if we should spend time on calculating the diffuse component of the color
        /// Same for isSpecular on calculating the specular component.
        /// </summary>
        public readonly bool isDiffuse, isSpecular;

        // For a purely diffuse object:
        public Material(Vector3 diffuseColor)
        {
            this.diffuseColor = diffuseColor;
            specularity = hardness = 0f;
            isDiffuse = true;
            isSpecular = false;
        }

        // If this is also a bit specular
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
