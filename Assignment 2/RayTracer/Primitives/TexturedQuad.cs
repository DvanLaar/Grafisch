using OpenTK;

namespace RayTracer.Primitives
{
    class TexturedQuad : Quad
    {
        public Texture texture;
        public Vector2 tPos, tDirU, tDirV;

        public TexturedQuad(Vector3 pos1, Vector3 edge1, Vector3 edge2,
            Texture texture, Material material)
                : this(pos1, edge1, edge2, texture, new Vector2(0f, 1f), new Vector2(1f, 0f), new Vector2(0f, -1f), material) { }

        public TexturedQuad(Vector3 pos1, Vector3 edge1, Vector3 edge2,
            Texture texture, Vector2 tPos, Vector2 tDirU, Vector2 tDirV,
            Material material)
                : base(pos1, edge1, edge2, material)
        {
            this.texture = texture;
            this.tPos = tPos;
            this.tDirU = tDirU;
            this.tDirV = tDirV;
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
