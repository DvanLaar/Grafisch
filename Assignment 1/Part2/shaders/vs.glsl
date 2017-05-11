#version 330
in vec3 vPosition;
in vec3 vColor;
in vec3 vNormal;

out vec3 color;
out vec3 normal;
out vec3 pos;

uniform mat4 M;

void main()
{
    gl_Position = M * vec4(vPosition, 1f);

    // passthrough for position for diffuse shading
    pos = vPosition.xyz;

    // pass the color to the fragment shader
    color = vColor;

    // normalize the normal here
    normal = normalize(vNormal);
}
