#version 330
 
// shader input
in vec2 vUV;				// vertex uv coordinate
in vec3 vNormal;			// untransformed vertex normal
in vec3 vPosition;			// untransformed vertex position

// shader output
out vec4 normal;			// transformed vertex normal
out vec2 uv;			

uniform mat4 modelToWorld;	
uniform mat4 worldToScreen;
 
// vertex shader
void main()
{
	// transform vertex using supplied matrix
	gl_Position = (modelToWorld*worldToScreen) * vec4(vPosition, 1.0);

	// forward normal and uv coordinate; will be interpolated over triangle
	normal = modelToWorld * vec4( vNormal, 0.0f );
	uv = vUV;
}