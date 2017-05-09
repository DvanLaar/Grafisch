#version 330
in vec3 color;
in vec3 normal;
in vec3 pos;

out vec4 outputColor;

// Data for diffuse light
uniform vec3 lightpoint;
uniform vec3 lightcolor;

void main()
{
    vec3 toLight = lightpoint - pos;

    // Use the inverse square law to get the brightness at distance 'toLeft'
    float distance = length(toLight);

    if (distance < 1e-15) {
        outputColor = vec4(1f, 1f, 1f, 1f);
    }

    float brightness = (distance < 2f ? 2.5f : 10f / (distance * distance));
    // float brightness = 1f;

    // Calculate the angle of incidence for diffuse shading
    float cosAlpha = max(dot(normalize(toLight), normal), 0f);

    outputColor = vec4(clamp(brightness * cosAlpha * lightcolor * color, 0f, 1f), 1f);
}
