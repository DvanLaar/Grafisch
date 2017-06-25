#version 330

// shader input
in vec2 vUV; // vertex uv coordinate
in vec3 vNormal; // untransformed vertex normal
in vec3 vPosition; // untransformed vertex position

// shader output
out vec4 normal; // transformed vertex normal
out vec2 uv;

// transformations
uniform mat4 modelToWorld, worldToScreen;

// fur specific uniforms
uniform float furoffset;

// vertex shader
void main()
{
    // transform vertex using supplied matrix
    vec3 position = vPosition;
	// apply offset of the fur layer
    position = position + (furoffset * 0.015 * vNormal);
    gl_Position = (worldToScreen * modelToWorld) * vec4(position, 1.0);

    // forward normal and uv coordinate; will be interpolated over triangle
    normal = modelToWorld * vec4(vNormal, 0.0f);
    uv = vUV;
}