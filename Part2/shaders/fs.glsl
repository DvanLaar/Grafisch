#version 330
in vec4 color;
out vec4 outputColor;
void main()
{
    outputColor = color;
	// outputColor = vec4(0,0,1.0,0.5);
    // outputColor = vec4(1.0, 0.0, 1.0, 1.0);
}
