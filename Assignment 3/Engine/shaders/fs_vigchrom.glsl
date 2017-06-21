#version 330

// shader input
in vec2 P, uv;
uniform sampler2D pixels;

// shader output
out vec3 outputColor;


// Temp variables to be changed by uniforms;

uniform float vignettingfactor = 1f;
uniform vec3 ca_factor = vec3(0.0005f,0.001f,0.002f);

uniform vec2 center = vec2(0.5f,0.5f);

void main()
{
	vec2 origin = uv-center;
	float length = length(origin);

	//Red, Green and Blue components
	float rcomp = texture(pixels, uv + (ca_factor.r*origin)).r;
	float gcomp = texture(pixels, uv + (ca_factor.g*origin)).g;
	float bcomp = texture(pixels, uv + (ca_factor.b*origin)).b;

	vec3 finalColor = vec3(rcomp,gcomp,bcomp);

	outputColor = cos(vignettingfactor*length)*finalColor;
}
