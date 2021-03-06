﻿#version 330

// shader input
in vec2 P, uv;

// shader output
out vec3 outputColor;

// uniforms (texture specific)
uniform sampler2D pixels;
uniform float pixelwidth, pixelheight;

// uniforms (kernel specific)
uniform int kernelwidth, kernelheight;
uniform float centerx, centery;
uniform float horizontal[32], vertical[32]; // Maximum size of kernel is 32x32

// returns value of the kernel at (x, y)
float boxvalue(in int x, in int y)
{
    return horizontal[x] * vertical[y];
}

void main()
{
	// sample for each element of the kernel
    vec3 finalColor = vec3(0, 0, 0);
    for (int x = 0; x < kernelwidth; x++) {
        for (int y = 0; y < kernelheight; y++) {
            finalColor += boxvalue(x, y) * texture(pixels, uv - vec2(pixelwidth * (x - centerx), pixelheight * (y - centery))).rgb;
            // finalColor += (1 / (kernelwidth * kernelheight)) * texture(pixels, vec2(uv.x - (x - centerx) * 0.1, uv.y)).rgb;
        }
    }
    outputColor = finalColor;
}
