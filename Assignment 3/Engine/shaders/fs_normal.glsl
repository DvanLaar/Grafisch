#version 330

// shader input
in vec2 uv; // interpolated texture coordinates
in vec3 position;
in mat3 TBN;

// shader output
layout(location = 0) out vec4 outputColor;
layout(location = 1) out vec4 outputHDR;

uniform sampler2D pixels, normals;
uniform vec3 lightpos, camerapos, materialcolor;

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

    // Direction from position to light for diffuse part
    vec3 lightdir = lightpos - position;
    float attenuation = 1f / dot(lightdir, lightdir);
    lightdir = normalize(lightdir);

    // Normalized normal, in world space
    vec3 normal = texture(normals, uv).xyz * 2f - vec3(1f, 1f, 1f);
    vec3 nnormal = normalize(TBN * normal);

    // direction and reflection from the camera to the position for specular part
    vec3 cameradir = normalize(camerapos - position);
    vec3 reflection = 2 * dot(cameradir, nnormal) * nnormal - cameradir;

    // Color of the fragment in general
    vec4 color = texture(pixels, uv);

    // Color specifics for Phong
    vec4 ambientcolor = vec4((0.1f * color).rgb, color.w);
    vec4 diffusecolor = color;
    vec4 specularcolor = color;

    // just add something to it :)
    vec4 ambient = ambientcolor * vec4(materialcolor, 1);
    // diffuse = c_{diff} * (N . L) * L_{diff}
    vec4 diffuse = diffusecolor * clamp(dot(nnormal, lightdir), 0, 1) * vec4(materialcolor, 1) * lightdiffuseintensity;
    // specular = c_{spec} * (L . R)^\alpha * L_{spec}
    vec4 specular = specularcolor * powself(clamp(dot(lightdir, reflection), 0, 1), specularPowerLog2) * lightspecularintensity;

    // Final phong output
    outputColor = ambient + attenuation * (diffuse + specular);
    outputHDR = clamp(outputColor - vec4(1f, 1f, 1f, 1f), 0, 1);
}