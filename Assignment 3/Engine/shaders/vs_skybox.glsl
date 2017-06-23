#version 330
 
// shader input
in vec2 vUV;				// vertex uv coordinate
in vec3 vNormal;			// untransformed vertex normal
in vec3 vPosition;			// untransformed vertex position

// shader output
out vec3 uv;			

//transformations
uniform mat4 modelToWorld;	
uniform mat4 worldToScreen;
uniform vec3 camerapos;

// vertex shader
void main()
{
	uv = vPosition;
	gl_Position = worldToScreen * modelToWorld * vec4(vPosition + camerapos, 1.0);
}