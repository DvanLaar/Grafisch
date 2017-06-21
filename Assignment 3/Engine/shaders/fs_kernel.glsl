#version 330

// shader input
in vec2 P, uv;
uniform sampler2D pixels;

// shader output
out vec3 outputColor;


// Temp variables to be changed by uniforms;

uniform float pixelwidth, pixelheight;
uniform int kernelwidth, kernelheight;
uniform float centerx, centery;

// Maximum size of kernel is 64x64
uniform float horizontal[32], vertical[32];

float boxvalue(in int x, in int y)
{
	return horizontal[x]*vertical[y];
}

void main()
{
	float ccenterx = floor(kernelwidth/2f);
	float ccentery = floor(kernelheight/2f);
	
	vec3 finalColor = vec3(0,0,0);
	for(int x = 0; x < kernelwidth; x++) {
		for(int y = 0; y < kernelheight; y++) {
			finalColor += boxvalue(x, y) * texture(pixels, vec2(uv.x - pixelwidth * (x - ccenterx), uv.y - pixelheight * (y - ccentery))).rgb;
			// finalColor += (1 / (kernelwidth * kernelheight)) * texture(pixels, vec2(uv.x - (x - centerx) * 0.1, uv.y)).rgb;
		}
	}
	outputColor = finalColor;
}
