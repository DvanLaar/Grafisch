﻿#version 330

// shader input
in vec2 P, uv;

uniform sampler2D pixels, bloom;

// shader output
out vec4 outputColor;

void main()
{
	// Add the "blurred hdr" bloom to the normal render 
    outputColor = texture(pixels, uv) + vec4(texture(bloom, uv).rgb, 0);
}
