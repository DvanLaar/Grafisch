using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using template_P3;

namespace Template_P3 {

public class ScreenQuad
{
	// data members
	int vbo_idx = 0, vbo_vert = 0;
	float [] vertices = { -1, 1, 0, 0, 1, 1, 1, 0, 1, 1, 1, -1, 0, 1, 0, -1, -1, 0, 0, 0 };
	int [] indices = { 0, 1, 2, 3 };
	// constructor
	public ScreenQuad()
	{
	}

	// initialization; called during first render
	public void Prepare( Shader shader )
	{
        if (vbo_vert != 0) return; // we've already been here
		// prepare VBO for quad rendering
		GL.GenBuffers( 1, out vbo_vert );
		GL.BindBuffer( BufferTarget.ArrayBuffer, vbo_vert );
		GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(4 * 5 * 4), vertices, BufferUsageHint.StaticDraw );
		GL.GenBuffers( 1, out vbo_idx );
		GL.BindBuffer( BufferTarget.ElementArrayBuffer, vbo_idx );
		GL.BufferData( BufferTarget.ElementArrayBuffer, (IntPtr)(16), indices, BufferUsageHint.StaticDraw );
	}

	// render the mesh using the supplied shader and matrix
	public void Render( Shader shader, int textureID )
	{
		// on first run, prepare buffers
		Prepare( shader );

		// enable texture
		int texLoc = GL.GetUniformLocation( shader.programID, "pixels" );
		GL.Uniform1( texLoc, 0 );
		GL.ActiveTexture( TextureUnit.Texture0 );
		GL.BindTexture( TextureTarget.Texture2D, textureID );

		// enable shader
		GL.UseProgram( shader.programID );

		// enable position and uv attributes
		GL.EnableVertexAttribArray( shader.attribute_vpos );
		GL.EnableVertexAttribArray( shader.attribute_vuvs );

		// bind interleaved vertex data
		GL.EnableClientState( ArrayCap.VertexArray );
		GL.BindBuffer( BufferTarget.ArrayBuffer, vbo_vert );
		GL.InterleavedArrays( InterleavedArrayFormat.T2fV3f, 20, IntPtr.Zero );

		// link vertex attributes to shader parameters 
		GL.VertexAttribPointer( shader.attribute_vpos, 3, VertexAttribPointerType.Float, false, 20, 0 );
		GL.VertexAttribPointer( shader.attribute_vuvs, 2, VertexAttribPointerType.Float, false, 20, 3 * 4 );

		// bind triangle index data and render
		GL.BindBuffer( BufferTarget.ElementArrayBuffer, vbo_idx );
		GL.DrawArrays( PrimitiveType.Quads, 0, 4 );

		// disable shader
		GL.UseProgram( 0 );
	}

        // render the mesh using the supplied shader and matrix
        public void KernelRender(postKernelShader shader, int textureID, float textureWidth, float textureHeight, Kernel kernel)
        {
            // on first run, prepare buffers
            Prepare(shader);

            // enable texture
            int texLoc = GL.GetUniformLocation(shader.programID, "pixels");
            GL.Uniform1(texLoc, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            // enable shader
            GL.UseProgram(shader.programID);

            // set uniforms
            GL.Uniform1(shader.uniform_pixelwidth, 1f / textureWidth);
            GL.Uniform1(shader.uniform_pixelheight, 1f / textureHeight);
            GL.Uniform1(shader.uniform_kernelwidth, kernel.horizontal.Length);
            GL.Uniform1(shader.uniform_kernelheight, kernel.vertical.Length);
            GL.Uniform1(shader.uniform_horizontal, kernel.horizontal.Length, kernel.horizontal);
            GL.Uniform1(shader.uniform_vertical, kernel.vertical.Length, kernel.vertical);

            // enable position and uv attributes
            GL.EnableVertexAttribArray(shader.attribute_vpos);
            GL.EnableVertexAttribArray(shader.attribute_vuvs);

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
    }

} // namespace Template_P3