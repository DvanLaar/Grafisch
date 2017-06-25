using System.IO;
using System.Collections.Generic;
using OpenTK;
using System;

namespace Template_P3
{
    // mesh and loader based on work by JTalton; http://www.opentk.com/node/642
    public class MeshLoader
    {
        public bool Load(Mesh mesh, string fileName)
        {
            try
            {
                using (StreamReader streamReader = new StreamReader(fileName))
                {
                    Load(mesh, streamReader);
                    streamReader.Close();
                    return true;
                }
            }
            catch { return false; }
        }

        public static bool averageTangents = true, divideByDet = true;

        private readonly char[] paramSplit = new char[] { ' ' }, faceParamSplit = new char[] { '/' };

        private List<Vector2> texCoords = new List<Vector2>();
        private List<Vector3> vertices = new List<Vector3>(), normals = new List<Vector3>(), tangentSum = new List<Vector3>();
        private List<List<int>> toObjVertices = new List<List<int>>();

        private List<Mesh.ObjVertex> objVertices = new List<Mesh.ObjVertex>();
        private List<Mesh.ObjTriangle> objTriangles = new List<Mesh.ObjTriangle>();
        private List<Mesh.ObjQuad> objQuads = new List<Mesh.ObjQuad>();

        private void Load(Mesh mesh, TextReader textReader)
        {
            string line;
            int lineNr = 0;
            while ((line = textReader.ReadLine()) != null)
            {
                lineNr++;
                line = line.Trim(paramSplit).Replace("  ", " ");
                string[] parameters = line.Split(paramSplit);
                switch (parameters[0])
                {
                    case "v": // vertex
                        float x = float.Parse(parameters[1]);
                        float y = float.Parse(parameters[2]);
                        float z = float.Parse(parameters[3]);
                        vertices.Add(new Vector3(x, y, z));
                        // additional data about tangents:
                        toObjVertices.Add(new List<int>());
                        tangentSum.Add(Vector3.Zero);
                        break;
                    case "vt": // texCoord
                        float u = float.Parse(parameters[1]);
                        float v = float.Parse(parameters[2]);
                        texCoords.Add(new Vector2(u, v));
                        break;
                    case "vn": // normal
                        float nx = float.Parse(parameters[1]);
                        float ny = float.Parse(parameters[2]);
                        float nz = float.Parse(parameters[3]);
                        normals.Add(new Vector3(nx, ny, nz));
                        break;
                    case "f":
                        switch (parameters.Length)
                        {
                            case 4:
                                int v0, i0 = ParseFaceParameter(parameters[1], out v0);
                                int v1, i1 = ParseFaceParameter(parameters[2], out v1);
                                int v2, i2 = ParseFaceParameter(parameters[3], out v2);
                                if (v0 == v1 || v0 == v2 || v1 == v2)
                                {
                                    objVertices.RemoveRange(objVertices.Count - 3, 3);
                                    // Console.WriteLine("Skipped invalid line " + lineNr);
                                    break;
                                }

                                calculateTangents(v0, i0, v1, i1, v2, i2);
                                Mesh.ObjTriangle objTriangle = new Mesh.ObjTriangle(i0, i1, i2);
                                objTriangles.Add(objTriangle);
                                break;
                            case 5:
                                int dummy;
                                Mesh.ObjQuad objQuad = new Mesh.ObjQuad(
                                    ParseFaceParameter(parameters[1], out dummy),
                                    ParseFaceParameter(parameters[2], out dummy),
                                    ParseFaceParameter(parameters[3], out dummy),
                                    ParseFaceParameter(parameters[4], out dummy)
                                );
                                objQuads.Add(objQuad);
                                break;
                        }
                        break;
                }
            }

            mesh.vertices = objVertices.ToArray();
            mesh.triangles = objTriangles.ToArray();
            mesh.quads = objQuads.ToArray();

            if (averageTangents)
            {
                for (int i = 0, N = vertices.Count; i < N; i++)
                {
                    if (toObjVertices[i].Count == 0) continue;
                    Vector3 avg = Vector3.Normalize(tangentSum[i] / toObjVertices[i].Count);
                    foreach (int to in toObjVertices[i])
                        mesh.vertices[to].Tangent = avg;

                    if (vertices.Count < 10)
                    {
                        Console.WriteLine(i + ": " + avg);
                    }
                }
            }

            Console.WriteLine("Mesh loaded with #V=" + vertices.Count + ", #T=" + objTriangles.Count + ", #Q=" + objQuads.Count + ", #V2=" + objVertices.Count);
            // RESET variables:
            vertices.Clear();
            normals.Clear();
            texCoords.Clear();
            tangentSum.Clear();
            toObjVertices.Clear();

            objVertices.Clear();
            objTriangles.Clear();
            objQuads.Clear();
        }

        private int parseIndex(string indexS, int count)
        {
            int index = int.Parse(indexS);
            if (index < 0) return index + count;
            // assert(index != 0);
            return index - 1;
        }

        private int ParseFaceParameter(string faceParameter, out int vertexIndex)
        {
            Vector3 vertex = new Vector3();
            Vector2 texCoord = new Vector2();
            Vector3 normal = new Vector3();
            string[] parameters = faceParameter.Split(faceParamSplit);
            vertexIndex = parseIndex(parameters[0], vertices.Count);
            vertex = vertices[vertexIndex];
            if (parameters.Length > 1 && parameters[1] != "")
            {
                int texCoordIndex = parseIndex(parameters[1], texCoords.Count);
                texCoord = texCoords[texCoordIndex];
            }
            if (parameters.Length > 2 && parameters[2] != "")
            {
                int normalIndex = parseIndex(parameters[2], normals.Count);
                normal = normals[normalIndex];
            }
            return AddObjVertex(ref vertex, ref texCoord, ref normal);
        }

        private int AddObjVertex(ref Vector3 vertex, ref Vector2 texCoord, ref Vector3 normal)
        {
            Mesh.ObjVertex newObjVertex = new Mesh.ObjVertex();
            newObjVertex.Vertex = vertex;
            newObjVertex.TexCoord = texCoord;
            newObjVertex.Normal = normal;
            objVertices.Add(newObjVertex);
            return objVertices.Count - 1;
        }

        private void calculateTangents(int v0, int i0, int v1, int i1, int v2, int i2)
        {
            Vector3 t0, t1, t2;
            getTangent(vertices[v0], vertices[v1], vertices[v2], objVertices[i0].TexCoord, objVertices[i1].TexCoord, objVertices[i2].TexCoord, out t0, out t1, out t2);
            if (averageTangents)
            {
                // Add the tangent to the sum, to calculate the average later on...
                tangentSum[v0] += t0;
                tangentSum[v1] += t1;
                tangentSum[v2] += t2;
                toObjVertices[v0].Add(i0);
                toObjVertices[v1].Add(i1);
                toObjVertices[v2].Add(i2);
            }
            else
            {
                objVertices[i0] = new Mesh.ObjVertex(objVertices[i0], t0);
                objVertices[i1] = new Mesh.ObjVertex(objVertices[i1], t1);
                objVertices[i2] = new Mesh.ObjVertex(objVertices[i2], t2);
            }
        }

        private static Vector3 getTangent(Vector3 e1, Vector3 e2, Vector2 duv1, Vector2 duv2)
        {
            return (duv2.Y * e1 - duv1.Y * e2) / (duv1.X * duv2.Y - duv2.X * duv1.Y);
        }

        public static void getTangent(Vector3 v0, Vector3 v1, Vector3 v2, Vector2 uv0, Vector2 uv1, Vector2 uv2, out Vector3 t0, out Vector3 t1, out Vector3 t2)
        {
            t0 = getTangent(v1 - v0, v2 - v0, uv1 - uv0, uv2 - uv0).Normalized();
            t1 = getTangent(v2 - v1, v0 - v1, uv2 - uv1, uv0 - uv1).Normalized();
            t2 = getTangent(v0 - v2, v1 - v2, uv0 - uv2, uv1 - uv2).Normalized();
        }
    }

} // namespace Template_P3
