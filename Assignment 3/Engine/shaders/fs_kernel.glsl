#version 330

// shader input
in vec2 P;
in vec2 uv;
uniform sampler2D pixels;

// shader output
out vec3 outputColor;

void main()
{
	//Temp variables to be changed by uniforms;
	float pixelwidth = 1/640;
	float pixelheight = 1/400;



	// retrieve input pixel
	outputColor = texture( pixels, uv-pixelwidth).rgb;
	outputColor += texture( pixels, uv).rgb;
	//outputColor += texture( pixels, uv+pixelwidth).rgb;
	outputColor /= 2;

}

// EOF