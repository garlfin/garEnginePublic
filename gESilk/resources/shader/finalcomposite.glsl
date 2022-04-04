#version 330 core

layout (location = 0) in vec2 inPos;
layout (location = 2) in vec2 inTexCoords;

out vec2 TexCoord;

void main()
{
    gl_Position = vec4(inPos.x, inPos.y, 0.0, 1.0);
    TexCoord = inTexCoords;
}

//-FRAGMENT-

#version  330 core

out vec4 FragColor;

in vec2 TexCoord;

const float gamma = 2.2;
const float exposure = 0.8;

uniform sampler2D screenTexture;
uniform sampler2D ao;
uniform sampler2D bloom;
uniform float bloomIntensity = 0.8;

const vec2 u_texelStep = vec2(1.0/1280, 1.0/720);

const float u_mulReduce = 9.0;
const float u_minReduce = 128.0;
const float u_maxSpan = 8.0;

void main()
{
    vec3 ScreenTex = texture(screenTexture, TexCoord).rgb;

    float aoData = texture(ao, TexCoord).r;
    vec3 hdrColor = ScreenTex + (texture(bloom, TexCoord).rgb * bloomIntensity) * aoData;

    // exposure tone mapping
    vec3 mapped = vec3(1.0) - exp(-hdrColor);
    // gamma correction 
    hdrColor = pow(hdrColor, vec3(1.0 / 2.2));
    
    FragColor = vec4(hdrColor, 1.0);
}