using OpenTK;

namespace RayTracer.Primitives
{
    class TexturedQuad : Quad
    {
        public Texture texture;
        public Vector2 tPos, tDirU, tDirV;

        public TexturedQuad(Vector3 pos1, Vector3 edge1, Vector3 edge2,
            Texture texture, Vector2 tPos, Vector2 tDirU, Vector2 tDirV,
            Vector3 color, float diffuse)
                : base(pos1, edge1, edge2, color, diffuse)
        {
            this.texture = texture;
            this.tPos = tPos;
            this.tDirU = tDirU;
            this.tDirV = tDirV;
        }

        public override Vector3 GetColor(Intersection intersection)
        {
            Vector2 barycord = ((BarycentricIntersection)intersection).barycentric;
            Vector2 texcord = tPos + barycord.X * tDirU + barycord.Y * tDirV;

            int tx = Utils.scaleFloat(texcord.X, texture.Width - 1);
            int ty = Utils.scaleFloat(texcord.Y, texture.Height - 1);
            return base.GetColor(intersection) * texture.Data[tx, ty];
        }
    }
}
