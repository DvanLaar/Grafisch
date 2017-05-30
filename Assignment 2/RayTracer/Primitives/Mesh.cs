using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;

namespace RayTracer.Primitives
{
    // A 3D object which can be loaded from a .obj file
    class Mesh : Primitive
    {
        // All the vertices of the object
        private List<Vector3> vertices;
        // The bounding box to check if an intersection is inside this object.
        public readonly BoundingBox BB;
        // The list of all triangles (quads are not supported)
        public readonly List<Primitive> triangles;
        // The 'center' of the object, to which all positions from the .OBJ file are relative.
        private Vector3 position;

        public Mesh(string path, Vector3 position, float scale,
            Material material) : base(material)
        {
            // TODO: when a model is loaded, try to find the bounding box of the object, and after that, apply the affine transformation
            triangles = new List<Primitive>();
            this.position = position;

            // Load the mesh:
            vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            vertices.Add(Vector3.Zero);
            normals.Add(Vector3.Zero);

            string line;
            string[] split1, split2;

            StreamReader obj = new StreamReader(path);
            while ((line = obj.ReadLine()) != null)
            {
                split1 = line.Split(' ');
                switch (split1[0])
                {
                    case "v":
                        // read a vertex
                        vertices.Add(new Vector3(Utils.Parse(split1[1]), Utils.Parse(split1[2]), Utils.Parse(split1[3])) * scale + position);
                        break;
                    case "vn":
                        // read a vertex normal
                        normals.Add(new Vector3(Utils.Parse(split1[1]), Utils.Parse(split1[2]), Utils.Parse(split1[3])).Normalized());
                        break;
                    case "f":
                        // a face should consist out of 3 vertices
                        if (split1.Length == 5)
                        {
                            throw new Exception("The faces are not triangles, but quads!");
                        }
                        split2 = split1[1].Split('/');
                        int v1 = int.Parse(split2[0]), n1 = int.Parse(split2[2]);
                        split2 = split1[2].Split('/');
                        int v2 = int.Parse(split2[0]), n2 = int.Parse(split2[2]);
                        split2 = split1[3].Split('/');
                        int v3 = int.Parse(split2[0]), n3 = int.Parse(split2[2]);

                        triangles.Add(new NormalTriangle(
                            vertices[v1], vertices[v2], vertices[v3], normals[n1], normals[n2], normals[n3],
                            base.material));
                        break;
                }
            }
            BB = new BoundingBox(vertices);
        }

        public override Intersection Intersect(Ray ray)
        {
            if (!BB.boundsWith(ray)) return null;

            // Check ALL triangles (lots o' calculating, lots o' waiting)
            Intersection ret = null;
            // Return the intersection with the smallest t value
            foreach (var primitive in triangles)
            {
                Intersection alt = primitive.Intersect(ray);
                if (alt == null) continue;
                if (ret == null || (Utils.DIST_EPS < alt.value && alt.value < ret.value))
                    ret = alt;
            }
            return ret;
        }

        public override Vector3 GetDiffuseColor(Intersection intersection)
        {
            throw new Exception("The primitive should be called instead");
            // return intersection.primitive.GetColor(intersection);
        }

        public override bool DoesIntersect(Ray ray, float maxValue)
        {
            if (!BB.boundsWith(ray)) return false;

            // Check ALL triangles (lots o' calculating, lots o' waiting)
            foreach (var primitive in triangles)
            {
                if (primitive.DoesIntersect(ray, maxValue)) return true;
            }
            return false;
        }
    }
}
