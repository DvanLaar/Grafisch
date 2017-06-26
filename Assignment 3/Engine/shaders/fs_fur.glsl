#version 330

// shader input
in vec2 uv; // interpolated texture coordinates
in vec4 normal; // interpolated normal

uniform sampler2D pixels; // texture sampler

// shader output
layout(location = 0) out vec4 outputColor;
layout(location = 1) out vec4 outputHDR;

// passthrough shader for fur
void main()
{
    vec4 color = texture(pixels, uv);
	// Easy opacity
    if (color.a < 0.1) {
        discard;
    }
    outputColor = color;
	outputHDR = clamp(color-vec4(1,1,1,1),0,1);
}