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

        private readonly char[] paramSplit = new char[] { ' ' }, faceParamSplit = new char[] { '/' };

        private List<Vector2> texCoords;
        private List<Vector3> vertices, normals;

        private List<Mesh.ObjVertex> objVertices;
        private List<Mesh.ObjTriangle> objTriangles;
        private List<Mesh.ObjQuad> objQuads;

        private void Load(Mesh mesh, TextReader textReader)
        {
            vertices = new List<Vector3>();
            normals = new List<Vector3>();
            texCoords = new List<Vector2>();

            objVertices = new List<Mesh.ObjVertex>();
            objTriangles = new List<Mesh.ObjTriangle>();
            objQuads = new List<Mesh.ObjQuad>();

            string line;
            while ((line = textReader.ReadLine()) != null)
            {
                line = line.Trim(paramSplit).Replace("  ", " ");
                string[] parameters = line.Split(paramSplit);
                switch (parameters[0])
                {
                    case "v": // vertex
                        float x = float.Parse(parameters[1]);
                        float y = float.Parse(parameters[2]);
                        float z = float.Parse(parameters[3]);
                        vertices.Add(new Vector3(x, y, z));
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
                                Mesh.ObjTriangle objTriangle = new Mesh.ObjTriangle(
                                    ParseFaceParameter(parameters[1]),
                                    ParseFaceParameter(parameters[2]),
                                    ParseFaceParameter(parameters[3])
                                );
                                calculateTangents(objTriangle);
                                objTriangles.Add(objTriangle);
                                break;
                            case 5:
                                Mesh.ObjQuad objQuad = new Mesh.ObjQuad(
                                    ParseFaceParameter(parameters[1]),
                                    ParseFaceParameter(parameters[2]),
                                    ParseFaceParameter(parameters[3]),
                                    ParseFaceParameter(parameters[4])
                                );
                                objQuads.Add(objQuad);
                                break;
                        }
                        break;
                }
            }

            Console.WriteLine("Mesh loaded with #V=" + vertices.Count + ", #T=" + objTriangles.Count + ", #Q=" + objQuads.Count + ", #V2=" + objVertices.Count);

            mesh.vertices = objVertices.ToArray();
            mesh.triangles = objTriangles.ToArray();
            mesh.quads = objQuads.ToArray();
            vertices = null;
            normals = null;
            texCoords = null;
            objVertices = null;
            objTriangles = null;
            objQuads = null;
        }

        private int parseIndex(string indexS, int count)
        {
            int index = int.Parse(indexS);
            if (index < 0) return index + vertices.Count;
            // assert(index != 0);
            return index - 1;
        }

        private int ParseFaceParameter(string faceParameter)
        {
            Vector3 vertex = new Vector3();
            Vector2 texCoord = new Vector2();
            Vector3 normal = new Vector3();
            string[] parameters = faceParameter.Split(faceParamSplit);
            int vertexIndex = parseIndex(parameters[0], vertices.Count);
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

        private void calculateTangents(Mesh.ObjTriangle triangle)
        {
            Vector3 v0 = objVertices[triangle.Index0].Vertex;
            Vector3 v1 = objVertices[triangle.Index1].Vertex;
            Vector3 v2 = objVertices[triangle.Index2].Vertex;

            Vector2 uv0 = objVertices[triangle.Index0].TexCoord;
            Vector2 uv1 = objVertices[triangle.Index1].TexCoord;
            Vector2 uv2 = objVertices[triangle.Index2].TexCoord;

            // Overwrite the data, but now with a tangent
            objVertices[triangle.Index0] = new Mesh.ObjVertex(objVertices[triangle.Index0], ((uv2 - uv0).Y * (v1 - v0) - (uv1 - uv0).Y * (v2 - v0)).Normalized());
            objVertices[triangle.Index1] = new Mesh.ObjVertex(objVertices[triangle.Index1], ((uv0 - uv1).Y * (v2 - v1) - (uv2 - uv1).Y * (v0 - v1)).Normalized());
            objVertices[triangle.Index2] = new Mesh.ObjVertex(objVertices[triangle.Index2], ((uv1 - uv2).Y * (v0 - v2) - (uv0 - uv2).Y * (v1 - v2)).Normalized());
        }
    }

} // namespace Template_P3
