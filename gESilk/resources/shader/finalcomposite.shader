#version 330 core

layout (location = 0) in vec2 inPos;
layout (location = 2) in vec2 inTexCoords;

out vec2 TexCoord;

void main()
{
    gl_Position = vec4(inPos.x, inPos.y, 0.0, 1.0);
    TexCoord = inTexCoords;
}
    #FRAGMENT
    #version  330 core

out vec4 FragColor;

in vec2 TexCoord;

const float gamma = 2.2;
const float exposure = 1;

uniform sampler2D screenTexture;
uniform sampler2D ao;
uniform sampler2D bloom;

void main()
{
    


    vec4 ScreenTex = texture(screenTexture, TexCoord);
    vec3 hdrColor = ScreenTex.rgb + texture(bloom, TexCoord).rgb;

    // exposure tone mapping
    vec3 mapped = vec3(1.0) - exp(-hdrColor * exposure);
    // gamma correction 
    mapped = pow(mapped, vec3(1.0 / gamma));
    float aoData = texture(ao,TexCoord).r;
    FragColor = vec4(mapped * aoData, 1.0);
}