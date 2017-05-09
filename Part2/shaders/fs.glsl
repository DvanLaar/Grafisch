#version 330
in vec4 color;
in vec3 normal;
in vec3 pos;

out vec4 outputColor;

//Data for diffuse light
uniform vec3 lightpoint;
uniform vec3 lightcolor;

void main()
{
	//Inverse square law
	float dist = length(lightpoint - pos);
	float brightness = 100f / dist / dist;

	//Variables for diffuse shading
	vec3 L = normalize (lightpoint - pos);
	float dotprod = max(dot(L,normal),0.0);

	vec4 lc = vec4(lightcolor,1f);
	outputColor = clamp(brightness*dotprod*lc*color,0.0,1.0);
}
