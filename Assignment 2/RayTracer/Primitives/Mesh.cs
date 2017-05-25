using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RayTracer.Primitives
{
    class Mesh : Primitive
    {
        public List<Primitive> triangles;
        private float radius;
        private Vector3 position;

        public Mesh(string path, Vector3 color, Vector3 position, float scale, float diffuse) : base(color, diffuse)
        {
            // TODO: when a model is loaded, try to find the bounding box of the object, and after that, apply the affine transformation
            triangles = new List<Primitive>();
            this.position = position;
            radius = scale * 1.74f; // Math.sqrt(3)

            // Load the mesh:
            List<Vector3> vertices = new List<Vector3>();
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
                        vertices.Add(new Vector3(Utils.Parse(split1[1]), Utils.Parse(split1[2]), Utils.Parse(split1[3])) * scale + position);
                        break;
                    case "vn":
                        normals.Add(new Vector3(Utils.Parse(split1[1]), Utils.Parse(split1[2]), Utils.Parse(split1[3])).Normalized());
                        break;
                    case "f":
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
                            material.color, material.diffuse));
                        break;
                }
            }
        }

        public override Intersection Intersect(Ray ray)
        {
            // Check if ray hits a sphere around to save triangle checks
            Vector3 c = position - ray.origin;
            float t = Vector3.Dot(c, ray.direction);
            float redp2 = radius * radius - (c - t * ray.direction).LengthSquared;
            if (redp2 < 0 || t < 0 || t * t < redp2) return null;

            // Check ALL triangles (lots o' calculating, lots o' waiting)
            Intersection intersect = null;
            foreach (var primitive in triangles)
            {
                Intersection inters = primitive.Intersect(ray);
                if (inters == null) continue;
                if (intersect == null || (Utils.DIST_EPS < inters.value && inters.value < intersect.value))
                    intersect = inters;
            }
            return intersect;
        }

        public override Vector3 GetColor(Intersection intersection)
        {
            throw new Exception("The primitive should be called instead");
            // return intersection.primitive.GetColor(intersection);
        }

        public override bool DoesIntersect(Ray ray, float maxValue)
        {
            // Check if ray hits a sphere around to save triangle checks
            Vector3 c = position - ray.origin;
            float t = Vector3.Dot(c, ray.direction);
            float redp2 = radius * radius - (c - t * ray.direction).LengthSquared;
            if (redp2 < 0 || t < 0 || t * t < redp2) return false;

            // Check ALL triangles (lots o' calculating, lots o' waiting)
            foreach (var primitive in triangles)
            {
                if (primitive.DoesIntersect(ray, maxValue)) return true;
            }
            return false;
        }
    }
}
