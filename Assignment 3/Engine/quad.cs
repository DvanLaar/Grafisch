using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using rasterizer;

namespace rasterizer
{

    public class ScreenQuad
    {
        // data members
        private int vbo_idx = 0, vbo_vert = 0;
        private readonly float[] vertices = new float[] {
            -1, 1, 0, 0,
            1, 1, 1, 0,
            1, 1, 1, -1,
            0, 1, 0, -1,
            -1, 0, 0, 0
        };
        private readonly int[] indices = new int[] {
            0, 1, 2, 3
        };

        // constructor
        public ScreenQuad()
        {
        }

        // initialization; called during first render
        public void Prepare(Shader shader)
        {
            if (vbo_vert != 0) return; // we've already been here

            // prepare VBO for quad rendering
            GL.GenBuffers(1, out vbo_vert);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_vert);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(4 * 5 * 4), vertices, BufferUsageHint.StaticDraw);
            GL.GenBuffers(1, out vbo_idx);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbo_idx);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(16), indices, BufferUsageHint.StaticDraw);
        }

        private void RenderQuad(Shader shader)
        {
            // bind interleaved vertex data
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_vert);
            GL.InterleavedArrays(InterleavedArrayFormat.T2fV3f, 20, IntPtr.Zero);

            // link vertex attributes to shader parameters 
            GL.VertexAttribPointer(shader.attribute_vpos, 3, VertexAttribPointerType.Float, false, 20, 0);
            GL.VertexAttribPointer(shader.attribute_vuvs, 2, VertexAttribPointerType.Float, false, 20, 3 * 4);

            // bind triangle index data and render
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbo_idx);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            // disable shader
            GL.UseProgram(0);
        }

        // render the mesh using the supplied shader and matrix
        public void RenderKernel(PostKernelShader shader, int textureID, float textureWidth, float textureHeight, Kernel kernel)
        {
            Prepare(shader);
            shader.KernelRender(textureID, textureWidth, textureHeight, kernel);
            RenderQuad(shader);
        }

        // render the mesh using the supplied shader and matrix
        public void RenderVigAndChrom(PostVigAndChromShader shader, int textureID, float vignettingfactor, Vector2 center, Vector3 ca_factor)
        {
            Prepare(shader);
            shader.KernelRender(textureID, vignettingfactor, center, ca_factor);
            RenderQuad(shader);
        }

        public void RenderBloomBlend(Shader shader, int textureID, int bloomID)
        {
            Prepare(shader);
            GL.UseProgram(shader.programID);

            // enable texture
            shader.SetTexture(textureID);
            shader.Load2DTexture(bloomID, "bloom", TextureUnit.Texture1);

            // enable position and uv attributes
            GL.EnableVertexAttribArray(shader.attribute_vpos);
            GL.EnableVertexAttribArray(shader.attribute_vuvs);

            RenderQuad(shader);
        }
    }

} // namespace Template_P3