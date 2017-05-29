using OpenTK;

namespace RayTracer.Primitives
{
    class TexturedTriangle : Triangle
    {

        public Texture texture;
        public Vector2 tPos, tDirU, tDirV;

        public TexturedTriangle(Vector3 pos1, Vector3 pos2, Vector3 pos3,
            Texture texture, Vector2 tex1, Vector2 tex2, Vector2 tex3,
            Material material) : base(pos1, pos2, pos3, material)
        {
            this.texture = texture;
            tPos = tex1;
            tDirU = tex2 - tex1;
            tDirV = tex3 - tex1;
        }

        public override Vector3 GetDiffuseColor(Intersection intersection)
        {
            Vector2 barycord = ((BarycentricIntersection)intersection).barycentric;
            Vector2 texcord = tPos + barycord.X * tDirU + barycord.Y * tDirV;

            int tx = Utils.scaleFloat(texcord.X, texture.Width - 1);
            int ty = Utils.scaleFloat(texcord.Y, texture.Height - 1);
            return base.GetDiffuseColor(intersection) * texture.Data[tx, ty];
        }
    }
}
