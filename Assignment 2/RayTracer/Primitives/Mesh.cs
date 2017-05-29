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
        public static float EPS = 1e-4f;
        public List<NormalTriangle> triangles;
        private float radius;
        private Vector3 position;

        public Mesh(string path, Vector3 color, Vector3 position, float scale, float diffuse) : base(color, diffuse)
        {
            triangles = new List<NormalTriangle>();
            this.position = position;
            this.radius = scale * 1.1f;
            loadMesh(path, position, scale);
        }

        private void loadMesh(string path, Vector3 position, float scale)
        {
            List<Vector3> vertices = new List<Vector3>();
            vertices.Add(Vector3.Zero);
            List<Vector3> normals = new List<Vector3>();
            normals.Add(Vector3.Zero);

            string line;
            string[] split1;
            string[] split2;

            StreamReader obj = new StreamReader(path);
            while ((line = obj.ReadLine()) != null)
            {
                split1 = line.Split(' ');
                switch (split1[0])
                {
                    case "v":
                        vertices.Add(new Vector3(float.Parse(split1[1]), -float.Parse(split1[2]), float.Parse(split1[3])));
                        break;
                    case "vn":
                        normals.Add(Vector3.Normalize(new Vector3(float.Parse(split1[1]), -float.Parse(split1[2]), float.Parse(split1[3]))));
                        break;
                    case "f":
                        split2 = split1[1].Split('/');
                        int v1 = int.Parse(split2[0]);
                        int n1 = int.Parse(split2[2]);
                        split2 = split1[2].Split('/');
                        int v2 = int.Parse(split2[0]);
                        int n2 = int.Parse(split2[2]);
                        split2 = split1[3].Split('/');
                        int v3 = int.Parse(split2[0]);
                        int n3 = int.Parse(split2[2]);
                        triangles.Add(new NormalTriangle(
                            scale * vertices[v1] + position, scale * vertices[v2] + position, scale * vertices[v3] + position,
                            normals[n1], normals[n2], normals[n3],
                            this.material.color, this.material.diffuse));
                        break;
                }
            }
        }

        public override Intersection Intersect(Ray ray)
        {
            //Check if ray hits a sphere around to save triangle checks
            Vector3 c = position - ray.origin;
            float t = Vector3.Dot(c, ray.direction);
            Vector3 q = c - t * ray.direction;
            float p2 = Vector3.Dot(q, q);
            if (p2 > this.radius * this.radius)
                return null;
            t -= (float)Math.Sqrt(this.radius * this.radius - p2);
            if (t <= 0)
                return null;

            //Check ALL triangles (lots o' calculating, lots o' waiting)
            Intersection intersect = null;
            foreach (Primitive primitive in triangles)
            {
                Intersection inters = primitive.Intersect(ray);
                if (inters == null) continue;
                if (intersect == null || (EPS < inters.value && inters.value < intersect.value))
                    intersect = inters;
            }
            return intersect;
        }

        public override Vector3 getNormal(Vector3 posOnPrim)
        {
            return new Vector3();
        }

        public override void Debug() { }
    }
}
