#version 330

// shader input
in vec2 vUV; // vertex uv coordinate
in vec3 vNormal; // untransformed vertex normal
in vec3 vPosition; // untransformed vertex position

// shader output
out vec2 uv;
out vec3 normal, position; // transformed normal and position

// transformations
uniform mat4 modelToWorld, worldToScreen;

// vertex shader
void main()
{
    // transform vertex using supplied matrix
    gl_Position = worldToScreen * modelToWorld * vec4(vPosition, 1.0);
    position = (modelToWorld * vec4(vPosition, 1.0)).xyz;

    // forward normal and uv coordinate; will be interpolated over triangle
    normal = (modelToWorld * vec4(vNormal, 0.0f)).xyz;
    uv = vUV;
}