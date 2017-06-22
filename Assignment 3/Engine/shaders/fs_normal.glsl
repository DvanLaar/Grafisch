﻿#version 330
 
// shader input
in vec2 uv; // interpolated texture coordinates
in vec3 position;

uniform sampler2D pixels, normals;
uniform vec3 camerapos, materialcolor;

// shader output
layout(location=0) out vec4 outputColor;
layout(location=1) out vec4 outputHDR;

// fragment shader
void main()
{
	// Hardcoded Light Parameters
	vec3 lightpos = vec3(10,10,10);
	vec3 lightdiffuseintensity = vec3(100,100,100);
	vec3 lightspecularintensity = lightdiffuseintensity;
	float specularpower = 60;

	// Direction from position to light for diffuse part
	vec3 lightdir = lightpos - position;
	float attenuation = 1/(length(lightdir)*length(lightdir));
	lightdir = normalize(lightdir);

	// Normalized normal
	vec3 nnormal = normalize(texture(normals, uv).rgb * 2f - 1f);

	// direction and reflection from the camera to the position for specular part
	vec3 cameradir = normalize(position-camerapos);
	vec3 reflection = reflect(cameradir,nnormal);
	
	// Color of the fragment in general
	vec4 color = texture(pixels, uv);

	// Color specifics for Phong
	vec4 ambientcolor = vec4((0.1f * color).rgb,color.w);
	vec4 diffusecolor = color;
	vec4 specularcolor = color;

	// Final phong output
    outputColor =	(ambientcolor) * vec4(materialcolor,1) +
					(attenuation * diffusecolor * clamp(dot(nnormal,lightdir),0,1) * vec4(materialcolor,1) * vec4(lightdiffuseintensity,1)) +
					(attenuation * specularcolor * pow( clamp(dot(lightdir,cameradir),0,1),specularpower )* vec4(lightspecularintensity,1));

	outputHDR = clamp(outputColor-vec4(1f,1f,1f,1f),0,1);
}