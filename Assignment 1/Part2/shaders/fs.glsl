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

    if (distance < 1e-4) {
        // In the case of the light source, we want a white dot.
        outputColor = vec4(lightcolor, 1f);
        return;
    }

    float brightness = (distance < 1f ? 3f : 3f / (distance * distance));

    // Calculate the angle of incidence for diffuse shading
    float cosAlpha = dot(normalize(toLight), normal);

    if (cosAlpha <= 0f) {
        // face is seen from behind
        outputColor = vec4(0f, 0f, 0f, 1f);
        return;
    } else {
        // face is seen from the right side
        outputColor = vec4(clamp(brightness * cosAlpha * lightcolor * color, 0f, 1f), 1f);
    }
}
