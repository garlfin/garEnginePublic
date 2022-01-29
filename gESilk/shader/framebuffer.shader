#version 330 core

layout (location = 0) in vec2 inPos;
layout (location = 2) in vec2 inTexCoords;

out vec2 texCoords;

void main()
{
    gl_Position = vec4(inPos.x, inPos.y, 0.0, 1.0);
    texCoords = inTexCoords;
}
    #FRAGMENT
    #version  330 core

out vec4 FragColor;

in vec2 texCoords;

uniform sampler2D screenTexture;


void main()
{
    FragColor = texture(screenTexture, texCoords);
}