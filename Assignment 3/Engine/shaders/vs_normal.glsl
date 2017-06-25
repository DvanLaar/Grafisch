#version 330
 
// shader input
in vec2 vUV; // vertex uv coordinate
in vec3 vNormal; // untransformed vertex normal
in vec3 vTangent; // untransformed vertex tangent: direction in +u direction
in vec3 vPosition; // untransformed vertex position

// shader output
out vec2 uv;
out vec3 position; // for lighting
out mat3 TBN; // tangent space, used for normal mapping

// transformations
uniform mat4 modelToWorld, worldToScreen;
 
// vertex shader
void main()
{
	// transform vertex using supplied matrix
	gl_Position = worldToScreen * modelToWorld * vec4(vPosition, 1.0);
	position = (modelToWorld * vec4(vPosition,1.0)).xyz;

    vec3 tangent = vTangent;
    // VERSION 1: don't orthonormalize T:
    // vec3 T = normalize((modelToWorld * vec4(vTangent, 1.0f)).xyz);
    // VERSION 2: do orthonormalize T:
    // tangent = tangent - dot(tangent, vNormal) * vNormal;

    if (-1e-10 < dot(tangent, tangent) && dot(tangent, tangent) < 1e-10) {
        gl_Position = worldToScreen * vec4(0, 0, 0, 1f);
    }

	// forward normal and uv coordinate; will be interpolated over triangle
	vec3 N = normalize((modelToWorld * vec4(vNormal, 1.0f)).xyz);
    vec3 T = normalize((modelToWorld * vec4(tangent, 1.0f)).xyz);
    vec3 B = cross(N, T); // calculate the bitangent (orthogonal part)
    TBN = mat3(T, B, N);
	uv = vUV;
}