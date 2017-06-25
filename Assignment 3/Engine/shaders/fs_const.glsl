#version 330

// shader input
in vec2 uv; // interpolated texture coordinates
in vec3 position; // interpolated normal and position in world coordinates

uniform vec3 materialcolor;

// shader output
layout(location = 0) out vec4 outputColor;
layout(location = 1) out vec4 outputHDR;

// fragment shader
void main()
{
    outputColor = vec4(materialcolor, 1f);
    outputHDR = clamp(outputColor - vec4(1f, 1f, 1f, 1f), 0, 1);
}
