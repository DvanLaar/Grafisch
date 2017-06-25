#version 330

// shader input
in vec2 uv; // interpolated texture coordinates
in vec4 normal; // interpolated normal
in vec3 position;

uniform vec3 camerapos;

// shader output
layout(location = 0) out vec4 outputColor;
layout(location = 1) out vec4 outputHDR;

uniform vec3 materialcolor;
uniform samplerCube skybox;

// fragment shader
void main()
{
	vec3 refl = reflect(normalize(position-camerapos),normalize(normal.xyz));

    outputColor = vec4(materialcolor, 1) * texture(skybox, refl);
    outputHDR = clamp(outputColor - vec4(1f, 1f, 1f, 1f), 0, 1);
}