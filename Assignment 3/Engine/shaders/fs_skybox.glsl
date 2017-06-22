#version 330

// shader input
in vec3 uv;
uniform samplerCube pixels;

// shader output
layout(location=0) out vec4 outputColor;
layout(location=1) out vec4 outputHDR;

void main()
{
	outputColor = texture(pixels, uv);
	outputHDR = clamp(outputColor*1.5f-vec4(1f,1f,1f,1f),0,1);
}
