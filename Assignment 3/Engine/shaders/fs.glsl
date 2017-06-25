#version 330

// shader input
in vec2 uv; // interpolated texture coordinates
in vec3 normal, position; // interpolated normal and position in world coordinates

uniform sampler2D pixels; // texture sampler
uniform vec3 camerapos, materialcolor;
uniform float[] lightpos;

// shader output
layout(location = 0) out vec4 outputColor;
layout(location = 1) out vec4 outputHDR;

float powself(float base, int exp)
{
    while (exp-- > 0) base *= base;
    return base;
}

// fragment shader
void main()
{
    // Hardcoded Light Parameters
    vec4 lightdiffuseintensity = vec4(400, 400, 400, 1), lightspecularintensity = lightdiffuseintensity;
    int specularPowerLog2 = 6;

	vec4 ambientcolor = vec4((0.1f * color).rgb, color.w);
	outputColor = ambientcolor * vec4(materialcolor, 1);

	for (int i = 0; i < 12; i+=3)
	{
		vec3 lpos = vec3(lightpos[i], lightpos[i+1], lightpos[i+2]);

		vec3 lightdir = lpos;
		float attenuation = 1f / dot(lightdir, lightdir);
		lightdir = normalize(lightdir);

		vec3 diffuse = attenuation * lightdiffuseintensity * materialcolor * max(0.0f, dot(normal, lightdir));
		vec3 specular;
		if (dot(normal, light) < 0.0f)
		{
			specular = vec3(0, 0, 0)
		}
		else
		{
		    vec3 cameradir = normalize(camerapos - position);
			vec3 reflection = 2 * dot(cameradir, nnormal) * nnormal - cameradir;
			specular = attenuation * lightspecularintensity * materialcolor * powself(max(0.0, dot(reflect(-lightdir, normal), cameradir)), reflection)
		}

		outputColor = outputColor + diffuse + specular;
	}

    // Final phong output
    outputColor = outputColor;
    outputHDR = clamp(outputColor - vec4(1f, 1f, 1f, 1f), 0, 1);
}