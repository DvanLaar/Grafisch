#version 330
in vec4 color;
in vec3 normal;
in vec3 pos;

out vec4 outputColor;

uniform vec3 lightpoint;
uniform vec3 lightcolor;

void main()
{
	float dist = length(lightpoint - pos);
	float brightness = 100f / dist / dist;

	vec3 L = normalize (lightpoint - pos);
	float dotprod = max(dot(L,normal),0.0);

	vec4 lc = vec4(lightcolor,1f);
	outputColor = clamp(brightness*dotprod*lc*color,0.0,1.0);
    //outputColor = color;
	// outputColor = vec4(0,0,1.0,0.5);
     //outputColor = vec4(normal.x, normal.y, normal.z, 1.0);
	 //outputColor = vec4(lightcolor.x, lightcolor.y, lightcolor.z, 1.0);
}
