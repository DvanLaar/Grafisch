using OpenTK;

namespace RayTracer
{
    public class Material
    {
        public readonly Vector3 diffuse, specular;
        public readonly float hardness;
        public readonly bool isDiffuse, isSpecular, isMirror;

        public Material(Vector3 diffuseColor, bool isMirror = false)
        {
            this.diffuse = diffuseColor;
            this.specular = Vector3.Zero;
            this.hardness = 0f;

            isDiffuse = diffuse.Length > Utils.DIST_EPS;
            isSpecular = false;
            this.isMirror = isMirror;
        }

        public Material(Vector3 diffuseColor, float hardness) :
            this(diffuseColor, Vector3.One * 0.25f, hardness)
        { }

        public Material(Vector3 diffuse, Vector3 specular, float hardness) :
            this(diffuse, specular, hardness, false)
        { }

        public Material(Vector3 diffuse, Vector3 specular, float hardness, bool isMirror)
        {
            this.diffuse = diffuse;
            this.specular = specular;
            this.hardness = hardness;

            isDiffuse = this.diffuse.Length > Utils.DIST_EPS;
            isSpecular = this.specular.Length > Utils.DIST_EPS;
            this.isMirror = isMirror;
        }
    }
}
