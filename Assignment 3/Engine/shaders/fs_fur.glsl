#version 330
 
// shader input
in vec2 uv;						// interpolated texture coordinates
in vec4 normal;					// interpolated normal

uniform sampler2D pixels;		// texture sampler

// shader output
out vec4 outputColor;

// passthrough shader for fur
void main()
{
	vec4 color = texture(pixels, uv);

	if(color.a < 0.1)
	{
		discard;
	}
    outputColor = color;
}