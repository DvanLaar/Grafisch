﻿#version 330

// shader input
in vec2 uv; // interpolated texture coordinates
in vec3 normal, position; // interpolated normal and position in world coordinates

uniform sampler2D pixels; // texture sampler
uniform vec3 camerapos, materialcolor;

uniform int nlights;
uniform vec3 lightpos[100];

// hardcoded light parameters
const int specularPowerLog2 = 6;
const float I = 100f;
const vec4 lightdiffuseintensity = vec4(I, I, I, 1);
const vec4 lightspecularintensity = vec4(I, I, I, 1);

// shader output
layout(location = 0) out vec4 outputColor;
layout(location = 1) out vec4 outputHDR;

float powself(float base, int exp)
{
	while (exp-- > 0) base *= base;
	return base;
}

// fragment shader
void main()
{
	// Color of the fragment in general
	vec4 color = texture(pixels, uv);

	// Color specifics for Phong
	vec4 ambientcolor = vec4((0.1f * color).rgb, color.w);
	vec4 diffusecolor = color, specularcolor = color;

	// Normalized normal
	vec3 N = normalize(normal);

	// direction and reflection from the camera to the position for specular part
	vec3 cameradir = normalize(camerapos - position);
	vec3 R = 2 * dot(cameradir, N) * N - cameradir;

	outputColor = ambientcolor * vec4(materialcolor, 1);
	for (int i = nlights; i-- > 0; ) {
		// Direction from position to light for diffuse part
		vec3 L = lightpos[i] - position;
		float attenuation = 1f / dot(L, L);
		L = normalize(L);

		float NdotL = dot(N, L);
		if (NdotL <= 0f) continue;

		// diffuse = c_{diff} * (N . L) * L_{diff}
		outputColor += attenuation * diffusecolor * clamp(NdotL, 0, 1) * vec4(materialcolor, 1) * lightdiffuseintensity;

		// specular = c_{spec} * (L . R)^\alpha * L_{spec}
		outputColor += attenuation * specularcolor * powself(clamp(dot(L, R), 0, 1), specularPowerLog2) * lightspecularintensity;
	}
	// Final phong output
	outputHDR = clamp(outputColor - vec4(1f, 1f, 1f, 1f), 0, 1);
}
