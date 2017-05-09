#version 330
in vec3 vPosition;
in vec3 vColor;
in vec3 vNormal;

out vec4 color;
out vec3 normal;
out vec3 pos;

uniform mat4 M;

void main()
{
	gl_Position = M * vec4(vPosition, 1.0 );
	pos = (M * vec4(vPosition, 1.0 )).xyz;
	color = vec4(vColor, 1.0);
	normal = normalize(vNormal);
    // color = vec4(1,0,1,.2);
    // color = vec4(vColor, 1.0);
}
