using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace rasterizer
{
    /// <summary>
    /// mesh and loader based on work by JTalton; http://www.opentk.com/node/642
    /// </summary>
    public class Mesh
    {
        // data members
        public ObjVertex[] vertices; // vertex positions, model space
        public ObjTriangle[] triangles; // triangles (3 vertex indices)
        public ObjQuad[] quads; // quads (4 vertex indices)

        /// <summary>
        /// The buffers belonging to these IDs contain all the data from above, in a format more useful to the GPU
        /// </summary>
        private int bufferVertices, bufferTriangles, bufferQuads;

        /// <summary>
        /// Loads all the meshes
        /// </summary>
        private static MeshLoader meshLoader = new MeshLoader();

        public Mesh() { }

        // constructor
        public Mesh(string fileName)
        {
            meshLoader.Load(this, fileName);
        }

        // initialization; called during first render
        public void Prepare(Shader shader)
        {
            if (bufferVertices != 0) return; // already taken care of

            // generate interleaved vertex data (uv/normal/position (total 8 floats) per vertex)
            GL.GenBuffers(1, out bufferVertices);
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferVertices);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * Marshal.SizeOf(typeof(ObjVertex))), vertices, BufferUsageHint.StaticDraw);

            // generate triangle index array
            GL.GenBuffers(1, out bufferTriangles);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufferTriangles);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(triangles.Length * Marshal.SizeOf(typeof(ObjTriangle))), triangles, BufferUsageHint.StaticDraw);

            // generate quad index array
            GL.GenBuffers(1, out bufferQuads);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufferQuads);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(quads.Length * Marshal.SizeOf(typeof(ObjQuad))), quads, BufferUsageHint.StaticDraw);
        }

        private void drawMesh(Shader shader)
        {
            // bind interleaved vertex data
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferVertices);
            GL.InterleavedArrays(InterleavedArrayFormat.T2fC4fN3fV3f, Marshal.SizeOf(typeof(ObjVertex)), IntPtr.Zero);

            // link vertex attributes to shader parameters
            GL.VertexAttribPointer(shader.attribute_vuvs, 2, VertexAttribPointerType.Float, false, 48, 0);
            GL.VertexAttribPointer(shader.attribute_vtan, 3, VertexAttribPointerType.Float, true, 32, 8);
            GL.VertexAttribPointer(shader.attribute_vnrm, 3, VertexAttribPointerType.Float, true, 48, 24);
            GL.VertexAttribPointer(shader.attribute_vpos, 3, VertexAttribPointerType.Float, false, 48, 36);

            // enable position, normal and uv attributes
            GL.EnableVertexAttribArray(shader.attribute_vuvs);
            GL.EnableVertexAttribArray(shader.attribute_vtan);
            GL.EnableVertexAttribArray(shader.attribute_vnrm);
            GL.EnableVertexAttribArray(shader.attribute_vpos);

            // bind triangle index data and render
            if (triangles.Length > 0)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufferTriangles);
                GL.DrawArrays(PrimitiveType.Triangles, 0, triangles.Length * 3);
            }

            // bind quad index data and render
            if (quads.Length > 0)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufferQuads);
                GL.DrawArrays(PrimitiveType.Quads, 0, quads.Length * 4);
            }

            // restore previous OpenGL state
            GL.UseProgram(0);
        }

        // render the mesh using the supplied shader and matrix
        public void Render(Shader shader, Matrix4 meshToWorld, Matrix4 worldToScreen, Vector3 materialcolor)
        {
            // pass transform to vertex shader
            shader.SetUniforms(meshToWorld, worldToScreen);
            GL.Uniform3(shader.uniform_materialcolor, ref materialcolor);

            float[] lightpos = Game.GetLightPositions();
            GL.Uniform1(shader.uniform_nlights, lightpos.Length / 3);
            GL.Uniform3(shader.uniform_lightpos, lightpos.Length, lightpos);

            drawMesh(shader);
        }

        // render the mesh using the supplied shader and matrix
        public void RenderSkyBox(Shader shader, Matrix4 meshToWorld, Matrix4 worldToScreen, CubeTexture textureSkyBox)
        {
            GL.DepthMask(false);
            Prepare(shader);
            GL.UseProgram(shader.programID);

            // enable texture
            GL.Uniform1(GL.GetUniformLocation(shader.programID, "pixels"), 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, textureSkyBox.id);

            // pass transform to vertex shader
            shader.SetUniforms(meshToWorld, worldToScreen);

            drawMesh(shader);
            GL.DepthMask(true);
        }

        // render the mesh using the supplied shader and matrix
        public void ReflectiveRender(Shader shader, Matrix4 meshToWorld, Matrix4 worldToScreen, CubeTexture textureSkyBox, Vector3 materialColor)
        {
            Prepare(shader);
            GL.UseProgram(shader.programID);

            // enable texture
            GL.Uniform1(GL.GetUniformLocation(shader.programID, "skybox"), 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, textureSkyBox.id);

            // pass transform to vertex shader
            shader.SetUniforms(meshToWorld, worldToScreen);
            GL.Uniform3(shader.uniform_materialcolor, materialColor);

            drawMesh(shader);
        }

        /// <summary>
        /// renders the mesh using the supplied shader and matrix
        /// </summary>
        /// <param name="furShader"></param>
        /// <param name="meshToWorld"></param>
        /// <param name="worldToScreen"></param>
        /// <param name="furTexture"></param>
        /// <param name="offset"></param>
        public void FurRender(FurShader furShader, Matrix4 meshToWorld, Matrix4 worldToScreen, Texture furTexture, float offset)
        {
            GL.UseProgram(furShader.programID);

            // enable texture
            furShader.SetTexture(furTexture);

            // pass transform to vertex shader
            furShader.SetUniforms(meshToWorld, worldToScreen);
            GL.Uniform1(furShader.uniform_furoffset, offset);

            drawMesh(furShader);
        }

        /// <summary>
        /// this contains the information about a single vertex
        /// The data in the struct is aligned, following the InterleavedArrayFormat.T2fC4fN3fV3f format.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct ObjVertex
        {
            [FieldOffset(0)] public Vector2 TexCoord;
            [FieldOffset(8)] public Vector3 Tangent;
            [FieldOffset(24)] public Vector3 Normal;
            [FieldOffset(36)] public Vector3 Vertex;

            public ObjVertex(Vector2 TexCoord, Vector3 Normal, Vector3 Vertex) : this(TexCoord, Vector3.Zero, Normal, Vertex) { }
            public ObjVertex(ObjVertex obj, Vector3 tangent) : this(obj.TexCoord, tangent, obj.Normal, obj.Vertex) { }
            public ObjVertex(Vector2 TexCoord, Vector3 Tangent, Vector3 Normal, Vector3 Vertex)
            {
                this.TexCoord = TexCoord;
                this.Tangent = Tangent;
                this.Normal = Normal;
                this.Vertex = Vertex;
            }
        }

        // layout of a single triangle
        [StructLayout(LayoutKind.Sequential)]
        public struct ObjTriangle
        {
            public int Index0, Index1, Index2;

            public ObjTriangle(int Index0, int Index1, int Index2)
            {
                this.Index0 = Index0;
                this.Index1 = Index1;
                this.Index2 = Index2;
            }
        }

        // layout of a single quad
        [StructLayout(LayoutKind.Sequential)]
        public struct ObjQuad
        {
            public int Index0, Index1, Index2, Index3;

            public ObjQuad(int Index0, int Index1, int Index2, int Index3)
            {
                this.Index0 = Index0;
                this.Index1 = Index1;
                this.Index2 = Index2;
                this.Index3 = Index3;
            }
        }
    }

} // namespace Template_P3
