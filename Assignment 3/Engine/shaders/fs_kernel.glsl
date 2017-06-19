#version 330

// shader input
in vec2 P;
in vec2 uv;
uniform sampler2D pixels;

// shader output
out vec3 outputColor;


//Temp variables to be changed by uniforms;

uniform float pixelwidth;
uniform float pixelheight;

uniform int kernelwidth;
uniform int kernelheight;

//Maximum size of kernel is 20x20
uniform float horizontal[20];
uniform float vertical[20];

float boxvalue(in int x, in int y)
{
	return horizontal[x]*vertical[y];
}

void main()
{
	vec3 finalColor = vec3(0,0,0);
	float centerx = floor(kernelwidth/2);
	float centery = floor(kernelheight/2);

	for(int x = 0; x < kernelwidth; x++)
	{
		for(int y = 0; y < kernelheight; y++)
		{
			finalColor += boxvalue(x,y)*texture(pixels, vec2(uv.x - pixelwidth*(x-centerx),uv.y - pixelheight*(y-centery))).rgb;
			//finalColor += (1/(kernelwidth*kernelheight))*texture(pixels,vec2(uv.x - (x-centerx)*0.1,uv.y)).rgb;
		}
	}

	outputColor = finalColor;
}
