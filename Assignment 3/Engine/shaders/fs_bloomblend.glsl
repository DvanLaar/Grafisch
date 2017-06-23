#version 330

// shader input
in vec2 P, uv;
uniform sampler2D pixels, bloom;

out vec4 outputColor;

void main()
{
    outputColor = texture(pixels, uv) + vec4(texture(bloom, uv).rgb, 0);
}
