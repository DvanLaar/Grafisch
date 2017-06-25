using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using template_P3;

namespace Template_P3
{

    // mesh and loader based on work by JTalton; http://www.opentk.com/node/642
    public class Mesh
    {
        // data members
        public ObjVertex[] vertices;            // vertex positions, model space
        public ObjTriangle[] triangles;         // triangles (3 vertex indices)
        public ObjQuad[] quads;                 // quads (4 vertex indices)

        // The buffers belonging to these IDs contain all the data from above, in a format more useful to the GPU
        private int vertexBufferId, triangleBufferId, quadBufferId;

        public Mesh() { }

        // constructor
        public Mesh(string fileName)
        {
            MeshLoader loader = new MeshLoader();
            loader.Load(this, fileName);
        }

        // initialization; called during first render
        public void Prepare(Shader shader)
        {
            if (vertexBufferId != 0) return; // already taken care of

            // generate interleaved vertex data (uv/normal/position (total 8 floats) per vertex)
            GL.GenBuffers(1, out vertexBufferId);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * Marshal.SizeOf(typeof(ObjVertex))), vertices, BufferUsageHint.StaticDraw);

            // generate triangle index array
            GL.GenBuffers(1, out triangleBufferId);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, triangleBufferId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(triangles.Length * Marshal.SizeOf(typeof(ObjTriangle))), triangles, BufferUsageHint.StaticDraw);

            // generate quad index array
            GL.GenBuffers(1, out quadBufferId);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, quadBufferId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(quads.Length * Marshal.SizeOf(typeof(ObjQuad))), quads, BufferUsageHint.StaticDraw);
        }

        public void Load2DTexture(Texture texture, int shaderID, string varName, TextureUnit unit)
        {
            GL.Uniform1(GL.GetUniformLocation(shaderID, varName), unit - TextureUnit.Texture0);
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, texture == null ? -1 : texture.id);
        }

        public void SetTexture(Texture texture, int shaderID)
        {
            Load2DTexture(texture, shaderID, "pixels", TextureUnit.Texture0);
        }

        public void SetNormal(Texture normalMap, int shaderID)
        {
            Load2DTexture(normalMap, shaderID, "normals", TextureUnit.Texture1);
        }

        private void drawMesh(Shader shader)
        {
            // bind interleaved vertex data
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
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
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, triangleBufferId);
                GL.DrawArrays(PrimitiveType.Triangles, 0, triangles.Length * 3);
            }

            // bind quad index data and render
            if (quads.Length > 0)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, quadBufferId);
                GL.DrawArrays(PrimitiveType.Quads, 0, quads.Length * 4);
            }

            // restore previous OpenGL state
            GL.UseProgram(0);
        }

        // render the mesh using the supplied shader and matrix
        public void Render(Shader shader, Matrix4 modelToWorld, Matrix4 worldToScreen, Vector3 materialcolor)
        {
            // pass transform to vertex shader
            GL.UniformMatrix4(shader.uniform_modeltoworld, false, ref modelToWorld);
            GL.UniformMatrix4(shader.uniform_worldtoscreen, false, ref worldToScreen);
            GL.Uniform3(GL.GetUniformLocation(shader.programID, "camerapos"), ref Game.cameraPosition);
            GL.Uniform3(shader.uniform_lightpos, ref Game.lightPosition);
            GL.Uniform3(shader.uniform_materialcolor, ref materialcolor);

            drawMesh(shader);
        }

        // render the mesh using the supplied shader and matrix
        public void SkyboxRender(Shader shader, Matrix4 modelToWorld, Matrix4 worldToScreen, CubeTexture texture)
        {
            GL.DepthMask(false);
            // on first run, prepare buffers
            Prepare(shader);

            // enable shader
            GL.UseProgram(shader.programID);
            
            // enable texture
            int texLoc = GL.GetUniformLocation(shader.programID, "pixels");
            GL.Uniform1(texLoc, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, texture.id);

            // pass transform to vertex shader
            GL.UniformMatrix4(shader.uniform_modeltoworld, false, ref modelToWorld);
            GL.UniformMatrix4(shader.uniform_worldtoscreen, false, ref worldToScreen);
            GL.Uniform3(shader.uniform_camerapos, ref Game.cameraPosition);

            drawMesh(shader);
            GL.DepthMask(true);
        }

        // render the mesh using the supplied shader and matrix
        public void ReflectiveRender(Shader shader, Matrix4 modelToWorld, Matrix4 worldToScreen, CubeTexture skybox, Vector3 materialcolor)
        {
            // on first run, prepare buffers
            Prepare(shader);

            // enable shader
            GL.UseProgram(shader.programID);

            // enable texture
            int texLoc = GL.GetUniformLocation(shader.programID, "skybox");
            GL.Uniform1(texLoc, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, skybox.id);

            // pass transform to vertex shader
            GL.UniformMatrix4(shader.uniform_modeltoworld, false, ref modelToWorld);
            GL.UniformMatrix4(shader.uniform_worldtoscreen, false, ref worldToScreen);
            GL.Uniform3(GL.GetUniformLocation(shader.programID, "camerapos"), ref Game.cameraPosition);
            GL.Uniform3(GL.GetUniformLocation(shader.programID, "materialcolor"), materialcolor);

            drawMesh(shader);
        }


        // render the mesh using the supplied shader and matrix
        public void FurRender(Shader shader, Matrix4 modelToWorld, Matrix4 worldToScreen, Texture texture, float offset)
        {
            // on first run, prepare buffers
            Prepare(shader);

            // enable shader
            GL.UseProgram(shader.programID);

            // enable texture
            int texLoc = GL.GetUniformLocation(shader.programID, "pixels");
            GL.Uniform1(texLoc, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture.id);

            // pass transform to vertex shader
            GL.UniformMatrix4(shader.uniform_modeltoworld, false, ref modelToWorld);
            GL.UniformMatrix4(shader.uniform_worldtoscreen, false, ref worldToScreen);
            GL.Uniform1(((FurShader)shader).uniform_furoffset, offset);

            drawMesh(shader);
        }

        // layout of a single vertex
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